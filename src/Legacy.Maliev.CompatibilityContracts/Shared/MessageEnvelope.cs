using System.Text.Json.Serialization;

namespace Maliev.MessagingContracts.Contracts.Shared;

/// <summary>The base structure that every retained legacy message contract follows.</summary>
public record BaseMessage(
    [property: JsonPropertyName("messageId")] Guid MessageId,
    [property: JsonPropertyName("messageName")] string MessageName,
    [property: JsonPropertyName("messageType")] MessageType MessageType,
    [property: JsonPropertyName("messageVersion")] string MessageVersion,
    [property: JsonPropertyName("publishedBy")] string PublishedBy,
    [property: JsonPropertyName("consumedBy")] IReadOnlyList<string> ConsumedBy,
    [property: JsonPropertyName("correlationId")] Guid CorrelationId,
    [property: JsonPropertyName("causationId")] Guid? CausationId,
    [property: JsonPropertyName("occurredAtUtc")] DateTimeOffset OccurredAtUtc,
    [property: JsonPropertyName("isPublic")] bool IsPublic)
{
    /// <summary>Creates an empty instance for serialization compatibility.</summary>
    public BaseMessage()
        : this(default, string.Empty, default, string.Empty, string.Empty, [], default, default, default, default)
    {
    }
}

/// <summary>Defines the stable legacy message categories.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageType
{
    /// <summary>A request to perform an action.</summary>
    Command,
    /// <summary>A notification of a fact that occurred.</summary>
    Event,
    /// <summary>A request for information.</summary>
    Request,
    /// <summary>A response to a request.</summary>
    Response,
}
