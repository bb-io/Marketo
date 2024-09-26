using System.Text;
using Apps.Marketo.Dtos;
using Apps.Marketo.Models.Forms.Requests;

namespace Apps.Marketo.HtmlHelpers.Forms;

public static class FormToHtmlConverter
{
    public static string ConvertToHtml(FormDto formDto, IEnumerable<FormFieldDto> formData, IgnoreFieldsRequest ignoreFieldsRequest)
    {
        const string formElementAttribute = "data-marketo-form-element";
        
        var html = new StringBuilder();
        html.Append($"<div id=\"{formDto.Id}\">");

        html.Append($"<div {formElementAttribute}=\"button\">");
        html.Append($"<p {formElementAttribute}=\"{nameof(formDto.ButtonLabel)}\">{formDto.ButtonLabel}</p>");
        html.Append($"<p {formElementAttribute}=\"{nameof(formDto.WaitingLabel)}\">{formDto.WaitingLabel}</p>");
        html.Append("</div>");

        html.Append($"<div {formElementAttribute}=\"thankYouList\">");

        foreach (var thankYouPage in formDto.ThankYouList)
        {
            html.Append("<div>");

            if (thankYouPage.Values != null)
                foreach (var value in thankYouPage.Values)
                {
                    html.Append($"<p>{value}</p>");
                }

            html.Append("</div>");
        }
        
        html.Append("</div>");

        html.Append($"<div {formElementAttribute}=\"fields\">");
        
        foreach (var field in formData)
        {
            if(ignoreFieldsRequest.IgnoreFields != null && ignoreFieldsRequest.IgnoreFields.Contains(field.Id))
            {
                continue;
            }
            var htmlField = WrapFieldInDiv(field, ignoreFieldsRequest.IgnoreVisibilityRules ?? false);
            html.Append(htmlField);
        }

        html.Append("</div>");
        html.Append("</div>");
        return html.ToString();
    }
    
    private static string WrapFieldInDiv(FormFieldDto field, bool ignoreVisibilityRulesContent)
    {
        const string dataFieldDataAttribute = "data-marketo-field-data";
        var htmlField = new StringBuilder();
        
        htmlField.Append($"<div data-marketo-{nameof(field.Id)}=\"{field.Id}\">");
        
        if (field.Label != null)
            htmlField.Append($"<div {dataFieldDataAttribute}=\"{nameof(field.Label)}\">{field.Label}</div>");
        
        if (field.Instructions != null)
            htmlField.Append($"<div {dataFieldDataAttribute}=\"{nameof(field.Instructions)}\">{field.Instructions}</div>");
            
        if (field.DefaultValue != null)
            htmlField.Append($"<div {dataFieldDataAttribute}=\"{nameof(field.DefaultValue)}\">{field.DefaultValue}</div>");
        
        if (field.ValidationMessage != null)
            htmlField.Append($"<div {dataFieldDataAttribute}=\"{nameof(field.ValidationMessage)}\">{field.ValidationMessage}</div>");
            
        if (field.HintText != null)
            htmlField.Append($"<div {dataFieldDataAttribute}=\"{nameof(field.HintText)}\">{field.HintText}</div>");
            
        if (field.Text != null)
            htmlField.Append($"<div {dataFieldDataAttribute}=\"{nameof(field.Text)}\">{field.Text}</div>");

        if (field.VisibilityRules != null && !ignoreVisibilityRulesContent)
        {
            htmlField.Append($"<div {dataFieldDataAttribute}=\"{nameof(field.VisibilityRules)}\">");
        
            if (field.VisibilityRules.Rules != null)
            {
                foreach (var rule in field.VisibilityRules.Rules)
                {
                    htmlField.Append("<div>");
                    htmlField.Append($"<label>{rule.AltLabel}</label>");

                    foreach (var value in rule.Values)
                    {
                        htmlField.Append($"<p>{value}</p>");
                    }
                    htmlField.Append("</div>");
                }
            }

            htmlField.Append("</div>");
        }
        
        if (field.FieldMetaData != null)
        {
            htmlField.Append($"<div {dataFieldDataAttribute}=\"{nameof(field.FieldMetaData)}\">");

            if (field.FieldMetaData.Values != null)
            {
                foreach (var value in field.FieldMetaData.Values)
                {
                    htmlField.Append($"<p>{value.Label}</p>");
                }
            }
            
            htmlField.Append("</div>");
        }

        htmlField.Append("</div>");

        return htmlField.ToString();
    }
}