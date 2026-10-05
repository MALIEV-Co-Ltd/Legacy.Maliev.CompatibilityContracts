using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using Maliev.Common.Enumerations;
using Maliev.MessagingContracts.Contracts.Iam;

namespace Legacy.Maliev.CompatibilityContracts.Tests;

public sealed class RetainedEntityHelperContractTests
{
    [Fact]
    public void GeneratedDocumentation_AccompaniesRetainedEntityTypesAndQueryParameters()
    {
        var path = Path.ChangeExtension(typeof(PermissionRegistrationRequest).Assembly.Location, ".xml");
        Assert.True(File.Exists(path));
        var members = XDocument.Load(path).Descendants("member").ToDictionary(
            member => Assert.IsType<string>(member.Attribute("name")?.Value), StringComparer.Ordinal);
        foreach (var name in new[]
        {
            "T:Maliev.Entities.ViewModels.NotificationModel",
            "P:Maliev.Entities.ViewModels.NotificationModel.Content",
            "P:Maliev.Entities.ViewModels.NotificationModel.Severity",
            "T:Maliev.Entities.ViewModels.PaginatedListWebApi`1",
            "M:Maliev.Entities.ViewModels.PaginatedListWebApi`1.#ctor",
            "M:Maliev.Entities.ViewModels.PaginatedListWebApi`1.#ctor(System.Collections.Generic.List{`0},System.Int32,System.Int32,System.Int32)",
            "P:Maliev.Entities.ViewModels.PaginatedListWebApi`1.Items",
            "P:Maliev.Entities.ViewModels.PaginatedListWebApi`1.PageIndex",
            "P:Maliev.Entities.ViewModels.PaginatedListWebApi`1.TotalPages",
            "P:Maliev.Entities.ViewModels.PaginatedListWebApi`1.TotalRecords",
            "P:Maliev.Entities.ViewModels.PaginatedListWebApi`1.HasNextPage",
            "P:Maliev.Entities.ViewModels.PaginatedListWebApi`1.HasPreviousPage",
            "M:Maliev.Entities.ViewModels.PaginatedListWebApi`1.CreateAsync(System.Linq.IQueryable{`0},System.Int32,System.Int32)",
        })
        {
            Assert.True(members.TryGetValue(name, out var member), $"Missing generated XML member {name}");
            Assert.NotNull(member);
            Assert.False(string.IsNullOrWhiteSpace(member.Element("summary")?.Value));
        }

        var query = members["M:Maliev.Entities.ViewModels.PaginatedListWebApi`1.CreateAsync(System.Linq.IQueryable{`0},System.Int32,System.Int32)"];
        Assert.Equal(new[] { "source", "pageIndex", "pageSize" },
            query.Elements("param").Select(parameter => parameter.Attribute("name")?.Value));
        Assert.Contains("null", query.Element("returns")?.Value ?? string.Empty, StringComparison.Ordinal);
    }

    private static Type EntityType(string name)
    {
        var type = Assert.IsAssignableFrom<Type>(typeof(PermissionRegistrationRequest).Assembly.GetType(name));
        Assert.True(type.IsPublic);
        Assert.False(type.IsSealed);
        Assert.Equal(typeof(object), type.BaseType);
        return type;
    }

    [Fact]
    public void NotificationModel_RetainsOriginalDefaultsMutableShapeAndJson()
    {
        var type = EntityType("Maliev.Entities.ViewModels.NotificationModel");
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Assert.Equal(new[] { "Content", "Severity" }, properties.Select(property => property.Name));
        var content = Assert.IsAssignableFrom<PropertyInfo>(type.GetProperty("Content"));
        var severity = Assert.IsAssignableFrom<PropertyInfo>(type.GetProperty("Severity"));
        Assert.Equal(typeof(string), content.PropertyType);
        Assert.Equal(typeof(Severity), severity.PropertyType);
        Assert.True(content.CanRead && content.CanWrite);
        Assert.True(severity.CanRead && severity.CanWrite);
        var value = Assert.IsAssignableFrom<object>(Activator.CreateInstance(type));
        Assert.Null(content.GetValue(value));
        Assert.Equal(Severity.Information, severity.GetValue(value));
        Assert.Equal("{\"Content\":null,\"Severity\":0}", JsonSerializer.Serialize(value, type));

        content.SetValue(value, "ข้อความแจ้งเตือน");
        severity.SetValue(value, Severity.Warning);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(value, type));
        Assert.Equal("ข้อความแจ้งเตือน", json.RootElement.GetProperty("Content").GetString());
        Assert.Equal(1, json.RootElement.GetProperty("Severity").GetInt32());
        var restored = Assert.IsAssignableFrom<object>(JsonSerializer.Deserialize(json.RootElement.GetRawText(), type));
        Assert.Equal("ข้อความแจ้งเตือน", content.GetValue(restored));
        Assert.Equal(Severity.Warning, severity.GetValue(restored));
    }

    [Fact]
    public void Pagination_DefaultConstructorAndMutableMembers_RetainOriginalSurface()
    {
        var definition = EntityType("Maliev.Entities.ViewModels.PaginatedListWebApi`1");
        Assert.True(definition.IsGenericTypeDefinition);
        var type = definition.MakeGenericType(typeof(string));
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Assert.Equal(new[] { "HasNextPage", "HasPreviousPage", "Items", "PageIndex", "TotalPages", "TotalRecords" },
            properties.Select(property => property.Name).Order(StringComparer.Ordinal));
        var value = Assert.IsAssignableFrom<object>(Activator.CreateInstance(type));
        var items = Assert.IsAssignableFrom<PropertyInfo>(type.GetProperty("Items"));
        Assert.Equal(typeof(List<string>), items.PropertyType);
        Assert.Empty(Assert.IsType<List<string>>(items.GetValue(value)));
        foreach (var name in new[] { "PageIndex", "TotalPages", "TotalRecords" })
        {
            var property = Assert.IsAssignableFrom<PropertyInfo>(type.GetProperty(name));
            Assert.True(property.CanWrite);
            Assert.Equal(typeof(int), property.PropertyType);
            Assert.Equal(0, property.GetValue(value));
        }
        foreach (var name in new[] { "HasNextPage", "HasPreviousPage" })
        {
            var property = Assert.IsAssignableFrom<PropertyInfo>(type.GetProperty(name));
            Assert.False(property.CanWrite);
            Assert.Equal(typeof(bool), property.PropertyType);
            Assert.Equal(false, property.GetValue(value));
        }
        var replacement = new List<string> { "retained item" };
        items.SetValue(value, replacement);
        Assert.Same(replacement, items.GetValue(value));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(123)]
    public void NotificationModel_RetainsNumericSeverityIncludingOriginalConsumerFallbackValues(int ordinal)
    {
        var type = EntityType("Maliev.Entities.ViewModels.NotificationModel");
        var value = Assert.IsAssignableFrom<object>(Activator.CreateInstance(type));
        var severity = Assert.IsAssignableFrom<PropertyInfo>(type.GetProperty("Severity"));
        severity.SetValue(value, (Severity)ordinal);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(value, type));
        Assert.Equal(ordinal, json.RootElement.GetProperty("Severity").GetInt32());
        var restored = Assert.IsAssignableFrom<object>(JsonSerializer.Deserialize(json.RootElement.GetRawText(), type));
        Assert.Equal((Severity)ordinal, severity.GetValue(restored));
    }

    [Theory]
    [InlineData(0, 1, 10, 0, false, false)]
    [InlineData(21, 0, 10, 3, true, false)]
    [InlineData(21, 1, 10, 3, true, false)]
    [InlineData(21, 2, 10, 3, true, true)]
    [InlineData(21, 3, 10, 3, false, true)]
    [InlineData(21, 4, 10, 3, false, true)]
    public void Pagination_CountConstructor_PreservesOneBasedNavigationAndJson(
        int count, int page, int size, int totalPages, bool next, bool previous)
    {
        var type = EntityType("Maliev.Entities.ViewModels.PaginatedListWebApi`1").MakeGenericType(typeof(string));
        var constructor = Assert.IsAssignableFrom<ConstructorInfo>(type.GetConstructor(
            [typeof(List<string>), typeof(int), typeof(int), typeof(int)]));
        Assert.Equal(new[] { "items", "count", "pageIndex", "pageSize" },
            constructor.GetParameters().Select(parameter => parameter.Name));
        var source = new List<string> { "รายการ" };
        var value = constructor.Invoke([source, count, page, size]);
        var copied = Assert.IsType<List<string>>(type.GetProperty("Items")!.GetValue(value));
        Assert.NotSame(source, copied);
        Assert.Equal(source, copied);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(value, type));
        var root = json.RootElement;
        Assert.Equal(new[] { "HasNextPage", "HasPreviousPage", "Items", "PageIndex", "TotalPages", "TotalRecords" },
            root.EnumerateObject().Select(property => property.Name).Order(StringComparer.Ordinal));
        Assert.Equal(count, root.GetProperty("TotalRecords").GetInt32());
        Assert.Equal(page, root.GetProperty("PageIndex").GetInt32());
        Assert.Equal(totalPages, root.GetProperty("TotalPages").GetInt32());
        Assert.Equal(next, root.GetProperty("HasNextPage").GetBoolean());
        Assert.Equal(previous, root.GetProperty("HasPreviousPage").GetBoolean());
    }

    [Fact]
    public void Pagination_CreateAsync_RetainsQueryableProviderMethodAndParameterShape()
    {
        var type = EntityType("Maliev.Entities.ViewModels.PaginatedListWebApi`1").MakeGenericType(typeof(string));
        var method = Assert.IsAssignableFrom<MethodInfo>(type.GetMethod("CreateAsync", BindingFlags.Public | BindingFlags.Static));
        Assert.Equal(new[] { typeof(IQueryable<string>), typeof(int), typeof(int) },
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(new[] { "source", "pageIndex", "pageSize" },
            method.GetParameters().Select(parameter => parameter.Name));
        Assert.Equal(typeof(Task<>).MakeGenericType(type), method.ReturnType);
    }
}
