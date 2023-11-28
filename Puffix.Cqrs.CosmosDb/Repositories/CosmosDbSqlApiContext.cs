using Microsoft.Azure.Cosmos;
using System;
using System.Threading.Tasks;

namespace Puffix.Cqrs.CosmosDb.Repositories;

public class CosmosDbSqlApiContext : ICosmosDbSqlApiContext
{
    private bool disposed = false;

    private readonly CosmosClient cosmosClient;

    private readonly string databaseName;
    private readonly string containerName;
    private readonly string partitionKeyName;
    private Container container;

    public Container Container => container!;

    public CosmosDbSqlApiContext(string endpointUrl, string authorizationKey, string databaseName, string containerName, string partitionKeyName)
        : base()
    {
        cosmosClient = new CosmosClient(endpointUrl, authorizationKey);

        this.databaseName = databaseName;
        this.containerName = containerName;
        this.partitionKeyName = partitionKeyName;
    }

    ~CosmosDbSqlApiContext() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            cosmosClient.Dispose();
        }

        disposed = true;
    }

    public bool EnsureContainerCreated()
    {
        Task<bool> ensureContainerCreatedAsyncTask = Task.Run(async () => await EnsureContainerCreatedAsync());
        ensureContainerCreatedAsyncTask.Wait();

        return ensureContainerCreatedAsyncTask.Result;
    }

    public async Task<bool> EnsureContainerCreatedAsync()
    {
        DatabaseResponse databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
        Database database = databaseResponse.Database;
        ContainerResponse containerResponse = await database.CreateContainerIfNotExistsAsync(containerName, $"/{partitionKeyName}");

        container = containerResponse.Container;
        return true;
    }
}