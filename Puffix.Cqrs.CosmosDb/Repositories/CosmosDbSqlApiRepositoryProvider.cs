using Microsoft.Azure.Cosmos;
using Puffix.Cqrs.CosmosDb.Models;
using Puffix.Cqrs.Models;
using Puffix.Cqrs.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Puffix.Cqrs.CosmosDb.Repositories;

public class CosmosDbSqlApiRepositoryProvider<AggregateImplementationT, AggregateT, IndexT> : IRepositoryProvider<AggregateImplementationT, AggregateT, IndexT>
    where AggregateImplementationT : class, AggregateT
    where AggregateT : IAggregate<IndexT>, IAggregateWithPartitionKey<IndexT>
    where IndexT : IComparable, IComparable<IndexT>, IEquatable<IndexT>
{
    private readonly ICosmosDbSqlApiContext cosmosDbSqlApiContext;

    private Container container => cosmosDbSqlApiContext.Container;

    public Type ElementType => typeof(AggregateT);

    public Expression Expression => container.GetItemLinqQueryable<AggregateImplementationT>(allowSynchronousQueryExecution: true).AsQueryable().Expression;

    public IQueryProvider Provider => container.GetItemLinqQueryable<AggregateImplementationT>(allowSynchronousQueryExecution: true).AsQueryable().Provider;

    public CosmosDbSqlApiRepositoryProvider(ICosmosDbSqlApiContext cosmosDbSqlApiContext, AggregateInfo aggregateInfo)
    {
        this.cosmosDbSqlApiContext = cosmosDbSqlApiContext;
        cosmosDbSqlApiContext.EnsureContainerCreated();
    }

    public async Task<bool> ExistsAsync(AggregateT aggregate)
    {
        bool exists;
        if (aggregate == null)
        {
            exists = false;
        }
        else
        {
            ICollection<AggregateT> collection = await BaseGetByIdAsync(aggregate.Id);
            exists = collection.Any();
        }

        return exists;
    }

    public async Task<bool> ExistsAsync(IndexT id)
    {
        ICollection<AggregateT> collection = await BaseGetByIdAsync(id);
        return collection.Any();
    }

    public async Task<AggregateT> GetByIdAsync(IndexT id)
    {
        ICollection<AggregateT> collection = await BaseGetByIdAsync(id);

        //throw new Exception($"The item ('{id}') is not found.");

        return collection.First();
    }

    public async Task<AggregateT> GetByIdOrDefaultAsync(IndexT id)
    {
        ICollection<AggregateT> collection = await BaseGetByIdAsync(id);
        return collection.FirstOrDefault();
    }

    private async Task<ICollection<AggregateT>> BaseGetByIdAsync(IndexT id)
    {
        string sqlQueryText = $"SELECT * FROM c WHERE c.id = '{id}'";
        QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

        ICollection<AggregateT> collection = new List<AggregateT>();
        FeedIterator<AggregateImplementationT> queryResultSetIterator = container.GetItemQueryIterator<AggregateImplementationT>(queryDefinition);
        while (queryResultSetIterator.HasMoreResults)
        {
            FeedResponse<AggregateImplementationT> currentResultSet = await queryResultSetIterator.ReadNextAsync();

            foreach (AggregateImplementationT aggregate in currentResultSet)
            {
                collection.Add(aggregate);
            }
        }

        return collection;
    }

    public async Task CreateAsync(AggregateT aggregate)
    {
        ItemResponse<AggregateImplementationT> itemResponse = await container.CreateItemAsync((AggregateImplementationT)aggregate, new PartitionKey(aggregate.PartitionKey));
    }

    public async Task UpdateAsync(AggregateT aggregate)
    {
        ItemResponse<AggregateImplementationT> itemResponse = await container.ReplaceItemAsync((AggregateImplementationT)aggregate, $"{aggregate.Id}", new PartitionKey(aggregate.PartitionKey));
    }

    public async Task DeleteAsync(AggregateT aggregate)
    {
        ItemResponse<AggregateImplementationT> itemResponse = await container.DeleteItemAsync<AggregateImplementationT>($"{aggregate.Id}", new PartitionKey(aggregate.PartitionKey));
    }

    public async Task<IndexT> GetNextAggregatetIdAsync(Func<IndexT, IndexT> generateNextId)
    {
        await Task.CompletedTask;

        IndexT lastId = default;

        IndexT nextId = generateNextId(lastId);
        ArgumentNullException.ThrowIfNull(nextId);
        return nextId;
    }

    public IEnumerator<AggregateImplementationT> GetEnumerator()
    {
        return container.GetItemLinqQueryable<AggregateImplementationT>(allowSynchronousQueryExecution: true).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return container.GetItemLinqQueryable<AggregateImplementationT>(allowSynchronousQueryExecution: true).GetEnumerator();
    }

    public async Task SaveAsync()
    {
        await Task.CompletedTask;
    }
}