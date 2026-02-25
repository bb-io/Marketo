using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.EmailTemplates.Requests;
using Apps.Marketo.Models.EmailTemplates.Response;
using Apps.Marketo.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;
using System.Text;

namespace Apps.Marketo.Actions;

[ActionList("Email templates")]
public class EmailTemplateActions(InvocationContext invocationContext) : MarketoInvocable(invocationContext)
{
    [Action("Search email templates", Description = "Search all email templates")]
    public async Task<ListEmailTemplatesResponse> ListEmailTemplates([ActionParameter] ListEmailTemplatesRequest input)
    {
        var request = new RestRequest($"/rest/asset/v1/emailTemplates.json", Method.Get);

        if (!string.IsNullOrEmpty(input.Status)) request.AddQueryParameter("status", input.Status);
        var response = (await Client.Paginate<EmailTemplateDto>(request)).ToList();

        if (string.IsNullOrEmpty(input.Status)) 
        {
            var requestApproved = new RestRequest($"/rest/asset/v1/emailTemplates.json", Method.Get);
            requestApproved.AddQueryParameter("status", "approved");
            var approvedTemplates = await Client.Paginate<EmailTemplateDto>(requestApproved);
            response.AddRange(approvedTemplates);
            response = response.DistinctBy(x => x.Id).ToList();
        } 

        return new(response);
    }

    [Action("Get email template info", Description = "Get email template info")]
    public async Task<EmailTemplateDto> GetEmailTemplateInfo([ActionParameter] EmailTemplateIdentifier input)
    {
        var request = new RestRequest($"/rest/asset/v1/emailTemplate/{input.EmailTemplateId}.json", Method.Get);
        return await Client.ExecuteWithErrorHandlingFirst<EmailTemplateDto>(request);
    }

    [Action("Get email template content", Description = "Get email template content")]
    public async Task<EmailTemplateContentResponse> GetEmailTemplateContent([ActionParameter] EmailTemplateIdentifier input)
    {
        var request = new RestRequest($"/rest/asset/v1/emailTemplate/{input.EmailTemplateId}/content.json", Method.Get);
        var response = await Client.ExecuteWithErrorHandlingFirst<EmailTemplateContentResponse>(request);
        return response;
    }

    [Action("Create email template", Description = "Create email template")]
    public async Task<EmailTemplateDto> CreateEmailTemplate([ActionParameter] CreateEmailTemplateRequest input)
    {
        var request = new RestRequest($"/rest/asset/v1/emailTemplates.json", Method.Post);

        request.AddParameter("name", input.Name);
        request.AddFile("content", Encoding.UTF8.GetBytes(input.Content), "content");
        if (input.Description != null) request.AddParameter("description", input.Description);
        request.AddParameter("folder", JsonConvert.SerializeObject(new
        {
            id = int.Parse(input.FolderId.Split("_").First()),
            type = input.FolderId.Split("_").Last()
        }));

        var response = await Client.ExecuteWithErrorHandlingFirst<EmailTemplateDto>(request);
        return response;
    }

    [Action("Update email template content", Description = "Update email template content")]
    public async Task UpdateEmailTemplateContent(
        [ActionParameter] EmailTemplateIdentifier input,
        [ActionParameter] UpdateEmailTemplateContentRequest emailTemplateContentRequest)
    {
        var request = new RestRequest($"/rest/asset/v1/emailTemplate/{input.EmailTemplateId}/content.json", Method.Post);
        request.AddFile("content", Encoding.UTF8.GetBytes(emailTemplateContentRequest.Content), "content");
        await Client.ExecuteWithErrorHandling<IdDto>(request);
    }

    [Action("Delete email template", Description = "Delete email template")]
    public async Task DeleteEmailTemplate([ActionParameter] EmailTemplateIdentifier input)
    {
        var request = new RestRequest($"/rest/asset/v1/emailTemplate/{input.EmailTemplateId}/delete.json", Method.Post);
        await Client.ExecuteWithErrorHandling<IdDto>(request);
    }

    [Action("Approve email template draft", Description = "Approve email template draft")]
    public async Task<EmailTemplateDto> ApproveEmailTemplateDraft([ActionParameter] EmailTemplateIdentifier input)
    {
        var request = new RestRequest($"/rest/asset/v1/emailTemplate/{input.EmailTemplateId}/approveDraft.json", Method.Post);
        return await Client.ExecuteWithErrorHandlingFirst<EmailTemplateDto>(request);
    }
}
