using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Maliev.MessagingContracts.Contracts.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Testcontainers.PostgreSql;

namespace Legacy.Maliev.CompatibilityContracts.Tests;

[Collection("Retained pagination PostgreSQL")]
public sealed class RetainedPaginationPostgresAcceptanceTests(RetainedPaginationPostgresFixture fixture)
    : IClassFixture<RetainedPaginationPostgresFixture>
{
    [Theory]
    [InlineData(-6, 1, "A,B,C")]
    [InlineData(0, 1, "A,B,C")]
    [InlineData(1, 1, "A,B,C")]
    [InlineData(2, 2, "D,E,F")]
    [InlineData(3, 3, "G")]
    [InlineData(4, 4, "")]
    public async Task CreateAsync_RealProvider_PreservesCountPageClampingOrderedSliceAndNavigation(
        int requestedPage, int expectedPage, string expectedItems)
    {
        var commands = new RetainedPaginationCommandRecorder();
        await using var context = fixture.CreateContext(commands);
        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", context.Database.ProviderName);
        var source = context.Rows.AsNoTracking().OrderBy(row => row.Id).Select(row => row.Name);
        var result = await CreateAsync(source, requestedPage, pageSize: 3);
        Assert.NotNull(result);
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(result, result.GetType()));
        var root = document.RootElement;
        Assert.Equal(7, root.GetProperty("TotalRecords").GetInt32());
        Assert.Equal(3, root.GetProperty("TotalPages").GetInt32());
        Assert.Equal(expectedPage, root.GetProperty("PageIndex").GetInt32());
        Assert.Equal(expectedPage < 3, root.GetProperty("HasNextPage").GetBoolean());
        Assert.Equal(expectedPage > 1, root.GetProperty("HasPreviousPage").GetBoolean());
        string[] expected = expectedItems.Length == 0 ? [] : expectedItems.Split(',');
        Assert.Equal(expected, root.GetProperty("Items").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(2, commands.Commands.Count);
        Assert.Contains("count(*)", commands.Commands[0], StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("count(*)", commands.Commands[1], StringComparison.OrdinalIgnoreCase);
        Assert.Contains("LIMIT", commands.Commands[1], StringComparison.OrdinalIgnoreCase);
        if (expectedPage > 1)
        {
            Assert.Contains("OFFSET", commands.Commands[1], StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task CreateAsync_RealProviderPage_RoundTripsThroughOriginalHttpContentFormatter()
    {
        await using var context = fixture.CreateContext();
        var source = context.Rows.AsNoTracking().OrderBy(row => row.Id).Select(row => row.Name);
        var produced = await CreateAsync(source, pageIndex: 2, pageSize: 3);
        Assert.NotNull(produced);
        using var content = new StringContent(JsonSerializer.Serialize(produced, produced.GetType()),
            Encoding.UTF8, "application/json");
        var consumed = await content.ReadAsAsync(produced.GetType());
        Assert.NotNull(consumed);
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(consumed, consumed.GetType()));
        var root = document.RootElement;
        Assert.Equal(new[] { "D", "E", "F" },
            root.GetProperty("Items").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(2, root.GetProperty("PageIndex").GetInt32());
        Assert.Equal(3, root.GetProperty("TotalPages").GetInt32());
        Assert.Equal(7, root.GetProperty("TotalRecords").GetInt32());
        Assert.True(root.GetProperty("HasPreviousPage").GetBoolean());
        Assert.True(root.GetProperty("HasNextPage").GetBoolean());
    }

    [Fact]
    public async Task CreateAsync_RealProviderEmptyQuery_ReturnsOriginalNullResult()
    {
        var commands = new RetainedPaginationCommandRecorder();
        await using var context = fixture.CreateContext(commands);
        var source = context.Rows.AsNoTracking().Where(row => row.Id < 0).Select(row => row.Name);
        Assert.Null(await CreateAsync(source, pageIndex: 1, pageSize: 3));
        Assert.Contains("count(*)", Assert.Single(commands.Commands), StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<object?> CreateAsync(IQueryable<string> source, int pageIndex, int pageSize)
    {
        var definition = Assert.IsAssignableFrom<Type>(typeof(PermissionRegistrationRequest).Assembly
            .GetType("Maliev.Entities.ViewModels.PaginatedListWebApi`1"));
        Assert.True(definition.IsPublic);
        var type = definition.MakeGenericType(typeof(string));
        var method = Assert.IsAssignableFrom<MethodInfo>(type.GetMethod("CreateAsync", BindingFlags.Public | BindingFlags.Static));
        var task = Assert.IsAssignableFrom<Task>(method.Invoke(null, [source, pageIndex, pageSize]));
        await task.WaitAsync(TimeSpan.FromSeconds(30));
        return Assert.IsAssignableFrom<PropertyInfo>(task.GetType().GetProperty("Result")).GetValue(task);
    }
}

[CollectionDefinition("Retained pagination PostgreSQL", DisableParallelization = true)]
public sealed class RetainedPaginationPostgresCollection;

public sealed class RetainedPaginationPostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();

    public async Task InitializeAsync()
    {
        using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        await _postgres.StartAsync(deadline.Token);
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync(deadline.Token);
        context.Rows.AddRange("ABCDEFG".Select(name => new RetainedPaginationRow { Name = name.ToString() }));
        await context.SaveChangesAsync(deadline.Token);
    }

    public RetainedPaginationQueryContext CreateContext(DbCommandInterceptor? interceptor = null)
    {
        var builder = new DbContextOptionsBuilder<RetainedPaginationQueryContext>()
            .UseNpgsql(_postgres.GetConnectionString(), options => options.CommandTimeout(30));
        if (interceptor is not null) builder.AddInterceptors(interceptor);
        return new(builder.Options);
    }

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();
}

public sealed class RetainedPaginationQueryContext(DbContextOptions<RetainedPaginationQueryContext> options) : DbContext(options)
{
    public DbSet<RetainedPaginationRow> Rows => Set<RetainedPaginationRow>();
}

public sealed class RetainedPaginationRow
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class RetainedPaginationCommandRecorder : DbCommandInterceptor
{
    public List<string> Commands { get; } = [];

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Commands.Add(command.CommandText);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }
}
