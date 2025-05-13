using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ITHelpDesk.Models;
using ITHelpDesk.Repositories;
using System.Text.Json;

namespace ITHelpDesk.Services
{
    public class SmsService : ISmsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;
        private readonly string _apiUrl;
        private readonly string _email;
        private readonly string _password;

        public SmsService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<SmsService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // Load configuration from appsettings.json
            _apiUrl = _configuration["SmsApi:Url"] ?? "https://api.itexmo.com/api/broadcast";
            _email = _configuration["SmsApi:Email"] ?? throw new ArgumentNullException("SmsApi:Email is not configured");
            _password = _configuration["SmsApi:Password"] ?? throw new ArgumentNullException("SmsApi:Password is not configured");
        }

        public async Task<bool> SendSmsAsync(string recipient, string message)
        {
            try
            {
                // Create JSON payload
                var content = new
                {
                    Recipients = recipient,
                    Message = message,
                    ApiCode = "CHANGE_TO_API_CODE" // Empty string as it's not required in the new API
                };

                // Convert content to JSON
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(content),
                    System.Text.Encoding.UTF8,
                    "application/json");

                // Set up Basic Authentication
                var authToken = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{_email}:{_password}"));
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                // Set Accept header
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                // Send request
                var response = await _httpClient.PostAsync(_apiUrl, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseData = JsonSerializer.Deserialize<ApiResponseModel>(responseString);

                    // Log the response status
                    _logger.LogInformation($"SMS sent to {recipient}. Status: {responseData?.Status}, Message: {responseData?.Message}");

                    return responseData?.Status == "success";
                }
                else
                {
                    _logger.LogError($"Failed to send SMS. Status code: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while sending SMS to {recipient}");
                return false;
            }
            finally
            {
                // Clear the headers to prevent them from affecting other requests
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        // Method to notify admin and IT users when a ticket is created
        public async Task NotifyTicketCreationAsync(string ticketId, string ticketTitle, string staffUserName, List<string> itAdminPhoneNumbers)
        {
            var message = $"New Ticket Created - ID: {ticketId}\nTitle: {ticketTitle}\nCreated By: {staffUserName}\nPlease review and assign.";

            // Combine admin and IT phone numbers
            var allRecipients = new List<string>();
            allRecipients.AddRange(itAdminPhoneNumbers);

            // Send to all recipients as a batch
            foreach (var recipient in allRecipients)
            {
                await SendSmsAsync(recipient, message);
            }
        }

        // Method to notify staff and assigned user when a ticket is assigned
        public async Task NotifyTicketAssignmentAsync(string ticketId, string ticketTitle, string staffUserPhone, string assignedUserName, string assignedUserPhone)
        {
            // Notify staff
            var staffMessage = $"Your Ticket (ID: {ticketId}) - {ticketTitle} has been assigned to {assignedUserName}.";
            await SendSmsAsync(staffUserPhone, staffMessage);

            // Notify assigned user
            var assignedMessage = $"Ticket ID: {ticketId} - {ticketTitle} has been assigned to you. Please review and take action.";
            await SendSmsAsync(assignedUserPhone, assignedMessage);
        }

        // Method to notify staff when ticket status changes
        public async Task NotifyTicketStatusChangeAsync(string ticketId, string ticketTitle, string staffUserPhone, string oldStatus, string newStatus)
        {
            var message = $"Ticket Status Update - ID: {ticketId}\nTitle: {ticketTitle}\nStatus changed from {oldStatus} to {newStatus}.";
            await SendSmsAsync(staffUserPhone, message);
        }
    }
}
