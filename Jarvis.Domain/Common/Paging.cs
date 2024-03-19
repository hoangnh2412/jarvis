using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Domain.Common;

public class Paging : IPaging
{
    public virtual int Size { get; set; } = 10;

    public virtual int Page { get; set; } = 1;

    public virtual string Q { get; set; }

    public virtual Dictionary<string, string> Filters { get; set; }

    public virtual Dictionary<string, string> Sort { get; set; }

    public virtual IEnumerable<string> Columns { get; set; }
}