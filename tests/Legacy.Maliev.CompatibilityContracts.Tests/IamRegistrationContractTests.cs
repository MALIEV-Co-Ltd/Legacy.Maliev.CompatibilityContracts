using System.Text.Json;
using Maliev.MessagingContracts.Contracts.Iam;
using Maliev.MessagingContracts.Contracts.Shared;
using MassTransit;

namespace Legacy.Maliev.CompatibilityContracts.Tests;

public sealed class IamRegistrationContractTests
{
    [Fact]
    public void MessageTypeOrdinalsAndNamesRemainStable()
    {
        Assert.Equal(
            ["Command", "Event", "Request", "Response"],
            Enum.GetNames<MessageType>());
        Assert.Equal([0, 1, 2, 3], Enum.GetValues<MessageType>().Select(value => (int)value).ToArray());
    }

    [Fact]
    public void PermissionRegistrationRequestRetainsExactClrAndMassTransitIdentity()
    {
        Assert.Equal(
            "Maliev.MessagingContracts.Contracts.Iam.PermissionRegistrationRequest",
            typeof(PermissionRegistrationRequest).FullName);
        Assert.Equal(
            "urn:message:Maliev.MessagingContracts.Contracts.Iam:PermissionRegistrationRequest",
            MessageUrn.ForTypeString<PermissionRegistrationRequest>());
    }

    [Fact]
    public void PermissionRegistrationRequestRetainsExactPublicPropertySurface()
    {
        Assert.Equal(
            [
                "CausationId", "ConsumedBy", "CorrelationId", "IsPublic",
                "MessageId", "MessageName", "MessageType", "MessageVersion",
                "OccurredAtUtc", "Permissions", "PublishedBy", "Roles", "ServiceName",
            ],
            typeof(PermissionRegistrationRequest)
                .GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }

    [Fact]
    public void JsonRoundTripRetainsExactWireNamesAndValues()
    {
        var occurredAt = new DateTimeOffset(2026, 7, 17, 8, 30, 0, TimeSpan.Zero);
        var message = new PermissionRegistrationRequest(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "permission-registration",
            MessageType.Command,
            "1.0.0",
            "legacy-order",
            ["iam"],
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            null,
            occurredAt,
            false,
            "legacy-order",
            [new("legacy-orders.orders.read", "Read orders")],
            [new("legacy-order-reader", "Order reader", ["legacy-orders.orders.read"])]);

        var json = JsonSerializer.Serialize(message);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(
            [
                "causationId", "consumedBy", "correlationId", "isPublic", "messageId",
                "messageName", "messageType", "messageVersion", "occurredAtUtc",
                "permissions", "publishedBy", "roles", "serviceName",
            ],
            root.EnumerateObject().Select(property => property.Name).Order(StringComparer.Ordinal).ToArray());
        Assert.Equal("Command", root.GetProperty("messageType").GetString());
        Assert.Equal("legacy-orders.orders.read", root.GetProperty("permissions")[0].GetProperty("permissionId").GetString());
        Assert.Equal("legacy-order-reader", root.GetProperty("roles")[0].GetProperty("roleId").GetString());

        var roundTrip = Assert.IsType<PermissionRegistrationRequest>(
            JsonSerializer.Deserialize<PermissionRegistrationRequest>(json));
        Assert.Equal(message.MessageId, roundTrip.MessageId);
        Assert.Equal(message.MessageType, roundTrip.MessageType);
        Assert.Equal(message.ServiceName, roundTrip.ServiceName);
        Assert.Equal(message.Permissions.Single(), roundTrip.Permissions.Single());
        Assert.Equal(message.Roles.Single().RoleId, roundTrip.Roles.Single().RoleId);
        Assert.Equal(message.Roles.Single().Description, roundTrip.Roles.Single().Description);
        Assert.Equal(message.Roles.Single().PermissionIds, roundTrip.Roles.Single().PermissionIds);
    }

    [Fact]
    public void ParameterlessConstructorsRetainSafeCollectionDefaults()
    {
        var message = new PermissionRegistrationRequest();
        Assert.NotNull(message.ConsumedBy);
        Assert.NotNull(message.Permissions);
        Assert.NotNull(message.Roles);
        Assert.Empty(message.ConsumedBy);
        Assert.Empty(message.Permissions);
        Assert.Empty(message.Roles);
    }
}
