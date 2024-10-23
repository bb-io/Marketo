using Apps.Marketo.Dtos;
using Apps.Marketo.Models.Emails.Requests;
using Apps.Marketo.Models.Emails.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;
using System.Globalization;
using Apps.Marketo.Invocables;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using System.Net.Mime;
using System.Text;
using Apps.Marketo.Models;
using Apps.Marketo.HtmlHelpers;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using HtmlAgilityPack;

namespace Apps.Marketo.Actions;

[ActionList]
public class EmailActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext)
{
    private const string HtmlIdAttribute = "id";
    private const string ContextImageAttribute = "data-blackbird-image";
    private const string BlackbirdEmailIdAttribute = "blackbird-email-id";

    [Action("Search emails", Description = "Search all emails")]
    public ListEmailsResponse ListEmails([ActionParameter] ListEmailsRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/emails.json", Method.Get, Credentials);
        var subfolders = AddFolderParameter(request, input.FolderId)?.ToList();

        if (input.Status != null) request.AddQueryParameter("status", input.Status);   
        if (input.EarliestUpdatedAt != null)
            request.AddQueryParameter("earliestUpdatedAt",
                ((DateTime)input.EarliestUpdatedAt).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
        if (input.LatestUpdatedAt != null)
            request.AddQueryParameter("latestUpdatedAt",
                ((DateTime)input.LatestUpdatedAt).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));

        var response = Client.Paginate<EmailDto>(request);
        response = input.NamePatterns != null ? response.Where(x => IsFilePathMatchingPattern(input.NamePatterns, x.Name, input.ExcludeMatched ?? false)).ToList() : response;
        return new() { Emails = response };
    }

    [Action("Get email info", Description = "Get email info")]
    public EmailDto GetEmailInfo([ActionParameter] GetEmailInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/email/{input.EmailId}.json", Method.Get, Credentials);
        return Client.GetSingleEntity<EmailDto>(request);
    }

    [Action("Update email metadata", Description = "Update email metadata")]
    public EmailDto UpdateEmailMetadata(
        [ActionParameter] GetEmailInfoRequest input,
        [ActionParameter] UpdateEmailMetadataRequest updateEmailMetadata)
    {
        var request = new MarketoRequest($"/rest/asset/v1/email/{input.EmailId}.json", Method.Post, Credentials);
        if (!string.IsNullOrEmpty(updateEmailMetadata.Name))
            request.AddParameter("name", updateEmailMetadata.Name);
        if (!string.IsNullOrEmpty(updateEmailMetadata.Description))
            request.AddParameter("description", updateEmailMetadata.Description);
        return Client.GetSingleEntity<EmailDto>(request);
    }

    [Action("Get email content", Description = "Get email content")]
    public EmailContentUserFriendlyResponse GetEmailContent([ActionParameter] GetEmailInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/email/{input.EmailId}/content.json", Method.Get, Credentials);
        var response = Client.ExecuteWithError<EmailContentDto>(request);
        return new(response.Result);
    }

    [Action("Delete email", Description = "Delete email")]
    public void DeleteEmail([ActionParameter] GetEmailInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/email/{input.EmailId}/delete.json", Method.Post, Credentials);
        Client.ExecuteWithError<IdDto>(request);
    }

    [Action("Get email as HTML for translation", Description = "Get email as HTML for translation")]
    public async Task<FileWrapper> GetEmailAsHtml(
        [ActionParameter] GetEmailInfoRequest getEmailInfoRequest,
        [ActionParameter] GetSegmentationRequest getSegmentationRequest,
        [ActionParameter] GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        [ActionParameter] GetEmailAsHtmlRequest getEmailAsHtmlRequest)
    {
        var emailInfo = GetEmailInfo(getEmailInfoRequest);
        var emailContentResponse = GetEmailContentAll(getEmailInfoRequest);
        var onlyDynamic = getEmailAsHtmlRequest.GetOnlyDynamicContent.HasValue && getEmailAsHtmlRequest.GetOnlyDynamicContent.Value;
        var includeImages = getEmailAsHtmlRequest.IncludeImages.HasValue && getEmailAsHtmlRequest.IncludeImages.Value;
        var sectionContent = emailContentResponse.EmailContentItems!
            .Where(x => x.ContentType == "DynamicContent" || (!onlyDynamic && x.ContentType == "Text") || (includeImages && x.ContentType == "Image"))
            .ToDictionary(
                x => x.HtmlId, 
                y => GetEmailSectionContent(getEmailInfoRequest, getSegmentationRequest, getSegmentBySegmentationRequest, y, includeImages))
            .Where(x => !string.IsNullOrEmpty(x.Value))
            .ToDictionary();

        if (emailInfo.Subject.Type == "Text")
        {
            sectionContent.Add("data-subject-value", emailInfo.Subject.Value);
        }
        else if (emailInfo.Subject.Type == "DynamicContent")
        {
            var subjectContent = GetEmailSectionContent(getEmailInfoRequest, getSegmentationRequest, getSegmentBySegmentationRequest, new EmailContentDto()
            {
                ContentType = "DynamicContent",
                Value = emailInfo.Subject.Value
            }, includeImages);
            sectionContent.Add("data-subject-value", subjectContent);
        }
        
        var resultHtml = HtmlContentBuilder.GenerateHtml(sectionContent, emailInfo.Name, getSegmentBySegmentationRequest.Segment, new(BlackbirdEmailIdAttribute, getEmailInfoRequest.EmailId));
        
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultHtml));
        var file = await fileManagementClient.UploadAsync(stream, MediaTypeNames.Text.Html, $"{emailInfo.Name}.html");
        return new() { File = file };
    }

    [Action("Translate email from HTML file", Description = "Translate email from HTML file")]
    public async Task<TranslateEmailWithHtmlResponse> TranslateEmailWithHtml(
        [ActionParameter] GetEmailInfoOptionalRequest getEmailInfoRequest,
        [ActionParameter] GetSegmentationRequest getSegmentationRequest,
        [ActionParameter] GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        [ActionParameter] TranslateEmailWithHtmlRequest translateEmailWithHtmlRequest)
    {
        var stream = await fileManagementClient.DownloadAsync(translateEmailWithHtmlRequest.File);
        var formBytes = await stream.GetByteData();
        var html = Encoding.UTF8.GetString(formBytes);
        
        var extractedMeta = HtmlContentBuilder.ExtractIdFromMeta(html, BlackbirdEmailIdAttribute);
        var translatedContent = HtmlContentBuilder.ParseHtml(html);

        var infoRequest = new GetEmailInfoRequest
        {
            EmailId = getEmailInfoRequest.EmailId ?? extractedMeta ?? throw new Exception("Email ID is not provided and not found in the HTML file. Please provide value in the optional input Email ID.") 
        };
        
        var emailContentResponse = GetEmailContentAll(infoRequest);

        if (!translateEmailWithHtmlRequest.TranslateOnlyDynamic.HasValue || 
            !translateEmailWithHtmlRequest.TranslateOnlyDynamic.Value)
        {
            foreach (var item in emailContentResponse.EmailContentItems)
            {
                if (item.ContentType == "Text")
                {
                    ConvertSectionToDynamicContent(infoRequest.EmailId, item.HtmlId, getSegmentationRequest.SegmentationId);
                }
            }
            emailContentResponse = GetEmailContentAll(infoRequest);
        }
        
        var updateEmailSubject = translateEmailWithHtmlRequest.UpdateEmailSubject.HasValue ? translateEmailWithHtmlRequest.UpdateEmailSubject.Value : true;
        if(translatedContent.TryGetValue("data-subject-value", out var subjectContent) && updateEmailSubject)
        {
            var emailInfo = GetEmailInfo(infoRequest);
            if (emailInfo.Subject?.Type == "Text" && translateEmailWithHtmlRequest.TranslateOnlyDynamic.HasValue
                                                  && !translateEmailWithHtmlRequest.TranslateOnlyDynamic.Value)
            {
                var subjectContentRequest = new MarketoRequest($"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/content.json", Method.Post, Credentials)
                    .AddParameter("subject", JsonConvert.SerializeObject(new
                    {
                        type = "Text",
                        value = subjectContent
                    }));
                try
                {
                    Client.GetSingleEntity<IdDto>(subjectContentRequest);
                }
                catch(BusinessRuleViolationException ex)
                {
                    return new()
                    {
                        Errors = new() { $"BusinessRuleViolationException, {ex.Message}, Error code: {ex.ErrorCode}, Content: {subjectContent}" }
                    };
                } 
            }
            else if (emailInfo.Subject?.Type == "DynamicContent")
            {
                var errorMessage = UpdateEmailDynamicContent(infoRequest, getSegmentationRequest, getSegmentBySegmentationRequest, new EmailContentDto()
                {
                    ContentType = "Text",
                    Value = emailInfo.Subject.Value
                }, subjectContent, emailContentResponse.EmailContentItems, translatedContent, 0, true, "Text");
                if(!string.IsNullOrEmpty(errorMessage))
                    return new()
                    {
                        Errors = new() { errorMessage }
                    };
            }
        }
        
        var modulesToIgnore = new List<string>();
        foreach (var item in emailContentResponse.EmailContentItems)
        {
            if (item.ContentType == "DynamicContent" && 
                !modulesToIgnore.Contains(item.ParentHtmlId) && 
                translatedContent.TryGetValue(item.HtmlId, out var translatedContentItem))
            {
                var ignoreModule = UpdateEmailDynamicContent(
                    infoRequest, getSegmentationRequest, getSegmentBySegmentationRequest, item, 
                    translatedContentItem, emailContentResponse.EmailContentItems, translatedContent, 0,
                    translateEmailWithHtmlRequest.RecreateCorruptedModules == null ? false : translateEmailWithHtmlRequest.RecreateCorruptedModules.Value,
                    updateStyle: translateEmailWithHtmlRequest.UpdateStyleForImages ?? false);
                modulesToIgnore.Add(ignoreModule);
            }
        }
        return new TranslateEmailWithHtmlResponse()
        {
            RecreateModules = modulesToIgnore.Where(x => !string.IsNullOrEmpty(x)).ToList(),
            Errors = modulesToIgnore.Where(x => !string.IsNullOrEmpty(x)).ToList(),
        };
    }

    private IdDto ConvertSectionToDynamicContent(string emailId, string htmlId, string segmentationId)
    {
        var endpoint = $"/rest/asset/v1/email/{emailId}/content/{htmlId}.json";
        var request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddParameter("value", segmentationId)
            .AddParameter("type", "DynamicContent");
        return Client.ExecuteWithError<IdDto>(request).Result.FirstOrDefault();
    }

    private string UpdateEmailDynamicContent(
        GetEmailInfoRequest getEmailInfoRequest,
        GetSegmentationRequest getSegmentationRequest,
        GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        EmailContentDto dynamicContentItem,
        string content,
        List<EmailContentDto> emailContentItems,
        Dictionary<string, string> translatedContent,
        int tryNumber,
        bool reacreateCorruptedModules,
        string? type = null,
        bool updateStyle = false)
    {
        var endpoint =
            $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{dynamicContentItem.Value}.json";

        RestRequest request = null;
        if (!content.Contains(ContextImageAttribute))
        {
            request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddQueryParameter("segment", getSegmentBySegmentationRequest.Segment)
            .AddQueryParameter("type", type ?? "HTML")
            .AddQueryParameter("value", content);
        }
        else
        {
            var htmlSnippet = new HtmlDocument();
            htmlSnippet.LoadHtml(content);
            var altTextAttribute = htmlSnippet.DocumentNode.FirstChild.Attributes["alt"];
            var styleAttribute = htmlSnippet.DocumentNode.FirstChild.Attributes["style"];
            var widthAttribute = htmlSnippet.DocumentNode.FirstChild.Attributes["width"];
            var heightAttribute = htmlSnippet.DocumentNode.FirstChild.Attributes["height"];
            var srcAttribute = htmlSnippet.DocumentNode.FirstChild.Attributes["src"];

            if (altTextAttribute == null)
                return string.Empty;
            var imageIdAttribute = htmlSnippet.DocumentNode.FirstChild.Attributes[ContextImageAttribute];

            request = new MarketoRequest(endpoint, Method.Post, Credentials)
            .AddQueryParameter("segment", getSegmentBySegmentationRequest.Segment)
            .AddQueryParameter("type", "Image")
            .AddQueryParameter("altText", altTextAttribute.Value);

            if (srcAttribute.Value == imageIdAttribute.Value)
                request.AddQueryParameter("externalUrl", srcAttribute.Value);
            else
                request.AddQueryParameter("value", imageIdAttribute.Value);


            if (styleAttribute != null && !string.IsNullOrWhiteSpace(styleAttribute.Value) && updateStyle)
            {
                request.AddQueryParameter("style", styleAttribute.Value);
            }
            
            if (widthAttribute != null && !string.IsNullOrWhiteSpace(widthAttribute.Value))
            {
                if(int.TryParse(widthAttribute.Value, out var width))
                {
                    request.AddQueryParameter("width", width);
                }
            }
            
            if (heightAttribute != null && !string.IsNullOrWhiteSpace(heightAttribute.Value))
            {
                if(int.TryParse(heightAttribute.Value, out var height))
                {
                    request.AddQueryParameter("height", height);
                }
            }
        }
        
        try
        {
            Client.GetSingleEntity<IdDto>(request);
            return string.Empty;
        }
        catch (BusinessRuleViolationException ex) 
        {
            if(ex.ErrorCode == 611 && tryNumber == 0 && reacreateCorruptedModules)
            {
                var ignoreModuleId = RecreateModuleWithIssue(getEmailInfoRequest, getSegmentationRequest, getSegmentBySegmentationRequest, dynamicContentItem, emailContentItems, translatedContent, tryNumber);
                return ignoreModuleId;
            }
            //throw ex;
            return $"BusinessRuleViolationException, {ex.Message}, Error code: {ex.ErrorCode}, ContentId: {dynamicContentItem.Value.ToString()}, Content: {content}";
        }
        catch(Exception ex)
        {
            return $"{ex.Message}, ContentId: {dynamicContentItem.Value}, Content: {content}";
        }
    }

    private string? GetEmailSectionContent(
        GetEmailInfoRequest getEmailInfoRequest,
        GetSegmentationRequest getSegmentationRequest,
        GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        EmailContentDto sectionContent,
        bool includeImages = false)
    {
        if (sectionContent.ContentType == "DynamicContent")
        {
            var requestSeg = new MarketoRequest(
                    $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{sectionContent.Value}.json",
                    Method.Get, Credentials);

            var responseSeg = Client.ExecuteWithError<DynamicContentDto<EmailImageSegmentDto>>(requestSeg);
            if (responseSeg.Result!.First().Segmentation.ToString() == getSegmentationRequest.SegmentationId)
            {
                var imageSegment = responseSeg.Result!.First().Content.Where(x => x.SegmentName == getSegmentBySegmentationRequest.Segment).FirstOrDefault();
                if (imageSegment != null && (imageSegment.Type == "File") && includeImages) // dynamic images
                {
                    var altTextAttribute = string.IsNullOrWhiteSpace(imageSegment.AltText) ? string.Empty : $" alt=\"{imageSegment.AltText}\"";
                    var imageIdAttribute = $" {ContextImageAttribute}=\"{imageSegment.Content}\"";
                    var widthAttribute = string.IsNullOrWhiteSpace(imageSegment.Width) ? string.Empty : $" width=\"{imageSegment.Width}\"";
                    var heightAttribute = string.IsNullOrWhiteSpace(imageSegment.Height) ? string.Empty : $" height=\"{imageSegment.Height}\"";
                    return $"<img src=\"{imageSegment.ContentUrl}\" style=\"{imageSegment.Style}\"{altTextAttribute}{imageIdAttribute}{widthAttribute}{heightAttribute}>";
                }
                else if(imageSegment != null && (imageSegment.Type == "Image") && includeImages)
                {
                    var altTextAttribute = string.IsNullOrWhiteSpace(imageSegment.AltText) ? string.Empty : $" alt=\"{imageSegment.AltText}\"";
                    var imageIdAttribute = $" {ContextImageAttribute}=\"{imageSegment.Content}\"";
                    var widthAttribute = string.IsNullOrWhiteSpace(imageSegment.Width) ? string.Empty : $" width=\"{imageSegment.Width}\"";
                    var heightAttribute = string.IsNullOrWhiteSpace(imageSegment.Height) ? string.Empty : $" height=\"{imageSegment.Height}\"";
                    return $"<img src=\"{imageSegment.Content}\" style=\"{imageSegment.Style}\"{altTextAttribute}{imageIdAttribute}{widthAttribute}{heightAttribute}>";
                }
                else if(imageSegment != null && imageSegment.Type == "Text")
                {
                    return imageSegment.Content;
                }
                else 
                {
                    return responseSeg.Result!.First().Content
                    .Where(x => x.Type == "HTML" && x.SegmentName == getSegmentBySegmentationRequest.Segment)
                    .Select(x => new GetEmailDynamicContentResponse(x)
                    { DynamicContentId = sectionContent.Value.ToString()! }).FirstOrDefault()?.Content;
                }
            }
                
        }
        else if(sectionContent.ContentType == "Text") // Static text
        {
            return JsonConvert.DeserializeObject<List<EmailContentValueDto>>(sectionContent.Value.ToString()).First(x => x.Type == "HTML").Value;
        }
        else if(sectionContent.ContentType == "Image") // Static images
        {
            var imageDto = JsonConvert.DeserializeObject<ImageDto>(sectionContent.Value.ToString());
            var imageUrl = string.IsNullOrWhiteSpace(imageDto.ContentUrl) ? imageDto.Value : imageDto.ContentUrl;
            var altTextAttribute = string.IsNullOrWhiteSpace(imageDto.AltText) ? "" : $" alt=\"{imageDto.AltText}\"";
            return $"<img src=\"{imageUrl}\" style=\"{imageDto.Style}\"{altTextAttribute}>";
        }
        return string.Empty;
    }

    private EmailContentResponse GetEmailContentAll([ActionParameter] GetEmailInfoRequest input)
    {
        var request = new MarketoRequest($"/rest/asset/v1/email/{input.EmailId}/content.json", Method.Get, Credentials);
        var response = Client.ExecuteWithError<EmailContentDto>(request);
        return new(response.Result);
    }

    private string RecreateModuleWithIssue(
        GetEmailInfoRequest getEmailInfoRequest,
        GetSegmentationRequest getSegmentationRequest,
        GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
        EmailContentDto dynamicContentItem, 
        List<EmailContentDto> emailContentItems,
        Dictionary<string, string> translatedContent,
        int tryNumber)
    {
        var parentModule = emailContentItems.FirstOrDefault(x => x.HtmlId == dynamicContentItem.ParentHtmlId);
        var oldItemsInModule = emailContentItems.Where(x => x.ParentHtmlId == dynamicContentItem.ParentHtmlId).ToList();

        var deleteModuleRequest = new MarketoRequest($"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/content/{dynamicContentItem.ParentHtmlId}/delete.json", Method.Post, Credentials);
        var deleteModuleResponse = Client.GetSingleEntity<IdDto>(deleteModuleRequest);

        var addNewModuleRequest = new MarketoRequest($"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/content/{dynamicContentItem.ParentHtmlId}/add.json", Method.Post, Credentials);
        addNewModuleRequest.AddQueryParameter("index", parentModule.Index);
        var addNewModuleResponse = Client.GetSingleEntity<IdDto>(addNewModuleRequest);

        
        var emailContentResponse = GetEmailContentAll(getEmailInfoRequest);

        var itemsInNewModule = emailContentResponse.EmailContentItems.Where(x => !string.IsNullOrEmpty(x.ParentHtmlId) && x.ParentHtmlId.Length > 36 && parentModule.HtmlId.Contains(x.ParentHtmlId.Remove(x.ParentHtmlId.Length - 36, 36))).ToList();
        var textItemsIds = new List<string>();
        foreach (var item in itemsInNewModule)
        {
            var idWithoutGuid = item.HtmlId.Remove(item.HtmlId.Length - 36, 36);
            var oldModuleItem = oldItemsInModule.FirstOrDefault(x => x.HtmlId.Contains(idWithoutGuid));
            if (item.ContentType == "Text" && oldModuleItem  != null && oldModuleItem.ContentType == "DynamicContent")
            {
                ConvertSectionToDynamicContent(getEmailInfoRequest.EmailId, item.HtmlId, getSegmentationRequest.SegmentationId);
                textItemsIds.Add(item.HtmlId);       
            }
            else if(item.ContentType != "DynamicContent" && oldModuleItem.ContentType != "DynamicContent")
            {
                var updateModuleItemRequest = new MarketoRequest($"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/content/{item.HtmlId}.json", Method.Post, Credentials);
                updateModuleItemRequest.AddQueryParameter("type", oldModuleItem.ContentType);
                updateModuleItemRequest.AddQueryParameter("value", oldModuleItem.Value.ToString());
                var updateModuleItemResponse = Client.GetSingleEntity<IdDto>(updateModuleItemRequest);
            }               
        }

        emailContentResponse = GetEmailContentAll(getEmailInfoRequest);
        itemsInNewModule = emailContentResponse.EmailContentItems.Where(x => textItemsIds.Contains(x.HtmlId)).ToList();

        foreach(var dynamicItem in itemsInNewModule)
        {
            var idWithoutGuid = dynamicItem.HtmlId.Remove(dynamicItem.HtmlId.Length - 36, 36);
            if(translatedContent.TryGetValue(translatedContent.Keys.FirstOrDefault(x => x.Contains(idWithoutGuid)) ?? "", out var translatedContentItem))
            {
                UpdateEmailDynamicContent(getEmailInfoRequest, getSegmentationRequest, getSegmentBySegmentationRequest, dynamicItem, translatedContentItem, null, null, ++tryNumber, false);
            }
        }
        return parentModule.HtmlId;
    }

    // Deprecated
    // Actions for more general usage of dynamic content

    //[Action("Update email dynamic content", Description = "Update email dynamic content")]
    //public IdDto UpdateEmailDynamicContent([ActionParameter] GetEmailInfoRequest getEmailInfoRequest,
    //    [ActionParameter] GetEmailDynamicItemRequest getEmailDynamicItemRequest,
    //    [ActionParameter] GetSegmentationRequest getSegmentationRequest,
    //    [ActionParameter] GetSegmentBySegmentationRequest getSegmentBySegmentationRequest,
    //    [ActionParameter] UpdateEmailDynamicContentRequest updateEmailDynamicContentRequest)
    //{
    //    var endpoint =
    //        $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{getEmailDynamicItemRequest.DynamicContentId}.json";
    //    var request = new MarketoRequest(endpoint, Method.Post, Credentials)
    //        .AddQueryParameter("segment", getSegmentBySegmentationRequest.Segment)
    //        .AddQueryParameter("type", "HTML")
    //        .AddQueryParameter("value", updateEmailDynamicContentRequest.HTMLContent);

    //    return Client.GetSingleEntity<IdDto>(request);
    //}

    [Action("Get email dynamic content", Description = "Get email dynamic content")]
    public DynamicContentDto<EmailBaseSegmentDto> GetEmailDynamicContent(
        [ActionParameter] GetEmailInfoRequest getEmailInfoRequest,
        [ActionParameter] GetEmailDynamicItemRequest getEmailDynamicItemRequest,
        [ActionParameter] GetEmailSegmentRequest getSegmentRequest)
    {
        var endpoint =
            $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{getEmailDynamicItemRequest.DynamicContentId}.json";
        var request = new MarketoRequest(endpoint, Method.Get, Credentials);
        return Client.GetSingleEntity<DynamicContentDto<EmailBaseSegmentDto>>(request);
    }

    [Action("Get email dynamic image content", Description = "Get email dynamic image content")]
    public DynamicContentDto<EmailImageSegmentDto> GetEmailDynamicImageContent(
       [ActionParameter] GetEmailInfoRequest getEmailInfoRequest,
       [ActionParameter] GetEmailDynamicItemRequest getEmailDynamicItemRequest,
       [ActionParameter] GetEmailSegmentRequest getSegmentRequest)
    {
        var endpoint =
            $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{getEmailDynamicItemRequest.DynamicContentId}.json";
        var request = new MarketoRequest(endpoint, Method.Get, Credentials);
        return Client.GetSingleEntity<DynamicContentDto<EmailImageSegmentDto>>(request);
    }

    //[Action("List email dynamic content", Description = "List email dynamic content by segmentation")]
    //public ListEmailDynamicContentResponse ListEmailDynamicContent(
    //    [ActionParameter] GetEmailInfoRequest getEmailInfoRequest,
    //    [ActionParameter] GetSegmentationRequest getSegmentationRequest,
    //    [ActionParameter] GetSegmentBySegmentationRequest getSegmentBySegmentationRequest)
    //{
    //    var endpoint = $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/content.json";
    //    var request = new MarketoRequest(endpoint, Method.Get, Credentials)
    //        .AddQueryParameter("maxReturn", 200);

    //    var response = Client.ExecuteWithError<EmailContentDto>(request);
    //    var allDynamicContentInfo = response.Result.Where(e => e.ContentType == "DynamicContent").ToList();

    //    var result = new List<GetEmailDynamicContentResponse>();
    //    foreach (var dynamicContentInfo in allDynamicContentInfo)
    //    {
    //        var requestSeg =
    //            new MarketoRequest(
    //                $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{dynamicContentInfo.Value.ToString()}.json",
    //                Method.Get, Credentials);
    //        var responseSeg = Client.ExecuteWithError<DynamicContentDto>(requestSeg);
    //        if (responseSeg.Result!.First().Segmentation.ToString() == getSegmentationRequest.SegmentationId)
    //            result.Add(responseSeg.Result!.First().Content
    //                .Where(x => x.Type == "HTML" && x.SegmentName == getSegmentBySegmentationRequest.Segment)
    //                .Select(x => new GetEmailDynamicContentResponse(x)
    //                    { DynamicContentId = dynamicContentInfo.Value.ToString()! }).FirstOrDefault()!);
    //    }
    //    return new() { EmailDynamicContentList = result };
    //}

    //[Action("Update email content", Description = "Update content of a specific email")]
    //public void UpdateEmailContent(
    //    [ActionParameter] GetEmailInfoRequest emailRequest,
    //    [ActionParameter] UpdateContentRequest input)
    //{
    //    var request = new MarketoRequest($"/rest/asset/v1/email/{emailRequest.EmailId}/content.json", Method.Post,
    //            Credentials)
    //        .AddParameter("type", input.Type)
    //        .AddParameter("content", input.Content);

    //    Client.ExecuteWithErrorHandling(request);
    //}
}