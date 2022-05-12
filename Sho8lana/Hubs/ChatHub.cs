using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Sho8lana.Models;
using Sho8lana.Unit_Of_Work;

namespace Sho8lana.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IUnitOfWork context;

        public ChatHub(IUnitOfWork context)
        {
            this.context = context;
        }
        public async Task SendMessage(string message,string receiverId,int serviceId)
        {
            string senderId = Context.UserIdentifier;
            var customer = await context.Customers.GetBy(c => c.Id == senderId);
            var senderName = $"{customer.FirstName} {customer.LastName}";
            ServiceMessage serviceMessage = new ServiceMessage()
            {
                MessageContent = message,
                ReceiverId = receiverId,
                CustomerId = senderId,
                MessageDate = DateTime.Now,
                ServiceId = serviceId,
            };
            var date = DateTime.Now.ToShortTimeString();
            context.ServiceMessages.Add(serviceMessage);
            await context.complete();
            var receiverConnectionIds = context.OnlineUsers.GetAllBy(u => u.UserId == receiverId).Result.Select(u => u.ConnectionId);
            if(receiverConnectionIds != null)
            {
                foreach (var receiverConnectionId in receiverConnectionIds)
                {
                    await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", message, senderId, senderName, date);
                    
                }
            }
            await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", message, senderId,senderName, date);
            

        }
        public async Task SendNotification(string receiverId, string content)
        {
            var receiverConnectionIds = context.OnlineUsers.GetAllBy(u => u.UserId == receiverId).Result.Select(u => u.ConnectionId);
            if (receiverConnectionIds != null)
            {
                foreach (var receiverConnectionId in receiverConnectionIds)
                {
                    await Clients.Client(receiverConnectionId).SendAsync("ReceiveNotification", content);

                }
            }
        }
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine(Context.UserIdentifier,Context.ConnectionId);
            context.OnlineUsers.Add(new OnlineUser() 
            { 
                ConnectionId = Context.ConnectionId,UserId=Context.UserIdentifier
            });
            await context.complete();
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var currentUser = await context.OnlineUsers.GetBy(u => u.ConnectionId == Context.ConnectionId && u.UserId == Context.UserIdentifier);
            context.OnlineUsers.Delete(currentUser);
            await context.complete();
            await base.OnDisconnectedAsync(exception);
        }
    }
}
