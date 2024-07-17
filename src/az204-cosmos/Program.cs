using System.Diagnostics;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace az204_cosmos;

public class Program
{
    /// <summary>
    /// The configuration instance for the application.
    /// </summary>
    private static readonly IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

    #region Environment Variables
    private static readonly string EndpointUri = configuration["COSMOS_ENDPOINT"] ?? throw new ArgumentNullException("COSMOS_ENDPOINT");
    private static readonly string PrimaryKey = configuration["COSMOS_KEY"] ?? throw new ArgumentNullException("COSMOS_KEY");
    #endregion

    #region Constants
    private const string databaseId = "az204Database";
    private const string containerId = "az204Container";
    #endregion

    /// <summary>
    /// The Azure Cosmos DB client instance
    /// </summary>
    private CosmosClient? cosmosClient;

    /// <summary>
    /// The Azure Cosmos DB database instance
    /// </summary>
    private Database? database;

    /// <summary>
    /// The Azure Cosmos DB container instance
    /// </summary>
    private Container? container;

    public static async Task Main()
    {
        try
        {
            Debug.WriteLine("Checking for environment variables...");

            if (string.IsNullOrEmpty(EndpointUri) || string.IsNullOrEmpty(PrimaryKey))
            {
                Console.Error.WriteLine("Environment variables not found. Exiting program.");
                return;
            }

            Debug.WriteLine("Environment variables found.");

            Console.WriteLine("Beginning operations...\n");

            var p = new Program();
            await p.CosmosAsync();
        }
        catch (CosmosException de)
        {
            Exception baseException = de.GetBaseException();
            Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e);
        }
        finally
        {
            Console.WriteLine("End of program, press any key to exit.");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// The main method for the Cosmos DB operations.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    public async Task CosmosAsync()
    {
        cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);


        await CreateDatabaseAsync();
        await CreateContainerAsync();

        // await AddItemsToContainerAsync();
        // await QueryItemsAsync();

        await DropContainerAsync();
        await DropDatabaseAsync();

        // Cleanup
        container = null;
        database = null;
        cosmosClient.Dispose();
    }

    #region Azure Cosmos DB

    private async Task CreateDatabaseAsync()
    {
        Console.WriteLine("Creating Database...");
        if (cosmosClient is null)
        {
            Console.Error.WriteLine($"Error: {nameof(cosmosClient)} is null.");
            return;
        }

        database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
        Console.WriteLine("Created Database: {0}\n", database.Id);
    }

    private async Task CreateContainerAsync()
    {
        Console.WriteLine("Creating Container...");
        if (database is null)
        {
            Console.Error.WriteLine($"Error: {nameof(database)} is null.");
            return;
        }

        container = await database.CreateContainerIfNotExistsAsync(containerId, "/LastName");
        Console.WriteLine("Created Container: {0}\n", container.Id);

    }

    private async Task DropContainerAsync()
    {
        Console.WriteLine("Deleting Container...");
        if (container is null)
        {
            Console.Error.WriteLine($"Error: {nameof(container)} is null.");
            return;
        }

        await container.DeleteContainerAsync();
        Console.WriteLine("Deleted Container: {0}\n", container.Id);
    }

    private async Task DropDatabaseAsync()
    {
        Console.WriteLine("Deleting Database...");
        if (database is null)
        {
            Console.Error.WriteLine($"Error: {nameof(database)} is null.");
            return;
        }

        await database.DeleteAsync();
        Console.WriteLine("Deleted Database: {0}\n", database.Id);
    }

    #endregion
}
