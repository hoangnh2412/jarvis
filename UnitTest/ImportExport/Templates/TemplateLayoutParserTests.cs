namespace UnitTest.ImportExport.Templates;

public sealed class TemplateLayoutParserTests
{
    [Fact]
    public void DeserializeLayout_parses_legacy_mapping_array()
    {
        const string json = """[{"columnName":"Name","fieldName":"name","required":true}]""";

        var (mappings, sheets, sheetName) = TemplateLayoutParser.DeserializeLayout(json);

        Assert.Single(mappings);
        Assert.Equal("Name", mappings[0].ColumnName);
        Assert.Equal("name", mappings[0].FieldName);
        Assert.True(mappings[0].Required);
        Assert.Empty(sheets);
        Assert.Null(sheetName);
    }

    [Fact]
    public void DeserializeLayout_parses_root_sheetName_and_mappings()
    {
        const string json =
            """{"sheetName":"Students","mappings":[{"columnName":"Name","fieldName":"name"}]}""";

        var (mappings, sheets, sheetName) = TemplateLayoutParser.DeserializeLayout(json);

        Assert.Single(mappings);
        Assert.Empty(sheets);
        Assert.Equal("Students", sheetName);
    }

    [Fact]
    public void DeserializeLayout_promotes_single_sheet_document_from_frontend()
    {
        const string json =
            """{"sheets":[{"sheetName":"Students","mappings":[{"columnName":"Name","fieldName":"name"}]}]}""";

        var (mappings, sheets, sheetName) = TemplateLayoutParser.DeserializeLayout(json);

        Assert.Single(mappings);
        Assert.Equal("Name", mappings[0].ColumnName);
        Assert.Empty(sheets);
        Assert.Equal("Students", sheetName);
    }

    [Fact]
    public void DeserializeLayout_keeps_multi_sheet_document_at_sheets()
    {
        const string json =
            """
            {
              "sheets": [
                {"sheetName":"A","mappings":[{"columnName":"C1","fieldName":"c1"}]},
                {"sheetName":"B","mappings":[{"columnName":"C2","fieldName":"c2"}]}
              ]
            }
            """;

        var (mappings, sheets, sheetName) = TemplateLayoutParser.DeserializeLayout(json);

        Assert.Empty(mappings);
        Assert.Equal(2, sheets.Count);
        Assert.Null(sheetName);
        Assert.Single(sheets[0].Mappings);
        Assert.Equal("C1", sheets[0].Mappings[0].ColumnName);
    }

    [Fact]
    public void SerializeLayout_round_trips_single_sheet_with_sheetName()
    {
        var json = TemplateLayoutParser.SerializeLayout(
            [new("Name", "name", Required: true)],
            sheetName: "Students");

        var (mappings, sheets, sheetName) = TemplateLayoutParser.DeserializeLayout(json);

        Assert.Single(mappings);
        Assert.True(mappings[0].Required);
        Assert.Empty(sheets);
        Assert.Equal("Students", sheetName);
    }

    [Fact]
    public void DeserializeLayout_trims_columnName_and_fieldName()
    {
        const string json =
            """[{"columnName":" Name ","fieldName":" name ","required":true}]""";

        var (mappings, _, _) = TemplateLayoutParser.DeserializeLayout(json);

        Assert.Single(mappings);
        Assert.Equal("Name", mappings[0].ColumnName);
        Assert.Equal("name", mappings[0].FieldName);
    }
}
