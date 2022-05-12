using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Sho8lana.Hubs;
using Sho8lana.Models;
using Sho8lana.Unit_Of_Work;

namespace Sho8lana.Hangfire
{
    public class ContractJobs
    {
        private readonly IUnitOfWork context;
        private readonly IHubContext<ChatHub> hubContext;
        public ContractJobs(IUnitOfWork context, IHubContext<ChatHub> hubContext)
        {
            this.context = context;
            this.hubContext = hubContext;
        }
        public async Task EndContract(int contractId)
        {
            //var contract = await context.Contracts.GetById(contractId);
            var contract = await context.Contracts.GetEagerLodingAsync(c => c.ContractId == contractId, new string[] { "Service" });
            if (!contract.IsDone && !contract.IsCanceled)
            {
                if (contract.SellerIsDone)
                {
                    await AddPendingBalance(contract.SellerId, contract.ContractPrice);
                    var jobId = BackgroundJob.Schedule(() => AddBalanceFromPending(contract.SellerId, contract.ContractPrice),TimeSpan.FromDays(14));
                    await AddNotification(contract.SellerId, $"تهانينا لقد تم تحويل {contract.ContractPrice}$" +
                        $" الى حسابك لإتمامك خدمة '{contract.Service.Title}'");
                    contract.JobId = jobId;
                    contract.IsDone = true;
                }
                else
                {
                    await AddBalance(contract.BuyerId, contract.ContractPrice);
                    await AddNotification(contract.SellerId, $"تم ارجاع {contract.ContractPrice}$" +
                        $" الى حسابك بسبب عدم تسليم البائع الخدمة  '{contract.Service.Title}' في الوقت المحدد");
                    contract.IsCanceled = true;
                }
                //context.Contracts.Update(contract);
                await context.complete();
            }
        }
        public async Task AddBalanceFromPending(string sellerId, float balance)
        {
            var seller = await context.Customers.GetById(sellerId);
            seller.PendingBalance -= balance;
            seller.Balance += balance;
            await AddNotification(sellerId, $"تهانينا لقد تم تحويل {balance}$ من رصيدك المعلق لرصيدك القابل للسحب");
            //context.Customers.Update(seller);
            await context.complete();
        }
        public async Task AddPendingBalance(string sellerId, float balance)
        {
            var seller = await context.Customers.GetById(sellerId);
            seller.PendingBalance += balance;
            //context.Customers.Update(seller);

        }
        public async Task AddBalance(string buyerId, float balance)
        {
            var buyer = await context.Customers.GetById(buyerId);
            buyer.Balance += balance;
            //context.Customers.Update(buyer);

        }
        public async Task AddNotification(string customerId, string content)
        {
            Notification notification = new Notification()
            {
                CustomerId = customerId,
                Content = content
            };
            context.Notifications.Add(notification);
            await SendNotification(customerId, content);
        }
        public async Task SendNotification(string receiverId, string content)
        {
            var receiverConnectionIds = context.OnlineUsers.GetAllBy(u => u.UserId == receiverId).Result.Select(u => u.ConnectionId);
            if (receiverConnectionIds != null)
            {
                foreach (var receiverConnectionId in receiverConnectionIds)
                {
                    await hubContext.Clients.Client(receiverConnectionId).SendAsync("ReceiveNotification", content);

                }
            }
        }
        public async Task CalculateOverallRate(int serviceId, int newRate)
        {
            var service = await context.Services.GetEagerLodingAsync(s => s.ServiceId == serviceId, new string[] { "Contracts" });
            if (service != null)
            {
                if (service.Rate == default)
                {
                    service.Rate = newRate;
                }
                else
                {
                    var numOfRatedContracts = service.Contracts.Where(c => c.ContractRateDone).Count()+1;//adding the new one
                    var numOfAllStars = service.Contracts.Where(c => c.ContractRateDone).Sum(c => c.ContractRateStars)+newRate;
                    service.Rate = (float)numOfAllStars / numOfRatedContracts;
                }
                //context.Services.Update(service);
            }

        }
    }
}
