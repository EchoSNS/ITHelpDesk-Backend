namespace ITHelpDesk.Services
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string recipient, string message);
        Task NotifyTicketCreationAsync(string ticketId, string ticketTitle, string staffUserName, List<string> itAdminPhoneNumbers);
        Task NotifyTicketAssignmentAsync(string ticketId, string ticketTitle, string staffUserPhone, string assignedUserName, string assignedUserPhone);
        Task NotifyTicketStatusChangeAsync(string ticketId, string ticketTitle, string staffUserPhone, string oldStatus, string newStatus);
    }
}
