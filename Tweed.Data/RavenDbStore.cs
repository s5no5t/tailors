using System.Diagnostics.CodeAnalysis;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Session;
using Raven.Client.Exceptions;
using Raven.Client.Exceptions.Database;
using Raven.Client.NodaTime;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace Tweed.Data;

[ExcludeFromCodeCoverage]
public class RavenDbStore
{
    private readonly DocumentStore _store;

    public RavenDbStore()
    {
        _store = new DocumentStore
        {
            Urls = new[] { "http://localhost:8080" },
            Database = "Tweed"
        };
        _store.ConfigureForNodaTime();
        _store.Initialize();
    }

    public IAsyncDocumentSession OpenSession()
    {
        var session = _store.OpenAsyncSession();
        return session;
    }

    public void EnsureDatabaseExists(bool createDatabaseIfNotExists = true)
    {
        if (string.IsNullOrWhiteSpace(_store.Database))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(_store.Database));

        try
        {
            _store.Maintenance.ForDatabase(_store.Database).Send(new GetStatisticsOperation());
        }
        catch (DatabaseDoesNotExistException)
        {
            if (createDatabaseIfNotExists == false)
                throw;

            try
            {
                _store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(_store.Database)));
            }
            catch (ConcurrencyException)
            {
                // The database was already created before calling CreateDatabaseOperation
            }
        }
    }
}
