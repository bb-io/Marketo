using Apps.Marketo.Dtos;
using Apps.Marketo.Invocables;
using Apps.Marketo.Models.EmailTemplates.Requests;
using Apps.Marketo.Models.EmailTemplates.Response;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Newtonsoft.Json;
using RestSharp;
using System.Text;

namespace Apps.Marketo.Actions
{
    [ActionList]
    public class EmailTemplateActions : MarketoInvocable
    {
        private readonly IFileManagementClient _fileManagementClient;

        public EmailTemplateActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(invocationContext)
        {
            _fileManagementClient = fileManagementClient;
        }

        [Action("Search email templates", Description = "Search all email templates")]
        public ListEmailTemplatesResponse ListEmailTemplates([ActionParameter] ListEmailTemplatesRequest input)
        {
            var request = new MarketoRequest($"/rest/asset/v1/emailTemplates.json", Method.Get, Credentials);

            if (input.Status != null) request.AddQueryParameter("status", input.Status);
            var response = Client.Paginate<EmailTemplateDto>(request);

            if (input.Status == null) 
            {
                request.AddQueryParameter("status", "approved");
                var approvedTemaplates = Client.Paginate<EmailTemplateDto>(request);
                response.AddRange(approvedTemaplates);
                response = response.DistinctBy(x => x.Id).ToList();
            } 

            return new() { EmailTemplates = response };
        }

        [Action("Get email template info", Description = "Get email template info")]
        public EmailTemplateDto GetEmailTemplateInfo([ActionParameter] GetEmailTemplateRequest input)
        {
            var request = new MarketoRequest($"/rest/asset/v1/emailTemplate/{input.EmailTemplateId}.json", Method.Get, Credentials);
            return Client.GetSingleEntity<EmailTemplateDto>(request);
        }

        [Action("Get email template content", Description = "Get email template content")]
        public EmailTemplateContentResponse GetEmailTemplateContent([ActionParameter] GetEmailTemplateRequest input)
        {
            var request = new MarketoRequest($"/rest/asset/v1/emailTemplate/{input.EmailTemplateId}/content.json", Method.Get, Credentials);
            var response = Client.GetSingleEntity<EmailTemplateContentResponse>(request);
            return response;
        }

        [Action("Create email template", Description = "Create email template")]
        public EmailTemplateDto CreateEmailTemplate([ActionParameter] CreateEmailTemplateRequest input)
        {
            var request = new MarketoRequest($"/rest/asset/v1/emailTemplates.json", Method.Post, Credentials);

            request.AddParameter("name", input.Name);
            request.AddFile("content", Encoding.UTF8.GetBytes(input.Content), "content");
            if (input.Description != null) request.AddParameter("description", input.Description);
            request.AddParameter("folder", JsonConvert.SerializeObject(new
            {
                id = int.Parse(input.FolderId.Split("_").First()),
                type = input.FolderId.Split("_").Last()
            }));

            var response = Client.GetSingleEntity<EmailTemplateDto>(request);
            return response;
        }

        [Action("Update email template content", Description = "Update email template content")]
        public void UpdateEmailTemplateContent([ActionParameter] GetEmailTemplateRequest input,
            [ActionParameter] UpdateEmailTemplateContentRequest emailTemplateContentRequest)
        {
            var request = new MarketoRequest($"/rest/asset/v1/emailTemplate/{input.EmailTemplateId}/content.json", Method.Post, Credentials);
            request.AddFile("content", Encoding.UTF8.GetBytes(emailTemplateContentRequest.Content), "content");
            Client.ExecuteWithError<IdDto>(request);
        }

        [Action("Delete email template", Description = "Delete email template")]
        public void DeleteEmailTemplate([ActionParameter] GetEmailTemplateRequest input)
        {
            var request = new MarketoRequest($"/rest/asset/v1/emailTemplate/{input.EmailTemplateId}/delete.json", Method.Post, Credentials);
            Client.ExecuteWithError<IdDto>(request);
        }

        [Action("Approve email template draft", Description = "Approve email template draft")]
        public EmailTemplateDto ApproveEmailTemplateDraft([ActionParameter] GetEmailTemplateRequest input)
        {
            var request = new MarketoRequest($"/rest/asset/v1/emailTemplate/{input.EmailTemplateId}/approveDraft.json", Method.Post, Credentials);
            return Client.GetSingleEntity<EmailTemplateDto>(request);
        }
    }
}
