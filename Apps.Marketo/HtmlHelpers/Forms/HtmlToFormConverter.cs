using Apps.Marketo.Dtos;
using HtmlAgilityPack;

namespace Apps.Marketo.HtmlHelpers.Forms;

public static class HtmlToFormConverter
{ 
    public static (FormDto, IEnumerable<FormFieldDto>) ConvertToForm(string html)
    {
        var htmlDocument = new HtmlDocument();
        html = RemoveSubstring(RemoveSubstring(html, "<html><body>"), "</body></html>");
        htmlDocument.LoadHtml(html);
        
        var outerDiv = htmlDocument.DocumentNode.SelectSingleNode("//div");
        var formDto = new FormDto { Folder = new FormFolderDto() };

        foreach (var attribute in outerDiv.Attributes)
        {
            switch (attribute.OriginalName)
            {
                case $"data-marketo-{nameof(formDto.Description)}":
                    formDto.Description = attribute.Value;
                    break;

                case $"data-marketo-{nameof(formDto.Theme)}":
                    formDto.Theme = attribute.Value;
                    break;

                case $"data-marketo-{nameof(formDto.ProgressiveProfiling)}":
                    formDto.ProgressiveProfiling = bool.Parse(attribute.Value);
                    break;

                case $"data-marketo-{nameof(formDto.LabelPosition)}":
                    formDto.LabelPosition = attribute.Value;
                    break;

                case $"data-marketo-{nameof(formDto.FontFamily)}":
                    formDto.FontFamily = attribute.Value;
                    break;
                
                case $"data-marketo-{nameof(formDto.FontSize)}":
                    formDto.FontSize = attribute.Value;
                    break;

                case "data-marketo-folderId":
                    formDto.Folder.Value = int.Parse(attribute.Value);
                    break;

                case "data-marketo-folderType":
                    formDto.Folder.Type = attribute.Value;
                    break;
            }
        }

        const string formElementAttribute = "data-marketo-form-element";
        var innerDivs = outerDiv.ChildNodes;
        var fields = new List<FormFieldDto>();

        foreach (var div in innerDivs)
        {
            switch (div.Attributes[formElementAttribute].Value)
            {
                case "button":
                    formDto.ButtonLocation =
                        int.Parse(div.Attributes[$"data-marketo-{nameof(formDto.ButtonLocation)}"].Value);
                    
                    foreach (var buttonNode in div.ChildNodes)
                    {
                        switch (buttonNode.Attributes[formElementAttribute].Value)
                        {
                            case nameof(formDto.ButtonLabel):
                                formDto.ButtonLabel = buttonNode.InnerText;
                                break;
                            
                            case nameof(formDto.WaitingLabel):
                                formDto.WaitingLabel = buttonNode.InnerText;
                                break;
                        }
                    }
                    
                    break;
                
                case "thankYouList":
                    var thankYouList = new List<ThankYouListDto>();
                    var thankYouPagesDivs = div.ChildNodes;

                    foreach (var pageDiv in thankYouPagesDivs)
                    {
                        var thankYouPage = new ThankYouListDto();
                        
                        foreach (var attribute in pageDiv.Attributes)
                        {
                            switch (attribute.OriginalName)
                            {
                                case $"data-marketo-{nameof(thankYouPage.FollowupType)}":
                                    thankYouPage.FollowupType = attribute.Value;
                                    break;
                                
                                case $"data-marketo-{nameof(thankYouPage.FollowupValue)}":
                                    thankYouPage.FollowupValue = attribute.Value;
                                    break;
                                
                                case $"data-marketo-{nameof(thankYouPage.Default)}":
                                    thankYouPage.Default = bool.Parse(attribute.Value);
                                    break;
                                
                                case $"data-marketo-{nameof(thankYouPage.Operator)}":
                                    thankYouPage.Operator = attribute.Value;
                                    thankYouPage.Values = new List<string>();
                                    break;
                                
                                case $"data-marketo-{nameof(thankYouPage.SubjectField)}":
                                    thankYouPage.SubjectField = attribute.Value;
                                    break;
                            }
                            
                            if (!string.IsNullOrWhiteSpace(pageDiv.InnerHtml))
                            {
                                var values = new List<string>();

                                foreach (var value in pageDiv.ChildNodes)
                                {
                                    values.Add(value.InnerText);
                                }

                                thankYouPage.Values = values;
                            }
                        }
                        
                        thankYouList.Add(thankYouPage);
                    }

                    formDto.ThankYouList = thankYouList;
                    break;
                
                case "fields":
                    foreach (var fieldNode in div.ChildNodes)
                    {
                        var field = UnwrapFieldFromDiv(fieldNode);
                        fields.Add(field);
                    }
                    
                    break;
            }
        }
        
        return (formDto, fields);
    }

    private static FormFieldDto UnwrapFieldFromDiv(HtmlNode div)
    {
        var field = new FormFieldDto();
        
        foreach (var attribute in div.Attributes)
        {
            switch (attribute.OriginalName)
            {
                case $"data-marketo-{nameof(field.Id)}":
                    field.Id = attribute.Value;
                    break;

                case $"data-marketo-{nameof(field.DataType)}":
                    field.DataType = attribute.Value;
                    break;

                case $"data-marketo-{nameof(field.RowNumber)}":
                    field.RowNumber = int.Parse(attribute.Value);
                    break;
                
                case $"data-marketo-{nameof(field.ColumnNumber)}":
                    field.ColumnNumber = int.Parse(attribute.Value);
                    break;
                
                case $"data-marketo-{nameof(field.LabelWidth)}":
                    field.LabelWidth = int.Parse(attribute.Value);
                    break;
                
                case $"data-marketo-{nameof(field.FieldWidth)}":
                    field.FieldWidth = int.Parse(attribute.Value);
                    break;
                
                case $"data-marketo-{nameof(field.MaxLength)}":
                    field.MaxLength = int.Parse(attribute.Value);
                    break;
                
                case $"data-marketo-{nameof(field.Required)}":
                    field.Required = bool.Parse(attribute.Value);
                    break;
                
                case $"data-marketo-{nameof(field.FormPrefill)}":
                    field.FormPrefill = bool.Parse(attribute.Value);
                    break;
            }
        }

        const string dataFieldDataAttribute = "data-marketo-field-data";
        
        foreach (var node in div.ChildNodes)
        {
            if (node.Attributes.Contains(dataFieldDataAttribute))
            {
                var innerHtml = node.InnerHtml;
                switch (node.Attributes[dataFieldDataAttribute].Value)
                {
                    case nameof(field.Label):
                        field.Label = innerHtml;
                        break;
                    
                    case nameof(field.Instructions):
                        field.Instructions = innerHtml;
                        break;
                    
                    case nameof(field.DefaultValue):
                        field.DefaultValue = innerHtml;
                        break;
                    
                    case nameof(field.ValidationMessage):
                        field.ValidationMessage = innerHtml;
                        break;
                    
                    case nameof(field.HintText):
                        field.HintText = innerHtml;
                        break;
                    
                    case nameof(field.Text):
                        field.Text = innerHtml;
                        break;
                    
                    case nameof(field.FieldMetaData):
                        var childDiv = node.ChildNodes.Single();
                        field.FieldMetaData = new FieldMetaData();

                        foreach (var attribute in childDiv.Attributes)
                        {
                            switch (attribute.OriginalName)
                            {
                                case $"data-marketo-{nameof(field.FieldMetaData.FieldMask)}":
                                    field.FieldMetaData.FieldMask = attribute.Value;
                                    break;
                                
                                case $"data-marketo-{nameof(field.FieldMetaData.InitiallyChecked)}":
                                    field.FieldMetaData.InitiallyChecked = bool.Parse(attribute.Value);
                                    break;
                                
                                case $"data-marketo-{nameof(field.FieldMetaData.VisibleLines)}":
                                    field.FieldMetaData.VisibleLines = int.Parse(attribute.Value);
                                    break;
                                
                                case $"data-marketo-{nameof(field.FieldMetaData.MultiSelect)}":
                                    field.FieldMetaData.MultiSelect = bool.Parse(attribute.Value);
                                    break;
                                
                                case $"data-marketo-{nameof(field.FieldMetaData.MinValue)}":
                                    field.FieldMetaData.MinValue = decimal.Parse(attribute.Value);
                                    break;
                                
                                case $"data-marketo-{nameof(field.FieldMetaData.MaxValue)}":
                                    field.FieldMetaData.MaxValue = decimal.Parse(attribute.Value);
                                    break;
                            }
                        }
                        
                        if (!string.IsNullOrWhiteSpace(childDiv.InnerHtml))
                        {
                            var fieldMetadataValues = new List<SelectLabelValue>();
                            var values = childDiv.ChildNodes;

                            foreach (var value in values)
                            {
                                var selectLabelValue = new SelectLabelValue();
                                    
                                foreach (var valueAttribute in value.Attributes)
                                {
                                    switch (valueAttribute.OriginalName)
                                    {
                                        case $"data-marketo-{nameof(selectLabelValue.Value)}":
                                            selectLabelValue.Value = valueAttribute.Value;
                                            break;
                                            
                                        case $"data-marketo-{nameof(selectLabelValue.Selected)}":
                                            selectLabelValue.Selected = bool.Parse(valueAttribute.Value);
                                            break;
                                            
                                        case $"data-marketo-{nameof(selectLabelValue.IsDefault)}":
                                            selectLabelValue.IsDefault = bool.Parse(valueAttribute.Value);
                                            break;
                                    }
                                }

                                selectLabelValue.Label = value.InnerText;
                                fieldMetadataValues.Add(selectLabelValue);
                            }

                            field.FieldMetaData.Values = fieldMetadataValues;
                        }
                        break;
                }
            }

            if (node.Attributes.Contains($"data-marketo-{nameof(field.VisibilityRules.RuleType)}"))
            {
                var ruleType = node.Attributes[$"data-marketo-{nameof(field.VisibilityRules.RuleType)}"].Value;
                field.VisibilityRules = new VisibilityRules { RuleType = ruleType };

                if (!string.IsNullOrWhiteSpace(node.InnerHtml))
                {
                    var visibilityRules = new List<VisibilityRules.VisibilityRule>();
                    
                    foreach (var childNode in node.ChildNodes)
                    {
                        var rule = new VisibilityRules.VisibilityRule();
                        
                        foreach (var attribute in childNode.Attributes)
                        {
                            switch (attribute.OriginalName)
                            {
                                case $"data-marketo-{nameof(rule.Operator)}":
                                    rule.Operator = attribute.Value;
                                    break;
                                
                                case $"data-marketo-{nameof(rule.SubjectField)}":
                                    rule.SubjectField = attribute.Value;
                                    break;
                            }
                        }

                        var label = childNode.ChildNodes.Single(n => n.Name == "label");
                        rule.AltLabel = label.InnerText;
                        
                        var ruleValues = new List<string>();
                        var values = childNode.ChildNodes.Where(n => n.Name == "p");

                        foreach (var value in values)
                        {
                            ruleValues.Add(value.InnerText);
                        }

                        rule.Values = ruleValues;
                        visibilityRules.Add(rule);
                    }

                    field.VisibilityRules.Rules = visibilityRules;
                }
            }
        }

        return field;
    }
    
    private static string RemoveSubstring(string sourceString, string substring)
    {
        var index = sourceString.IndexOf(substring, StringComparison.Ordinal);
        var result = index < 0 ? sourceString : sourceString.Remove(index, substring.Length);
        return result;
    }
}