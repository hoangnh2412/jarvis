using Jarvis.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Sample.RequestResponseModels;

public class CustomPaging : Paging
{
    [FromQuery(Name = "currentPage")]
    public override int Page { get => base.Page; set => base.Page = value; }
}