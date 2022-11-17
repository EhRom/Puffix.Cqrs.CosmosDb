using Puffix.Cqrs.Models;
using Puffix.Cqrs.Repositories;
using System;
using System.Reflection;

namespace Puffix.Cqrs.CosmosDb.Repositories;

public class CosmosDbSqlApiRepositoriesConfiguration : IRepositoriesConfiguration
{
    private readonly ICosmosDbSqlApiContext cosmosDbSqlApiContext;

    public CosmosDbSqlApiRepositoriesConfiguration(ICosmosDbSqlApiContext cosmosDbSqlApiContext)
    {
        this.cosmosDbSqlApiContext = cosmosDbSqlApiContext;
    }

    public IRepositoryProvider<AggregateImplementationT, AggregateT, IndexT> GetRepositoryProvider<AggregateImplementationT, AggregateT, IndexT>(AggregateInfo aggregateInfo)
        where AggregateImplementationT : class, AggregateT
        where AggregateT : IAggregate<IndexT>
        where IndexT : IComparable, IComparable<IndexT>, IEquatable<IndexT>
    {
        Type cosmosDbSqlApiRepositoryProviderType = typeof(CosmosDbSqlApiRepositoryProvider<,,>).MakeGenericType(aggregateInfo.ImplementationType, aggregateInfo.AggregateType, aggregateInfo.IndexType);
        ArgumentNullException.ThrowIfNull(cosmosDbSqlApiRepositoryProviderType);

        ConstructorInfo? contructorInfo = cosmosDbSqlApiRepositoryProviderType.GetConstructor(new[] { typeof(ICosmosDbSqlApiContext), typeof(AggregateInfo) });
        ArgumentNullException.ThrowIfNull(contructorInfo);

        IRepositoryProvider<AggregateImplementationT, AggregateT, IndexT> repositoryProvider = (IRepositoryProvider<AggregateImplementationT, AggregateT, IndexT>)contructorInfo.Invoke(new object[] { cosmosDbSqlApiContext, aggregateInfo }); ;
        ArgumentNullException.ThrowIfNull(repositoryProvider);

        return repositoryProvider;
    }
}