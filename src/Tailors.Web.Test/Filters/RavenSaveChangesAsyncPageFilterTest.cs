using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Client.Documents.Session.Loaders;
using Tailors.Web.Filters;
using Xunit;

namespace Tailors.Web.Test.Filters;

public class RavenSaveChangesAsyncPageFilterTest
{
    [Fact]
    public async Task OnPageHandlerExecutionAsync_ShouldCallSaveChangesAsync()
    {
        var sessionMock = new SessionMock();
        var actionContext = new ActionContext
        {
            HttpContext = new DefaultHttpContext(),
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };
        var pageContext = new PageContext(actionContext);
        var pageHandlerExecutingContext = new PageHandlerExecutingContext(pageContext,
            Array.Empty<IFilterMetadata>(), null, new Dictionary<string, object?>(), new object());
        var pageHandlerExecutedContext = new PageHandlerExecutedContext(pageContext,
            Array.Empty<IFilterMetadata>(), null, new object());
        var asyncFilter = new RavenSaveChangesAsyncPageFilter(sessionMock);

        await asyncFilter.OnPageHandlerExecutionAsync(pageHandlerExecutingContext,
            () => Task.FromResult(pageHandlerExecutedContext));

        Assert.True(sessionMock.ChangesSaved);
    }
}

internal class SessionMock : IAsyncDocumentSession
{
    public bool ChangesSaved { get; private set; }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public IAsyncSessionDocumentCounters CountersFor(string documentId)
    {
        throw new NotImplementedException();
    }

    public IAsyncSessionDocumentCounters CountersFor(object entity)
    {
        throw new NotImplementedException();
    }

    public void Delete<T>(T entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(string id)
    {
        throw new NotImplementedException();
    }

    public void Delete(string id, string expectedChangeVector)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(CancellationToken token = new())
    {
        ChangesSaved = true;
        return Task.CompletedTask;
    }

    public Task StoreAsync(object entity, CancellationToken token = new())
    {
        throw new NotImplementedException();
    }

    public Task StoreAsync(object entity, string changeVector, string id, CancellationToken token = new())
    {
        throw new NotImplementedException();
    }

    public Task StoreAsync(object entity, string id, CancellationToken token = new())
    {
        throw new NotImplementedException();
    }

    public IAsyncLoaderWithInclude<object> Include(string path)
    {
        throw new NotImplementedException();
    }

    public IAsyncLoaderWithInclude<T> Include<T>(Expression<Func<T, string>> path)
    {
        throw new NotImplementedException();
    }

    public IAsyncLoaderWithInclude<T> Include<T, TInclude>(Expression<Func<T, string>> path)
    {
        throw new NotImplementedException();
    }

    public IAsyncLoaderWithInclude<T> Include<T>(Expression<Func<T, IEnumerable<string>>> path)
    {
        throw new NotImplementedException();
    }

    public IAsyncLoaderWithInclude<T> Include<T, TInclude>(Expression<Func<T, IEnumerable<string>>> path)
    {
        throw new NotImplementedException();
    }

    public Task<T> LoadAsync<T>(string id, CancellationToken token = new())
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<string, T>> LoadAsync<T>(IEnumerable<string> ids, CancellationToken token = new())
    {
        throw new NotImplementedException();
    }

    public Task<T> LoadAsync<T>(string id, Action<IIncludeBuilder<T>> includes, CancellationToken token = new())
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<string, T>> LoadAsync<T>(IEnumerable<string> ids, Action<IIncludeBuilder<T>> includes,
        CancellationToken token = new())
    {
        throw new NotImplementedException();
    }

    public IRavenQueryable<T> Query<T>(string? indexName = null, string? collectionName = null,
        bool isMapReduce = false)
    {
        throw new NotImplementedException();
    }

    public IRavenQueryable<T> Query<T, TIndexCreator>() where TIndexCreator : AbstractCommonApiForIndexes, new()
    {
        throw new NotImplementedException();
    }

    public IAsyncSessionDocumentTimeSeries TimeSeriesFor(string documentId, string name)
    {
        throw new NotImplementedException();
    }

    public IAsyncSessionDocumentTimeSeries TimeSeriesFor(object entity, string name)
    {
        throw new NotImplementedException();
    }

    public IAsyncSessionDocumentTypedTimeSeries<TValues> TimeSeriesFor<TValues>(string documentId, string? name = null)
        where TValues : new()
    {
        throw new NotImplementedException();
    }

    public IAsyncSessionDocumentTypedTimeSeries<TValues> TimeSeriesFor<TValues>(object entity, string? name = null)
        where TValues : new()
    {
        throw new NotImplementedException();
    }

    public IAsyncSessionDocumentRollupTypedTimeSeries<TValues> TimeSeriesRollupFor<TValues>(object entity,
        string policy, string? raw = null) where TValues : new()
    {
        throw new NotImplementedException();
    }

    public IAsyncSessionDocumentRollupTypedTimeSeries<TValues> TimeSeriesRollupFor<TValues>(string documentId,
        string policy,
        string? raw = null) where TValues : new()
    {
        throw new NotImplementedException();
    }

    public IAsyncSessionDocumentIncrementalTimeSeries IncrementalTimeSeriesFor(string documentId, string name)
    {
        throw new NotImplementedException();
    }

    public IAsyncSessionDocumentIncrementalTimeSeries IncrementalTimeSeriesFor(object entity, string name)
    {
        throw new NotImplementedException();
    }

    public IAsyncSessionDocumentTypedIncrementalTimeSeries<TValues> IncrementalTimeSeriesFor<TValues>(string documentId,
        string? name = null) where TValues : new()
    {
        throw new NotImplementedException();
    }

    public IAsyncSessionDocumentTypedIncrementalTimeSeries<TValues> IncrementalTimeSeriesFor<TValues>(object entity,
        string? name = null) where TValues : new()
    {
        throw new NotImplementedException();
    }

    public IAsyncAdvancedSessionOperations? Advanced => null;
}
