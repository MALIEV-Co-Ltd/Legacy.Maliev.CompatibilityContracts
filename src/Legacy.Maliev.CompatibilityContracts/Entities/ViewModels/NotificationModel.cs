using Maliev.Common.Enumerations;

namespace Maliev.Entities.ViewModels;

/// <summary>Retains the mutable notification used by legacy member views.</summary>
public class NotificationModel
{
    /// <summary>Gets or sets the notification content, initially null.</summary>
    public string? Content { get; set; }

    /// <summary>Gets or sets severity, initially Information.</summary>
    public Severity Severity { get; set; }
}
