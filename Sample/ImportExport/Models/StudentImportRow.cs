using System.ComponentModel.DataAnnotations;

namespace Sample.ImportExport.Models;

public sealed class StudentImportRow
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 150)]
    public int Age { get; set; }
}
