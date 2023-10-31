namespace Apps.Marketo.Dtos;

public class FormFieldDto
{
    #region Metadata

    public string Id { get; set; }
    public string DataType { get; set; }
    public int RowNumber { get; set; }
    public int ColumnNumber { get; set; }
    public VisibilityRules VisibilityRules { get; set; } // contains localizable content under Rules
    public int? LabelWidth { get; set; }
    public int? FieldWidth { get; set; }
    public int? MaxLength { get; set; }
    public bool? Required { get; set; }
    public bool? FormPrefill { get; set; }
    public FieldMetaData? FieldMetaData { get; set; }

    #endregion

    #region Data

    public string? Label { get; set; }
    public string? Instructions { get; set; }
    public string? DefaultValue { get; set; }
    public string? ValidationMessage { get; set; }
    public string? HintText { get; set; }
    public string? Text { get; set; }

    #endregion
}

public class VisibilityRules
{
    public class VisibilityRule
    {
        public string SubjectField { get; set; }
        public string Operator { get; set; }
        public IEnumerable<string> Values { get; set; }
        public string AltLabel { get; set; }
    }
    
    public IEnumerable<VisibilityRule>? Rules { get; set; }
    public string RuleType { get; set; }
}

public class FieldMetaData
{
    public bool? InitiallyChecked { get; set; }
    public string? FieldMask { get; set; }
    public int? VisibleLines { get; set; }
    public bool? MultiSelect { get; set; }
    public IEnumerable<SelectLabelValue>? Values { get; set; }
}

public class SelectLabelValue
{
    public string Label { get; set; }
    public string Value { get; set; }
    public bool? IsDefault { get; set; }
    public bool? Selected { get; set; }
}