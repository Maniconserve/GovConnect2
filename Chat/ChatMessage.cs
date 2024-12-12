namespace GovConnect.Chat
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public int GrievanceId { get; set; }
        public string Name { get; set; }
    }
}
