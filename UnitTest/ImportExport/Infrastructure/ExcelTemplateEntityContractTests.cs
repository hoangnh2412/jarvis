using Platform.Modules.ImportExport.EntityFramework.Persistence.Mappings;

namespace UnitTest.ImportExport.Infrastructure;

public sealed class ExcelTemplateEntityContractTests
{
    private sealed class FakeTemplate : IExcelTemplateEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string FileName { get; set; } = "";
        public string? Location { get; set; }
        public string MappingsJson { get; set; } = "[]";
        public int HeaderRow { get; set; }
        public int StartRow { get; set; }
        public int? RowCount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    [Fact]
    public void SoftDelete_sets_IsActive_false()
    {
        var e = new FakeTemplate { IsActive = true };
        e.SoftDelete();
        Assert.False(e.IsActive);
    }

    [Fact]
    public void ToDefinition_maps_interface_fields()
    {
        var entity = new FakeTemplate
        {
            Code = "T1",
            Name = "N",
            FileName = "a.xlsx",
            MappingsJson = """[{"columnName":"A","fieldName":"A"}]""",
            HeaderRow = 1,
            StartRow = 2
        };
        var def = ExcelTemplateMapper.ToDefinition(entity);
        Assert.Equal("T1", def.Code);
        Assert.NotEmpty(def.Mappings);
    }

    [Fact]
    public void ToDefinition_maps_TenantId()
    {
        var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var entity = new FakeTemplate
        {
            Code = "T1",
            Name = "N",
            FileName = "a.xlsx",
            MappingsJson = "[]",
            TenantId = tenantId,
            HeaderRow = 1,
            StartRow = 2
        };
        var def = ExcelTemplateMapper.ToDefinition(entity);
        Assert.Equal(tenantId, def.TenantId);
    }
}
