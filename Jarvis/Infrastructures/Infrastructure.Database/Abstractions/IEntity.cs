using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Database.Abstractions
{
    public interface IEntity
    {
    }

    public interface IEntity<T> : IEntity
    {
        T Id { get; set; }
    }
}
