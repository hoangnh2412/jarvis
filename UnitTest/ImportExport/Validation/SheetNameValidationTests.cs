namespace UnitTest.ImportExport.Validation;

public class SheetNameValidationTests
{
    [Fact]
    public void ValidateSheetNameForTemplate_Csv_AllowsEmptySheetName()
    {
        var act = () => TemplateFileTypeHelper.ValidateSheetNameForTemplate(
            "data.csv",
            sheetName: null,
            sheets: []);

        act();
    }

    [Fact]
    public void ValidateSheetNameForTemplate_Excel_RequiresSheetName()
    {
        var act = () => TemplateFileTypeHelper.ValidateSheetNameForTemplate(
            "data.xlsx",
            sheetName: null,
            sheets: []);

        var ex = Assert.Throws<InvalidTemplateException>(act);
        Assert.Equal(EngineMessages.SheetNameRequiredForExcel, ex.Message);
    }

    [Fact]
    public void ColumnMappingJsonParser_ParsesMinLength()
    {
        const string json = """
            [{"columnName":"Code","fieldName":"Code","minLength":2,"maxLength":20}]
            """;

        var mappings = ColumnMappingJsonParser.Parse(json);

        Assert.Single(mappings);
        Assert.Equal(2, mappings[0].MinLength);
        Assert.Equal(20, mappings[0].MaxLength);
    }

    [Fact]
    public void MappingValidator_RejectsMinLengthGreaterThanMaxLength()
    {
        var mappings = new[]
        {
            new ColumnMapping("Code", "Code", MinLength: 10, MaxLength: 5)
        };

        var act = () => MappingValidator.Validate(mappings);

        Assert.Throws<InvalidTemplateException>(act);
    }
}
