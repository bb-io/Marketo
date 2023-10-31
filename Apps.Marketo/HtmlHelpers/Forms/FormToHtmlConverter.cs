using System.Text;
using Apps.Marketo.Dtos;

namespace Apps.Marketo.HtmlHelpers.Forms;

public static class FormToHtmlConverter
{
    public static string ConvertToHtml(FormDto formDto, IEnumerable<FormFieldDto> formData)
    {
        var html = new StringBuilder();
        
        html.Append($"<div data-marketo-{nameof(formDto.Description)}=\"{formDto.Description}\" ");
        html.Append($"data-marketo-{nameof(formDto.Theme)}=\"{formDto.Theme}\" ");
        html.Append($"data-marketo-{nameof(formDto.ProgressiveProfiling)}=\"{formDto.ProgressiveProfiling}\" ");
        html.Append($"data-marketo-{nameof(formDto.LabelPosition)}=\"{formDto.LabelPosition}\" ");
        html.Append($"data-marketo-{nameof(formDto.FontFamily)}=\"{formDto.FontFamily}\" ");
        html.Append($"data-marketo-{nameof(formDto.FontSize)}=\"{formDto.FontSize}\" ");
        html.Append($"data-marketo-folderId=\"{formDto.Folder.Value}\" ");
        html.Append($"data-marketo-folderType=\"{formDto.Folder.Type}\">");

        foreach (var field in formData)
        {
            var htmlField = WrapFieldInDiv(field);
            html.Append(htmlField);
        }

        html.Append("</div>");
        return html.ToString();
    }
    
    private static string WrapFieldInDiv(FormFieldDto field)
    {
        const string dataTextTypeAttribute = "data-marketo-text-type";
        var htmlField = new StringBuilder();
        
        htmlField.Append($"<div data-marketo-{nameof(field.Id)}=\"{field.Id}\" ");
        htmlField.Append($"data-marketo-{nameof(field.DataType)}=\"{field.DataType}\" ");
        htmlField.Append($"data-marketo-{nameof(field.RowNumber)}=\"{field.RowNumber}\" ");
        htmlField.Append($"data-marketo-{nameof(field.ColumnNumber)}=\"{field.ColumnNumber}\" ");

        if (field.LabelWidth != null)
            htmlField.Append($"data-marketo-{nameof(field.LabelWidth)}=\"{field.LabelWidth}\" ");
        
        if (field.FieldWidth != null)
            htmlField.Append($"data-marketo-{nameof(field.FieldWidth)}=\"{field.FieldWidth}\" ");
            
        if (field.MaxLength != null)
            htmlField.Append($"data-marketo-{nameof(field.MaxLength)}=\"{field.MaxLength}\" ");
            
        if (field.Required != null)
            htmlField.Append($"data-marketo-{nameof(field.Required)}=\"{field.Required}\" ");
            
        if (field.FormPrefill != null)
            htmlField.Append($"data-marketo-{nameof(field.FormPrefill)}=\"{field.FormPrefill}\" ");

        htmlField.Append(">");
        
        if (field.Label != null)
            htmlField.Append($"<div {dataTextTypeAttribute}=\"{nameof(field.Label)}\">{field.Label}</div>");
        
        if (field.Instructions != null)
            htmlField.Append($"<div {dataTextTypeAttribute}=\"{nameof(field.Instructions)}\">{field.Instructions}</div>");
            
        if (field.DefaultValue != null)
            htmlField.Append($"<div {dataTextTypeAttribute}=\"{nameof(field.DefaultValue)}\">{field.DefaultValue}</div>");
        
        if (field.ValidationMessage != null)
            htmlField.Append($"<div {dataTextTypeAttribute}=\"{nameof(field.ValidationMessage)}\">{field.ValidationMessage}</div>");
            
        if (field.HintText != null)
            htmlField.Append($"<div {dataTextTypeAttribute}=\"{nameof(field.HintText)}\">{field.HintText}</div>");
            
        if (field.Text != null)
            htmlField.Append($"<div {dataTextTypeAttribute}=\"{nameof(field.Text)}\">{field.Text}</div>");

        htmlField.Append($"<div data-marketo-{nameof(field.VisibilityRules.RuleType)}=\"{field.VisibilityRules.RuleType}\">");
        if (field.VisibilityRules.Rules != null)
        {
            foreach (var rule in field.VisibilityRules.Rules)
            {
                htmlField.Append($"<div data-marketo-{nameof(rule.Operator)}=\"{rule.Operator}\" ");
                htmlField.Append($"data-marketo-{nameof(rule.SubjectField)}=\"{rule.SubjectField}\">");
                htmlField.Append($"<label>{rule.AltLabel}</label>");

                foreach (var value in rule.Values)
                {
                    htmlField.Append($"<p>{value}</p>");
                }

                htmlField.Append("</div>");
            }
        }
        
        if (field.FieldMetaData != null)
        {
            htmlField.Append("<div ");
            
            if (field.FieldMetaData.FieldMask != null)
                htmlField.Append($"data-marketo-{nameof(field.FieldMetaData.FieldMask)}=\"{field.FieldMetaData.FieldMask}\" ");
            
            if (field.FieldMetaData.InitiallyChecked != null)
                htmlField.Append($"data-marketo-{nameof(field.FieldMetaData.InitiallyChecked)}=\"{field.FieldMetaData.InitiallyChecked}\" ");
            
            if (field.FieldMetaData.VisibleLines != null)
                htmlField.Append($"data-marketo-{nameof(field.FieldMetaData.VisibleLines)}=\"{field.FieldMetaData.VisibleLines}\" ");
            
            if (field.FieldMetaData.MultiSelect != null)
                htmlField.Append($"data-marketo-{nameof(field.FieldMetaData.MultiSelect)}=\"{field.FieldMetaData.MultiSelect}\" ");

            htmlField.Append(">");

            if (field.FieldMetaData.Values != null)
            {
                foreach (var value in field.FieldMetaData.Values)
                {
                    htmlField.Append($"<p data-marketo-{nameof(value.Value)}=\"{value.Value}\" ");

                    if (value.Selected != null)
                        htmlField.Append($"data-marketo-{nameof(value.Selected)}=\"{value.Selected}\" ");
                    
                    if (value.IsDefault != null)
                        htmlField.Append($"data-marketo-{nameof(value.IsDefault)}=\"{value.IsDefault}\"");

                    htmlField.Append($">{value.Label}</p>");
                }
            }
            
            htmlField.Append("</div>");
        }

        htmlField.Append("</div></div>");

        return htmlField.ToString();
    }
}