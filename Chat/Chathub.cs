using Microsoft.AspNetCore.SignalR;
namespace GovConnect.Chat
{
    public class ChatHub : Hub
    {
        private readonly ChatService _chatService;

        public ChatHub(ChatService chatService)
        {
            _chatService = chatService;
        }

        // Send message to a group based on grievance ID
        public async Task SendMessage(string grievanceId,String name, string officerId, string message)
        {
            var senderId = Context.UserIdentifier;  // Get the UserId (Citizen or Officer)

            // Save message to database
            await _chatService.SaveMessageAsync(senderId, officerId, grievanceId, message, name);

            // Broadcast the message to the relevant group
            await Clients.Group(grievanceId).SendAsync("ReceiveMessage", senderId, name, message);
        }

        // Join a specific grievance chat group
        public async Task JoinChat(string grievanceId)
        {
            // Add user to the group
            await Groups.AddToGroupAsync(Context.ConnectionId, grievanceId);
            Console.WriteLine($"User {Context.UserIdentifier} joined group {grievanceId}");
        }

        // Leave the grievance chat group
        public async Task LeaveChat(string grievanceId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, grievanceId);
            Console.WriteLine($"User {Context.UserIdentifier} left group {grievanceId}");
        }
    }
}
