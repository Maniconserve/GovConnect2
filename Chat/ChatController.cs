namespace GovConnect.Chat
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        // GET api/chat/messages/{grievanceId}
        [HttpGet("messages/{grievanceId}")]
        public async Task<IActionResult> GetMessages(string grievanceId)
        {
            var messages = await _chatService.GetMessagesAsync(grievanceId);
            return Ok(messages);  // Return messages as JSON
        }
    }

}
