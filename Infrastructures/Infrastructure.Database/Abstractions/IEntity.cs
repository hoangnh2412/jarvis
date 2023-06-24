using System;

namespace Infrastructure.Database.Abstractions
{
    public interface IEntity
    {
        public Guid Key { get; set; }
    }

    public interface IEntity<T> : IEntity
    {
        T Id { get; set; }
    }
}
