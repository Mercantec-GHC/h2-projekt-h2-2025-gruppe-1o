using System;
using System.Net.Http;
using System.Threading.Tasks;
using DomainModels.DTOs;
using DomainModels.Enums;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Blazor.Services
{
    public class TicketSignalRService : IAsyncDisposable
    {
        private HubConnection? _hubConnection;
        private readonly HttpClient _httpClient;
        private string? _currentTicketId;

        public event Action<TicketSummaryDto>? OnNewTicketReceived;
        public event Action<TicketMessageDto>? OnMessageReceived;
        public event Action<string, TicketStatus>? OnStatusChanged;

        public TicketSignalRService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task StartConnectionAsync(string? authToken = null)
        {
            if (_hubConnection == null)
            {
                var hubUrl = new Uri(_httpClient.BaseAddress!, $"ticketHub?access_token={authToken}").ToString();

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl)
                    .WithAutomaticReconnect()
                    .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Debug))
                    .Build();

                _hubConnection.On<TicketSummaryDto>("NewTicketCreated", (ticket) => OnNewTicketReceived?.Invoke(ticket));
                _hubConnection.On<TicketMessageDto>("ReceiveMessage", (message) => OnMessageReceived?.Invoke(message));
                _hubConnection.On<string, TicketStatus>("TicketStatusChanged",
                    (ticketId, newStatus) => OnStatusChanged?.Invoke(ticketId, newStatus));

                try
                {
                    await _hubConnection.StartAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SignalR Connection failed: {ex.Message}");
                }
            }
        }

        public async Task JoinTicketGroupAsync(string ticketId)
        {
            if (_hubConnection is not null && IsConnected)
            {
                await _hubConnection.InvokeAsync("JoinTicketGroup", ticketId);
                _currentTicketId = ticketId;
            }
        }

        public async Task LeaveTicketGroupAsync(string ticketId)
        {
            if (_hubConnection is not null && IsConnected)
            {
                await _hubConnection.InvokeAsync("LeaveTicketGroup", ticketId);
                _currentTicketId = null;
            }
        }

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null && _currentTicketId is not null)
            {
                await LeaveTicketGroupAsync(_currentTicketId);
            }
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}