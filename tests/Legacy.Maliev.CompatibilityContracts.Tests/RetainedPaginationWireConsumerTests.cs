using System.Reflection;
using System.Text;
using Maliev.MessagingContracts.Contracts.Iam;

namespace Legacy.Maliev.CompatibilityContracts.Tests;

public sealed class RetainedPaginationWireConsumerTests
{
    // Committed source WebApiService.GetAs<T> uses HttpContent.ReadAsAsync<T>;
    // retain that actual formatter package/version instead of substituting STJ.
    [Theory]
    [InlineData("{\"items\":[\"รายการลูกค้า\"],\"pageIndex\":2,\"totalPages\":3,\"totalRecords\":51,\"hasNextPage\":true,\"hasPreviousPage\":true}")]
    [InlineData("{\"Items\":[\"รายการลูกค้า\"],\"PageIndex\":2,\"TotalPages\":3,\"TotalRecords\":51,\"HasNextPage\":true,\"HasPreviousPage\":true}")]
    public async Task OriginalHttpContentFormatter_RetainsPageCountsItemsAndNavigation(string payload)
    {
        var definition = Assert.IsAssignableFrom<Type>(typeof(PermissionRegistrationRequest).Assembly
            .GetType("Maliev.Entities.ViewModels.PaginatedListWebApi`1"));
        var retainedType = definition.MakeGenericType(typeof(string));
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var page = await content.ReadAsAsync(retainedType);
        Assert.NotNull(page);
        Assert.Equal(new[] { "รายการลูกค้า" }, Assert.IsType<List<string>>(Read(page, "Items")));
        Assert.Equal(2, Read(page, "PageIndex"));
        Assert.Equal(3, Read(page, "TotalPages"));
        Assert.Equal(51, Read(page, "TotalRecords"));
        Assert.Equal(true, Read(page, "HasPreviousPage"));
        Assert.Equal(true, Read(page, "HasNextPage"));
    }

    [Fact]
    public async Task OriginalHttpContentFormatter_DoesNotTrustIncomingComputedNavigationFlags()
    {
        var definition = Assert.IsAssignableFrom<Type>(typeof(PermissionRegistrationRequest).Assembly
            .GetType("Maliev.Entities.ViewModels.PaginatedListWebApi`1"));
        using var content = new StringContent(
            "{\"items\":[],\"pageIndex\":1,\"totalPages\":1,\"totalRecords\":0,\"hasNextPage\":true,\"hasPreviousPage\":true}",
            Encoding.UTF8, "application/json");
        var page = await content.ReadAsAsync(definition.MakeGenericType(typeof(string)));
        Assert.NotNull(page);
        Assert.Empty(Assert.IsType<List<string>>(Read(page, "Items")));
        Assert.Equal(false, Read(page, "HasPreviousPage"));
        Assert.Equal(false, Read(page, "HasNextPage"));
    }

    private static object? Read(object instance, string name) =>
        Assert.IsAssignableFrom<PropertyInfo>(instance.GetType().GetProperty(name)).GetValue(instance);

    [Fact]
    public async Task OriginalHttpContentFormatter_JsonNullRemainsNull()
    {
        var definition = Assert.IsAssignableFrom<Type>(typeof(PermissionRegistrationRequest).Assembly
            .GetType("Maliev.Entities.ViewModels.PaginatedListWebApi`1"));
        using var content = new StringContent("null", Encoding.UTF8, "application/json");
        Assert.Null(await content.ReadAsAsync(definition.MakeGenericType(typeof(string))));
    }

    [Fact]
    public async Task OriginalHttpContentFormatter_OmittedFieldsRetainConstructorDefaults()
    {
        var definition = Assert.IsAssignableFrom<Type>(typeof(PermissionRegistrationRequest).Assembly
            .GetType("Maliev.Entities.ViewModels.PaginatedListWebApi`1"));
        using var content = new StringContent("{}", Encoding.UTF8, "application/json");
        var page = await content.ReadAsAsync(definition.MakeGenericType(typeof(string)));
        Assert.NotNull(page);
        Assert.Empty(Assert.IsType<List<string>>(Read(page, "Items")));
        Assert.Equal(0, Read(page, "PageIndex"));
        Assert.Equal(0, Read(page, "TotalPages"));
        Assert.Equal(0, Read(page, "TotalRecords"));
        Assert.Equal(false, Read(page, "HasPreviousPage"));
        Assert.Equal(false, Read(page, "HasNextPage"));
    }

    [Fact]
    public async Task OriginalHttpContentFormatter_ExplicitNullItemsRemainsNullRatherThanInventingACollection()
    {
        var definition = Assert.IsAssignableFrom<Type>(typeof(PermissionRegistrationRequest).Assembly
            .GetType("Maliev.Entities.ViewModels.PaginatedListWebApi`1"));
        using var content = new StringContent("{\"items\":null,\"pageIndex\":1,\"totalPages\":1,\"totalRecords\":0}",
            Encoding.UTF8, "application/json");
        var page = await content.ReadAsAsync(definition.MakeGenericType(typeof(string)));
        Assert.NotNull(page);
        Assert.Null(Read(page, "Items"));
        Assert.Equal(false, Read(page, "HasPreviousPage"));
        Assert.Equal(false, Read(page, "HasNextPage"));
    }
}
