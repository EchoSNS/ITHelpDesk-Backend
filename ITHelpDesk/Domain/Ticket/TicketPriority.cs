using System.ComponentModel;

public enum TicketPriority
{
    [Description("Low Priority")]
    Low,

    [Description("Medium Priority")]
    Medium,

    [Description("High Priority")]
    High,

    [Description("Critical Priority")]
    Critical
}
