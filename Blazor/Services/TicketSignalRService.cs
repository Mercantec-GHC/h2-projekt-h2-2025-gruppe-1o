using System;
using System.Threading.Tasks;
using DomainModels.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Blazor.Services
{
    public class TicketSignalRService : IAsyncDisposable
    {
        private HubConnection? _hubConnection;
        private readonly NavigationManager _navigationManager;
        private string? _currentTicketId;

        public event Action<TicketSummaryDto>? OnNewTicketReceived;
        public event Action<TicketMessageDto>? OnMessageReceived;
        public event Action<string, DomainModels.Enums.TicketStatus>? OnStatusChanged;

        public TicketSignalRService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public async Task StartConnectionAsync(string? authToken = null)
        {
            if (_hubConnection == null)
            {
                var hubUrl = _navigationManager.ToAbsoluteUri("/ticketHub");
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl, options =>
                    {
                        if (!string.IsNullOrEmpty(authToken))
                        {
                            options.AccessTokenProvider = () => Task.FromResult(authToken);
                        }
                    })
                    .WithAutomaticReconnect()
                    .Build();

                _hubConnection.On<TicketSummaryDto>("NewTicketCreated", (ticket) => OnNewTicketReceived?.Invoke(ticket));
                _hubConnection.On<TicketMessageDto>("ReceiveMessage", (message) => OnMessageReceived?.Invoke(message));

                // NY LISTENER
                _hubConnection.On<string, DomainModels.Enums.TicketStatus>("TicketStatusChanged",
                    (ticketId, newStatus) => OnStatusChanged?.Invoke(ticketId, newStatus));

                await _hubConnection.StartAsync();
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