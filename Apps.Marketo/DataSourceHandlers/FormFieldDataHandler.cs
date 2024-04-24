using Apps.Marketo.Dtos;
using Apps.Marketo.Models.Forms.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Marketo.DataSourceHandlers
{
    public class FormFieldDataHandler : BaseInvocable, IDataSourceHandler
    {
        public GetFormRequest GetFormRequest { get; set; }

        public FormFieldDataHandler(InvocationContext invocationContext, [ActionParameter] GetFormRequest getFormRequest) : base(invocationContext)
        {
            GetFormRequest = getFormRequest;
        }

        public Dictionary<string, string> GetData(DataSourceContext context)
        {
            if (string.IsNullOrWhiteSpace(GetFormRequest.FormId))
                throw new ArgumentException("Please specify form first!");

            var client = new MarketoClient(InvocationContext.AuthenticationCredentialsProviders);
            var getFieldsRequest = new MarketoRequest($"/rest/asset/v1/form/{GetFormRequest.FormId}/fields.json", Method.Get, InvocationContext.AuthenticationCredentialsProviders);
            var formFields = client.ExecuteWithError<FormFieldDto>(getFieldsRequest);
            return formFields.Result.Where(formField => context.SearchString == null ||
                                                 formField.Label.Contains(context.SearchString))
                .ToDictionary(formField => formField.Id, formField => formField.Label ?? "");
        }
    }
}
