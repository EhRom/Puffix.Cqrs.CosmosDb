using Puffix.Cqrs.Models;
using System;

namespace Puffix.Cqrs.CosmosDb.Models;

public interface IAggregateWithPartitionKey<IndexT> : IAggregate<IndexT>, IIndexable<IndexT>, IAggregate
    where IndexT : IComparable, IComparable<IndexT>, IEquatable<IndexT>
{
    public string PartitionKey { get; }

    public string TypeName { get; }
}