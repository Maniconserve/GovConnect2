namespace GovConnect.Chat
{
    public class ChatService
    {
        private readonly SqlServerDbContext _context;

        public ChatService(SqlServerDbContext context)
        {
            _context = context;
        }

        // Method to save a chat message
        public async Task SaveMessageAsync(string senderId, string receiverId, string grievanceId, string message, string name)
        {
            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                GrievanceId = int.Parse(grievanceId),
                Message = message,
                Timestamp = DateTime.UtcNow,
                Name = name
            };

            // Add the message to the database
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();  // Commit changes to the database
        }

        // Optional: Get all messages for a specific grievance
        public async Task<List<ChatMessage>> GetMessagesAsync(string grievanceId)
        {
            return await _context.ChatMessages
                .Where(m => m.GrievanceId == int.Parse(grievanceId))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();  // Fetch all chat messages for the given grievance
        }
    }

}
