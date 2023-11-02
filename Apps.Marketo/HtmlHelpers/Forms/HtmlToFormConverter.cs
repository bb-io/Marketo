using Apps.Marketo.Dtos;
using Blackbird.Applications.Sdk.Common.Authentication;
using HtmlAgilityPack;
using RestSharp;

namespace Apps.Marketo.HtmlHelpers.Forms;

public static class HtmlToFormConverter
{ 
    public static (FormDto, IEnumerable<FormFieldDto>) ConvertToForm(string html, 
        IEnumerable<AuthenticationCredentialsProvider> credentials)
    {
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        
        var outerDiv = htmlDocument.DocumentNode.SelectSingleNode("//body").SelectSingleNode("//div");
        var formId = outerDiv.Attributes["id"].Value;

        var client = new MarketoClient(credentials);
        var getFormRequest = new MarketoRequest($"/rest/asset/v1/form/{formId}.json", Method.Get, credentials);
        var originalForm = client.ExecuteWithError<FormDto>(getFormRequest).Result.First();
        
        var getFieldsRequest = new MarketoRequest($"/rest/asset/v1/form/{formId}/fields.json", Method.Get, credentials);
        var originalFields = client.ExecuteWithError<FormFieldDto>(getFieldsRequest).Result;

        const string formElementAttribute = "data-marketo-form-element";
        var innerDivs = outerDiv.ChildNodes;

        foreach (var div in innerDivs)
        {
            switch (div.Attributes[formElementAttribute].Value)
            {
                case "button":
                    foreach (var buttonNode in div.ChildNodes)
                    {
                        switch (buttonNode.Attributes[formElementAttribute].Value)
                        {
                            case nameof(originalForm.ButtonLabel):
                                originalForm.ButtonLabel = buttonNode.InnerText;
                                break;
                            
                            case nameof(originalForm.WaitingLabel):
                                originalForm.WaitingLabel = buttonNode.InnerText;
                                break;
                        }
                    }
                    
                    break;
                
                case "thankYouList":
                    var thankYouPagesDivs = div.ChildNodes;
                    var originalFormThankYouListCounter = 0;
                    var originalFormThankYouList = originalForm.ThankYouList.ToArray();

                    foreach (var pageDiv in thankYouPagesDivs)
                    {
                        if (!string.IsNullOrWhiteSpace(pageDiv.InnerHtml))
                        {
                            var values = new List<string>();

                            foreach (var value in pageDiv.ChildNodes)
                            {
                                values.Add(value.InnerText);
                            }

                            originalFormThankYouList[originalFormThankYouListCounter].Values = values;
                            originalFormThankYouListCounter++;
                        }
                    }

                    originalForm.ThankYouList = originalFormThankYouList;
                    break;
                
                case "fields":
                    foreach (var fieldNode in div.ChildNodes)
                    {
                        var fieldId = fieldNode.Attributes["data-marketo-Id"].Value;
                        var originalField = originalFields.Single(f => f.Id == fieldId);
                        UnwrapFieldFromDiv(fieldNode, originalField);
                    }
                    
                    break;
            }
        }
        
        return (originalForm, originalFields);
    }

    private static FormFieldDto UnwrapFieldFromDiv(HtmlNode div, FormFieldDto originalField)
    {
        const string dataFieldDataAttribute = "data-marketo-field-data";

        foreach (var node in div.ChildNodes)
        {
            if (node.Attributes.Contains(dataFieldDataAttribute))
            {
                var innerHtml = node.InnerHtml;
                switch (node.Attributes[dataFieldDataAttribute].Value)
                {
                    case nameof(originalField.Label):
                        originalField.Label = innerHtml;
                        break;

                    case nameof(originalField.Instructions):
                        originalField.Instructions = innerHtml;
                        break;

                    case nameof(originalField.DefaultValue):
                        originalField.DefaultValue = innerHtml;
                        break;

                    case nameof(originalField.ValidationMessage):
                        originalField.ValidationMessage = innerHtml;
                        break;

                    case nameof(originalField.HintText):
                        originalField.HintText = innerHtml;
                        break;

                    case nameof(originalField.Text):
                        originalField.Text = innerHtml;
                        break;

                    case nameof(originalField.FieldMetaData):
                        if (!string.IsNullOrWhiteSpace(innerHtml))
                        {
                            var originalFieldMetadataValues = originalField.FieldMetaData!.Values!.ToArray();
                            var values = node.ChildNodes;
                            var valuesCounter = 0;

                            foreach (var value in values)
                            {
                                originalFieldMetadataValues[valuesCounter].Label = value.InnerText;
                                valuesCounter++;
                            }

                            originalField.FieldMetaData.Values = originalFieldMetadataValues;
                        }

                        break;

                    case nameof(originalField.VisibilityRules):
                        if (!string.IsNullOrWhiteSpace(innerHtml))
                        {
                            var originalFieldVisibilityRules = originalField.VisibilityRules.Rules!.ToArray();
                            var rulesCounter = 0;

                            foreach (var childNode in node.ChildNodes)
                            {
                                var label = childNode.ChildNodes.Single(n => n.Name == "label");
                                originalFieldVisibilityRules[rulesCounter].AltLabel = label.InnerText;

                                var ruleValues = new List<string>();
                                var values = childNode.ChildNodes.Where(n => n.Name == "p");

                                foreach (var value in values)
                                {
                                    ruleValues.Add(value.InnerText);
                                }

                                originalFieldVisibilityRules[rulesCounter].Values = ruleValues;
                                rulesCounter++;
                            }
                        }
                        
                        break;
                }
            }
        }
        
        return originalField;
    }
}