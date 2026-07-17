using System.Text.Json.Serialization;
using Maliev.MessagingContracts.Contracts.Shared;

namespace Maliev.MessagingContracts.Contracts.Iam;

/// <summary>One permission advertised by a legacy service.</summary>
public record PermissionRegistrationRequestPermissionsItem(
    [property: JsonPropertyName("permissionId")] string PermissionId,
    [property: JsonPropertyName("description")] string Description)
{
    /// <summary>Creates an empty instance for serialization compatibility.</summary>
    public PermissionRegistrationRequestPermissionsItem() : this(string.Empty, string.Empty)
    {
    }
}

/// <summary>One role advertised by a legacy service.</summary>
public record PermissionRegistrationRequestRolesItem(
    [property: JsonPropertyName("roleId")] string RoleId,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("permissionIds")] IReadOnlyList<string> PermissionIds)
{
    /// <summary>Creates an empty instance for serialization compatibility.</summary>
    public PermissionRegistrationRequestRolesItem() : this(string.Empty, string.Empty, [])
    {
    }
}

/// <summary>Request published by a legacy service to register its permissions and roles with IAM.</summary>
public record PermissionRegistrationRequest(
    Guid MessageId,
    string MessageName,
    MessageType MessageType,
    string MessageVersion,
    string PublishedBy,
    IReadOnlyList<string> ConsumedBy,
    Guid CorrelationId,
    Guid? CausationId,
    DateTimeOffset OccurredAtUtc,
    bool IsPublic,
    [property: JsonPropertyName("serviceName")] string ServiceName,
    [property: JsonPropertyName("permissions")] IReadOnlyList<PermissionRegistrationRequestPermissionsItem> Permissions,
    [property: JsonPropertyName("roles")] IReadOnlyList<PermissionRegistrationRequestRolesItem> Roles)
    : BaseMessage(
        MessageId,
        MessageName,
        MessageType,
        MessageVersion,
        PublishedBy,
        ConsumedBy,
        CorrelationId,
        CausationId,
        OccurredAtUtc,
        IsPublic)
{
    /// <summary>Creates an empty instance for serialization compatibility.</summary>
    public PermissionRegistrationRequest()
        : this(
            default,
            string.Empty,
            default,
            string.Empty,
            string.Empty,
            [],
            default,
            default,
            default,
            default,
            string.Empty,
            [],
            [])
    {
    }
}
