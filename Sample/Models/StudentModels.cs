using System.ComponentModel.DataAnnotations;

namespace Sample.Models;

public sealed record StudentDto(Guid Id, string Name, int Age);

public sealed class CreateStudentRequest
{
    [Required]
    [MaxLength(200)]
    public required string Name { get; init; }

    [Range(0, 150)]
    public int Age { get; init; }
}

public sealed class UpdateStudentRequest
{
    [Required]
    [MaxLength(200)]
    public required string Name { get; init; }

    [Range(0, 150)]
    public int Age { get; init; }
}

public sealed class StudentListResponse
{
    public required IReadOnlyList<StudentDto> Items { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int Size { get; init; }
}

public sealed class StudentImportRequest
{
    [Required]
    public required IFormFile File { get; init; }
}
