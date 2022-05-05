namespace Sho8lana.Models.ViewModels
{
    public class ChatViewModel
    {
        public ChatViewModel()
        {
            Messages = new List<ServiceMessage>();
        }
        public string SenderId { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverId { get; set; }
        public bool IsReceiverOnline { get; set; }
        public string ServiceTitle { get; set; }
        public int ServiceId { get; set; }
        public IEnumerable<ServiceMessage> Messages { get; set; }
    }
}
