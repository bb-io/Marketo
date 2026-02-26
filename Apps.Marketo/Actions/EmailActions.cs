using Apps.Marketo.Constants;
using Apps.Marketo.Dtos;
using Apps.Marketo.Dtos.Email;
using Apps.Marketo.Extensions;
using Apps.Marketo.Helper;
using Apps.Marketo.Helper.Filter;
using Apps.Marketo.HtmlHelpers;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models;
using Apps.Marketo.Models.Emails.Requests;
using Apps.Marketo.Models.Emails.Responses;
using Apps.Marketo.Models.Entities.Email;
using Apps.Marketo.Models.Identifiers;
using Apps.Marketo.Models.Identifiers.Optional;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;
using System.Net.Mime;
using System.Text;

namespace Apps.Marketo.Actions;

[ActionList("Emails")]
public class EmailActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : MarketoInvocable(invocationContext)
{
    [Action("Search emails", Description = "Search all emails")]
    public async Task<SearchEmailsResponse> ListEmails([ActionParameter] SearchEmailsRequest input)
    {
        var request = new RestRequest($"/rest/asset/v1/emails.json", Method.Get);
        var subfolders = await FileFolderHelper.AddFolderParameter(Client, request, input.FolderId);
        request.AddQueryParameterIfNotNull("status", input.Status);           
        request.AddQueryParameterIfNotNull("earliestUpdatedAt", input.UpdatedAfter);
        request.AddQueryParameterIfNotNull("latestUpdatedAt", input.UpdatedBefore);

        var emails = await Client.Paginate<EmailEntity>(request);
        emails = emails.ApplyNamePatternFilter(input.NamePatterns, input.ExcludeMatched);
        emails = emails.ApplyCreatedAtFilter(input.CreatedAfter, input.CreatedBefore);
        emails = await emails.ApplyIgnoreInArchiveFilter(Client, input.IgnoreInArchive);

        return new(emails.Select(x => new EmailDto(x)).ToList());
    }

    [Action("Get email info", Description = "Get email info")]
    public async Task<EmailDto> GetEmailInfo([ActionParameter] EmailIdentifier input)
    {
        var request = new RestRequest($"/rest/asset/v1/email/{input.EmailId}.json", Method.Get);
        var result = await Client.ExecuteWithErrorHandlingFirst<EmailEntity>(request);
        return new(result);
    }

    [Action("Update email metadata", Description = "Update email metadata")]
    public async Task<EmailDto> UpdateEmailMetadata(
        [ActionParameter] EmailIdentifier input,
        [ActionParameter] UpdateEmailMetadataRequest updateEmailMetadata)
    {
        var request = new RestRequest($"/rest/asset/v1/email/{input.EmailId}.json", Method.Post);
        request.AddParameterIfNotNull("name", updateEmailMetadata.Name);
        request.AddParameterIfNotNull("description", updateEmailMetadata.Description);

        var result = await Client.ExecuteWithErrorHandlingFirst<EmailEntity>(request);
        return new(result);
    }

    [Action("Get email content", Description = "Get email content")]
    public async Task<EmailContentUserFriendlyResponse> GetEmailContent([ActionParameter] EmailIdentifier input)
    {
        var request = new RestRequest($"/rest/asset/v1/email/{input.EmailId}/content.json", Method.Get);
        var response = await Client.ExecuteWithErrorHandling<EmailContentDto>(request);
        return new(response.ToList());
    }

    [Action("Delete email", Description = "Delete email")]
    public async Task DeleteEmail([ActionParameter] EmailIdentifier input)
    {
        var request = new RestRequest($"/rest/asset/v1/email/{input.EmailId}/delete.json", Method.Post);
        await Client.ExecuteWithErrorHandling<IdDto>(request);
    }

    [Action("Get email as HTML for translation", Description = "Get email as HTML for translation")]
    public async Task<FileWrapper> GetEmailAsHtml(
        [ActionParameter] EmailIdentifier emailInput,
        [ActionParameter] SegmentationIdentifier getSegmentationRequest,
        [ActionParameter] SegmentIdentifier getSegmentBySegmentationRequest,
        [ActionParameter] GetEmailAsHtmlRequest getEmailAsHtmlRequest)
    {
        var emailInfoRequest = new RestRequest($"/rest/asset/v1/email/{emailInput.EmailId}.json", Method.Post);
        var emailInfo = await Client.ExecuteWithErrorHandlingFirst<EmailEntity>(emailInfoRequest);
        var emailContentResponse = await GetEmailContentAll(emailInput);
        var onlyDynamic = getEmailAsHtmlRequest.GetOnlyDynamicContent.HasValue && getEmailAsHtmlRequest.GetOnlyDynamicContent.Value;
        var includeImages = getEmailAsHtmlRequest.IncludeImages.HasValue && getEmailAsHtmlRequest.IncludeImages.Value;
        
        var targetItems = emailContentResponse.EmailContentItems!
            .Where(x => x.ContentType == "DynamicContent"
                     || (!onlyDynamic && x.ContentType == "Text")
                     || (includeImages && x.ContentType == "Image"))
            .ToList();

        var contentTasks = targetItems.Select(async item =>
        {
            var content = await GetEmailSectionContent(
                emailInput,
                getSegmentationRequest,
                getSegmentBySegmentationRequest,
                item,
                includeImages);

            return new KeyValuePair<string, string>(item.HtmlId, content);
        });

        var contentResults = await Task.WhenAll(contentTasks);
        var sectionContent = contentResults
            .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (emailInfo.Subject.Type == "Text")
        {
            sectionContent.Add("data-subject-value", emailInfo.Subject.Value);
        }
        else if (emailInfo.Subject.Type == "DynamicContent")
        {
            var subjectContent = await GetEmailSectionContent(emailInput, getSegmentationRequest, getSegmentBySegmentationRequest, new EmailContentDto()
            {
                ContentType = "DynamicContent",
                Value = emailInfo.Subject.Value
            }, includeImages);
            sectionContent.Add("data-subject-value", subjectContent);
        }
        
        var resultHtml = HtmlContentBuilder.GenerateHtml(
            sectionContent, 
            emailInfo.Name, 
            getSegmentBySegmentationRequest.Segment, 
            new(MetadataConstants.BlackbirdEmailIdAttribute, emailInput.EmailId));
        
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultHtml));
        var file = await fileManagementClient.UploadAsync(stream, MediaTypeNames.Text.Html, $"{emailInfo.Name}.html");
        return new() { File = file };
    }

    [Action("Translate email from HTML file", Description = "Translate email from HTML file")]
    public async Task<TranslateEmailWithHtmlResponse> TranslateEmailWithHtml(
        [ActionParameter] OptionalEmailIdenfitier getEmailInfoRequest,
        [ActionParameter] SegmentationIdentifier getSegmentationRequest,
        [ActionParameter] SegmentIdentifier getSegmentBySegmentationRequest,
        [ActionParameter] TranslateEmailWithHtmlRequest translateEmailWithHtmlRequest)
    {
        var stream = await fileManagementClient.DownloadAsync(translateEmailWithHtmlRequest.File);
        var formBytes = await stream.GetByteData();
        var html = Encoding.UTF8.GetString(formBytes);
        
        var extractedMeta = HtmlContentBuilder.ExtractIdFromMeta(html, MetadataConstants.BlackbirdEmailIdAttribute);
        var translatedContent = HtmlContentBuilder.ParseHtml(html);

        var infoRequest = new EmailIdentifier
        {
            EmailId = 
                getEmailInfoRequest.EmailId ?? 
                extractedMeta ?? 
                throw new PluginMisconfigurationException(
                    "Email ID is not provided and not found in the HTML file. " +
                    "Please provide value in the optional input Email ID."
                ) 
        };
        
        var emailContentResponse = await GetEmailContentAll(infoRequest);

        if (!translateEmailWithHtmlRequest.TranslateOnlyDynamic.HasValue || 
            !translateEmailWithHtmlRequest.TranslateOnlyDynamic.Value)
        {
            foreach (var item in emailContentResponse.EmailContentItems)
            {
                if (item.ContentType == "Text")
                {
                    await ConvertSectionToDynamicContent(infoRequest.EmailId, item.HtmlId, getSegmentationRequest.SegmentationId);
                }
            }
            emailContentResponse = await GetEmailContentAll(infoRequest);
        }
        
        var updateEmailSubject = translateEmailWithHtmlRequest.UpdateEmailSubject.HasValue ? translateEmailWithHtmlRequest.UpdateEmailSubject.Value : true;
        if(translatedContent.TryGetValue("data-subject-value", out var subjectContent) && updateEmailSubject)
        {
            var emailInfoRequest = new RestRequest($"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}.json", Method.Post);
            var emailInfo = await Client.ExecuteWithErrorHandlingFirst<EmailEntity>(emailInfoRequest);
            if (emailInfo.Subject?.Type == "Text" && translateEmailWithHtmlRequest.TranslateOnlyDynamic.HasValue
                                                  && !translateEmailWithHtmlRequest.TranslateOnlyDynamic.Value)
            {
                var subjectContentRequest = new RestRequest($"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/content.json", Method.Post)
                    .AddParameter("subject", JsonConvert.SerializeObject(new
                    {
                        type = "Text",
                        value = subjectContent
                    }));

                await Client.ExecuteWithErrorHandlingFirst<IdDto>(subjectContentRequest);
            }
            else if (emailInfo.Subject?.Type == "DynamicContent")
            {
                var errorMessage = await UpdateEmailDynamicContent(infoRequest, getSegmentationRequest, getSegmentBySegmentationRequest, new EmailContentDto()
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
                var ignoreModule = await UpdateEmailDynamicContent(
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

    private async Task<IdDto> ConvertSectionToDynamicContent(string emailId, string htmlId, string segmentationId)
    {
        var endpoint = $"/rest/asset/v1/email/{emailId}/content/{htmlId}.json";
        var request = new RestRequest(endpoint, Method.Post)
            .AddParameter("value", segmentationId)
            .AddParameter("type", "DynamicContent");
        return await Client.ExecuteWithErrorHandlingFirst<IdDto>(request);
    }

    private async Task<string> UpdateEmailDynamicContent(
        EmailIdentifier getEmailInfoRequest,
        SegmentationIdentifier getSegmentationRequest,
        SegmentIdentifier getSegmentBySegmentationRequest,
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

        RestRequest request;
        if (!content.Contains(MetadataConstants.ContextImageAttribute))
        {
            request = new RestRequest(endpoint, Method.Post)
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
            var imageIdAttribute = htmlSnippet.DocumentNode.FirstChild.Attributes[MetadataConstants.ContextImageAttribute];

            request = new RestRequest(endpoint, Method.Post)
                .AddQueryParameter("segment", getSegmentBySegmentationRequest.Segment)
                .AddQueryParameter("type", "Image")
                .AddQueryParameter("altText", altTextAttribute.Value);

            if (srcAttribute.Value == imageIdAttribute.Value)
                request.AddQueryParameter("externalUrl", srcAttribute.Value.Replace(" ", "+"));
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
        
        var result = await Client.ExecuteNoErrorHandling<IdDto>(request);
        if (result != null)
        {
            if (result.Errors.Any(x => x.Code == "611") && tryNumber == 0 && reacreateCorruptedModules)
            {
                var ignoreModuleId = await RecreateModuleWithIssue(getEmailInfoRequest, getSegmentationRequest, getSegmentBySegmentationRequest, dynamicContentItem, emailContentItems, translatedContent, tryNumber);
                return ignoreModuleId;
            }

            var errorMessages = result.Errors.Select(x => x.Message).ToList();
            string errorMessage = string.Join("; ", errorMessages);
            return $"{errorMessage}, ContentId: {dynamicContentItem.Value.ToString()}, Content: {content}";
        }

        return string.Empty;
    }

    private async Task<string?> GetEmailSectionContent(
        EmailIdentifier getEmailInfoRequest,
        SegmentationIdentifier getSegmentationRequest,
        SegmentIdentifier getSegmentBySegmentationRequest,
        EmailContentDto sectionContent,
        bool includeImages = false)
    {
        if (sectionContent.ContentType == "DynamicContent")
        {
            var requestSeg = new RestRequest(
                $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{sectionContent.Value}.json",
                Method.Get);

            var responseSeg = await Client.ExecuteWithErrorHandlingFirst<DynamicContentDto<EmailImageSegmentDto>>(requestSeg);
            if (responseSeg.Segmentation.ToString() == getSegmentationRequest.SegmentationId)
            {
                var imageSegment = responseSeg.Content.Where(x => x.SegmentName == getSegmentBySegmentationRequest.Segment).FirstOrDefault();
                if (imageSegment != null && (imageSegment.Type == "File") && includeImages) // dynamic images
                {
                    var altTextAttribute = string.IsNullOrWhiteSpace(imageSegment.AltText) ? string.Empty : $" alt=\"{imageSegment.AltText}\"";
                    var imageIdAttribute = $" {MetadataConstants.ContextImageAttribute}=\"{imageSegment.Content}\"";
                    var widthAttribute = string.IsNullOrWhiteSpace(imageSegment.Width) ? string.Empty : $" width=\"{imageSegment.Width}\"";
                    var heightAttribute = string.IsNullOrWhiteSpace(imageSegment.Height) ? string.Empty : $" height=\"{imageSegment.Height}\"";
                    return $"<img src=\"{imageSegment.ContentUrl}\" style=\"{imageSegment.Style}\"{altTextAttribute}{imageIdAttribute}{widthAttribute}{heightAttribute}>";
                }
                else if(imageSegment != null && (imageSegment.Type == "Image") && includeImages)
                {
                    var altTextAttribute = string.IsNullOrWhiteSpace(imageSegment.AltText) ? string.Empty : $" alt=\"{imageSegment.AltText}\"";
                    var imageIdAttribute = $" {MetadataConstants.ContextImageAttribute}=\"{imageSegment.Content}\"";
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
                    return responseSeg.Content
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

    private async Task<EmailContentResponse> GetEmailContentAll([ActionParameter] EmailIdentifier input)
    {
        var request = new RestRequest($"/rest/asset/v1/email/{input.EmailId}/content.json", Method.Get);
        var response = await Client.ExecuteWithErrorHandling<EmailContentDto>(request);
        return new(response.ToList());
    }

    private async Task<string> RecreateModuleWithIssue(
        EmailIdentifier getEmailInfoRequest,
        SegmentationIdentifier getSegmentationRequest,
        SegmentIdentifier getSegmentBySegmentationRequest,
        EmailContentDto dynamicContentItem, 
        List<EmailContentDto> emailContentItems,
        Dictionary<string, string> translatedContent,
        int tryNumber)
    {
        var parentModule = emailContentItems.FirstOrDefault(x => x.HtmlId == dynamicContentItem.ParentHtmlId);
        var oldItemsInModule = emailContentItems.Where(x => x.ParentHtmlId == dynamicContentItem.ParentHtmlId).ToList();

        var deleteModuleRequest = new RestRequest($"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/content/{dynamicContentItem.ParentHtmlId}/delete.json", Method.Post);
        var deleteModuleResponse = await Client.ExecuteWithErrorHandlingFirst<IdDto>(deleteModuleRequest);

        var addNewModuleRequest = new RestRequest($"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/content/{dynamicContentItem.ParentHtmlId}/add.json", Method.Post);
        addNewModuleRequest.AddQueryParameter("index", parentModule.Index);
        var addNewModuleResponse = await Client.ExecuteWithErrorHandlingFirst<IdDto>(addNewModuleRequest);

        
        var emailContentResponse = await GetEmailContentAll(getEmailInfoRequest);

        var itemsInNewModule = emailContentResponse.EmailContentItems.Where(x => !string.IsNullOrEmpty(x.ParentHtmlId) && x.ParentHtmlId.Length > 36 && parentModule.HtmlId.Contains(x.ParentHtmlId.Remove(x.ParentHtmlId.Length - 36, 36))).ToList();
        var textItemsIds = new List<string>();
        foreach (var item in itemsInNewModule)
        {
            var idWithoutGuid = item.HtmlId.Remove(item.HtmlId.Length - 36, 36);
            var oldModuleItem = oldItemsInModule.FirstOrDefault(x => x.HtmlId.Contains(idWithoutGuid));
            if (item.ContentType == "Text" && oldModuleItem  != null && oldModuleItem.ContentType == "DynamicContent")
            {
                await ConvertSectionToDynamicContent(getEmailInfoRequest.EmailId, item.HtmlId, getSegmentationRequest.SegmentationId);
                textItemsIds.Add(item.HtmlId);       
            }
            else if(item.ContentType != "DynamicContent" && oldModuleItem.ContentType != "DynamicContent")
            {
                var updateModuleItemRequest = new RestRequest($"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/content/{item.HtmlId}.json", Method.Post);
                updateModuleItemRequest.AddQueryParameter("type", oldModuleItem.ContentType);
                updateModuleItemRequest.AddQueryParameter("value", oldModuleItem.Value.ToString());
                var updateModuleItemResponse = await Client.ExecuteWithErrorHandlingFirst<IdDto>(updateModuleItemRequest);
            }               
        }

        emailContentResponse = await GetEmailContentAll(getEmailInfoRequest);
        itemsInNewModule = emailContentResponse.EmailContentItems.Where(x => textItemsIds.Contains(x.HtmlId)).ToList();

        foreach(var dynamicItem in itemsInNewModule)
        {
            var idWithoutGuid = dynamicItem.HtmlId.Remove(dynamicItem.HtmlId.Length - 36, 36);
            if(translatedContent.TryGetValue(translatedContent.Keys.FirstOrDefault(x => x.Contains(idWithoutGuid)) ?? "", out var translatedContentItem))
            {
                await UpdateEmailDynamicContent(getEmailInfoRequest, getSegmentationRequest, getSegmentBySegmentationRequest, dynamicItem, translatedContentItem, null, null, ++tryNumber, false);
            }
        }
        return parentModule.HtmlId;
    }

    [Action("Get email dynamic content", Description = "Get email dynamic content")]
    public async Task<DynamicContentDto<EmailBaseSegmentDto>> GetEmailDynamicContent(
        [ActionParameter] EmailIdentifier getEmailInfoRequest,
        [ActionParameter] GetEmailDynamicItemRequest getEmailDynamicItemRequest,
        [ActionParameter] GetEmailSegmentRequest getSegmentRequest)
    {
        var endpoint =
            $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{getEmailDynamicItemRequest.DynamicContentId}.json";
        var request = new RestRequest(endpoint, Method.Get);
        return await Client.ExecuteWithErrorHandlingFirst<DynamicContentDto<EmailBaseSegmentDto>>(request);
    }

    [Action("Get email dynamic image content", Description = "Get email dynamic image content")]
    public async Task<DynamicContentDto<EmailImageSegmentDto>> GetEmailDynamicImageContent(
       [ActionParameter] EmailIdentifier getEmailInfoRequest,
       [ActionParameter] GetEmailDynamicItemRequest getEmailDynamicItemRequest,
       [ActionParameter] GetEmailSegmentRequest getSegmentRequest)
    {
        var endpoint =
            $"/rest/asset/v1/email/{getEmailInfoRequest.EmailId}/dynamicContent/{getEmailDynamicItemRequest.DynamicContentId}.json";
        var request = new RestRequest(endpoint, Method.Get);
        return await Client.ExecuteWithErrorHandlingFirst<DynamicContentDto<EmailImageSegmentDto>>(request);
    }
}