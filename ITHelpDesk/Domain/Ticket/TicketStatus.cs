using System.ComponentModel;

public enum TicketStatus
{
    [Description("New Ticket")]
    New,

    [Description("Assigned to an IT")]
    Assigned,

    [Description("Currently Being Worked On")]
    InProgress,

    [Description("Resolved and Awaiting Confirmation")]
    Resolved,

    [Description("Closed and Completed")]
    Closed
}
