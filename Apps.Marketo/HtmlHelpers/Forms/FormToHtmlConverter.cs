using HtmlAgilityPack;
using Apps.Marketo.Dtos;
using Apps.Marketo.Constants;
using Apps.Marketo.Models.Entities.Form;

namespace Apps.Marketo.HtmlHelpers.Forms;

public static class FormToHtmlConverter
{
    private const string FormElementAttribute = "data-marketo-form-element";
    private const string DataFieldDataAttribute = "data-marketo-field-data";

    public static string ConvertToHtml(
        FormEntity form,
        IEnumerable<FormFieldDto> formData,
        bool? ignoreVisibilityRules,
        IEnumerable<string>? ignoreFields)
    {
        var doc = new HtmlDocument();

        var htmlNode = doc.CreateElement("html");
        doc.DocumentNode.AppendChild(htmlNode);

        var headNode = doc.CreateElement("head");
        htmlNode.AppendChild(headNode);

        var metaNode = doc.CreateElement("meta");
        metaNode.SetAttributeValue("name", MetadataConstants.BlackbirdFormIdAttribute);
        metaNode.SetAttributeValue("content", form.Id.ToString());
        headNode.AppendChild(metaNode);

        var bodyNode = doc.CreateElement("body");
        htmlNode.AppendChild(bodyNode);

        var buttonDiv = doc.CreateElement("div");
        buttonDiv.SetAttributeValue(FormElementAttribute, "button");

        var btnLabelP = doc.CreateElement("p");
        btnLabelP.SetAttributeValue(FormElementAttribute, nameof(form.ButtonLabel));
        btnLabelP.InnerHtml = form.ButtonLabel;
        buttonDiv.AppendChild(btnLabelP);

        var waitLabelP = doc.CreateElement("p");
        waitLabelP.SetAttributeValue(FormElementAttribute, nameof(form.WaitingLabel));
        waitLabelP.InnerHtml = form.WaitingLabel;
        buttonDiv.AppendChild(waitLabelP);

        bodyNode.AppendChild(buttonDiv);

        var thankYouDiv = doc.CreateElement("div");
        thankYouDiv.SetAttributeValue(FormElementAttribute, "thankYouList");

        foreach (var thankYouPage in form.ThankYouList ?? [])
        {
            var tyItemDiv = doc.CreateElement("div");
            foreach (var value in thankYouPage.Values ?? [])
            {
                var p = doc.CreateElement("p");
                p.InnerHtml = value;
                tyItemDiv.AppendChild(p);
            }
            thankYouDiv.AppendChild(tyItemDiv);
        }
        bodyNode.AppendChild(thankYouDiv);

        var fieldsDiv = doc.CreateElement("div");
        fieldsDiv.SetAttributeValue(FormElementAttribute, "fields");

        var fieldsToProcess = formData.Where(f => ignoreFields == null || !ignoreFields.Contains(f.Id));

        foreach (var field in fieldsToProcess)
            fieldsDiv.AppendChild(CreateFieldNode(doc, field, ignoreVisibilityRules ?? false));

        bodyNode.AppendChild(fieldsDiv);

        return doc.DocumentNode.OuterHtml;
    }

    private static HtmlNode CreateFieldNode(HtmlDocument doc, FormFieldDto field, bool ignoreVisibilityRules)
    {
        var fieldDiv = doc.CreateElement("div");
        fieldDiv.SetAttributeValue($"data-marketo-{nameof(field.Id)}", field.Id);

        void AddDataField(string propertyName, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var div = doc.CreateElement("div");
                div.SetAttributeValue(DataFieldDataAttribute, propertyName);
                div.InnerHtml = value;
                fieldDiv.AppendChild(div);
            }
        }

        AddDataField(nameof(field.Label), field.Label);
        AddDataField(nameof(field.Instructions), field.Instructions);
        AddDataField(nameof(field.DefaultValue), field.DefaultValue);
        AddDataField(nameof(field.ValidationMessage), field.ValidationMessage);
        AddDataField(nameof(field.HintText), field.HintText);
        AddDataField(nameof(field.Text), field.Text);

        if (field.VisibilityRules?.Rules != null && !ignoreVisibilityRules)
        {
            var visDiv = doc.CreateElement("div");
            visDiv.SetAttributeValue(DataFieldDataAttribute, nameof(field.VisibilityRules));

            foreach (var rule in field.VisibilityRules.Rules)
            {
                var ruleDiv = doc.CreateElement("div");

                var label = doc.CreateElement("label");
                label.InnerHtml = rule.AltLabel;
                ruleDiv.AppendChild(label);

                foreach (var value in rule.Values ?? [])
                {
                    var p = doc.CreateElement("p");
                    p.InnerHtml = value;
                    ruleDiv.AppendChild(p);
                }
                visDiv.AppendChild(ruleDiv);
            }
            fieldDiv.AppendChild(visDiv);
        }

        if (field.FieldMetaData?.Values != null)
        {
            var metaDiv = doc.CreateElement("div");
            metaDiv.SetAttributeValue(DataFieldDataAttribute, nameof(field.FieldMetaData));

            foreach (var value in field.FieldMetaData.Values)
            {
                var p = doc.CreateElement("p");
                p.InnerHtml = value.Label;
                metaDiv.AppendChild(p);
            }
            fieldDiv.AppendChild(metaDiv);
        }

        return fieldDiv;
    }
}