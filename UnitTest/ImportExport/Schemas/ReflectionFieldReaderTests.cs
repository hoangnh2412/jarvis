using System.ComponentModel.DataAnnotations;

namespace UnitTest.ImportExport.Schemas;

public class ReflectionFieldReaderTests
{
    private sealed class AnnotatedRow
    {
        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        [RegularExpression("^[A-Z]+$")]
        public string Code { get; set; } = string.Empty;

        [Range(0, 100)]
        public int Score { get; set; }
    }

    [Fact]
    public void Read_MapsAllValidationAttributes()
    {
        var fields = ReflectionFieldReader.Read(typeof(AnnotatedRow));
        var code = fields.Single(f => f.Name == "Code");
        var score = fields.Single(f => f.Name == "Score");

        Assert.True(code.Required);
        Assert.Equal(2, code.MinLength);
        Assert.Equal(50, code.MaxLength);
        Assert.Equal("^[A-Z]+$", code.Regex);
        Assert.Equal("string", code.DataType);
        Assert.False(code.IsNumeric);
        Assert.Contains("String", code.ClrType);

        Assert.Equal("int", score.DataType);
        Assert.True(score.IsNumeric);
        Assert.Equal(0, score.Min);
        Assert.Equal(100, score.Max);
    }
}
