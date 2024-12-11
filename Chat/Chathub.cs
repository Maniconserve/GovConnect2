using Microsoft.AspNetCore.SignalR;
namespace GovConnect.Chat
{
    public class ChatHub : Hub
    {
        // Send message to a specific user (officer or user)
        public async Task SendMessage(string receiverId, string message)
        {
            await Clients.User(receiverId).SendAsync("ReceiveMessage", message);
        }

        // Method to add a user to a "group" (this could be a chat room for a grievance)
        public async Task JoinChat(string grievanceId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, grievanceId);
        }

        // Method to leave a chat group
        public async Task LeaveChat(string grievanceId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, grievanceId);
        }
    }

}
