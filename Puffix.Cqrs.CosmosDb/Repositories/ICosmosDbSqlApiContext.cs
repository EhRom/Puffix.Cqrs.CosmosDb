using Microsoft.Azure.Cosmos;
using System;
using System.Threading.Tasks;

namespace Puffix.Cqrs.CosmosDb.Repositories;

public interface ICosmosDbSqlApiContext : IDisposable
{
    Container Container { get; }

    bool EnsureContainerCreated();

    Task<bool> EnsureContainerCreatedAsync();
}