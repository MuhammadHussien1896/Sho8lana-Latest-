using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Sho8lana.Hangfire;
using Sho8lana.Models;
using Sho8lana.Models.ViewModels;
using Sho8lana.Unit_Of_Work;


namespace Sho8lana.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IUnitOfWork context;
        private readonly UserManager<Customer> userManager;
        private readonly ContractJobs jobs;
        //private readonly HubConnection connection;

        public CustomerController(IUnitOfWork context,UserManager<Customer> userManager)
        {
            this.context = context;
            this.userManager = userManager;
            this.jobs = new ContractJobs(context);
            //connection = new HubConnectionBuilder()
            //                    .WithUrl("https://localhost:7009/chatHub", options =>
            //                    {
            //                        options.UseDefaultCredentials = true;
            //                    })
            //                    .Build();
            
        }
        public async Task<IActionResult> account(string id)
        {
            string customerId = userManager.GetUserId(User);
            var customer = await context.Customers.GetBy(c => c.Id == id);
            if (customer == null) { return NotFound(); }
            var services = await context.Services.GetAllEagerLodingAsync(s => s.CustomerId == customerId, new string[] { "Medias", "Contracts" });
            AccountViewModel model = new AccountViewModel() { Customer = customer,Services = services };
            return View(model);
            

        }
     
        public IActionResult Index()
        {
            return View();
        }
        

        //customer balance
        public  async Task< IActionResult> GetCustomerBalance()
        {
            string customerId = userManager.GetUserId(User);
            var customer = await context.Customers.GetBy(c => c.Id == customerId);
            if (customer == null)
            {
                return NotFound();
            }
                
            return View(customer);
        }

        //incoming and outgoing requests at the same action or page
        public async Task<IActionResult> CustomerRequests()
        {
            string customerId = userManager.GetUserId(User);
            var customer = await context.Customers.GetEagerLodingAsync(c => c.Id == customerId);
            if (customer != null)
            {
                var incomingRequests = await context.CustomerRequests
                                        .GetAllEagerLodingAsync(r => r.Service.CustomerId == customer.Id
                                        ,new string[] {"Service","Customer"});
                var outgoingRequests = await context.CustomerRequests.GetAllEagerLodingAsync(r => r.CustomerId == customerId, new string[] { "Service.Customer" });
                CustomerRequestViewModel model = new CustomerRequestViewModel()
                {
                    IncomingRequests = incomingRequests.ToList(),
                    OutgoingRequests = outgoingRequests.ToList()
                };
                return View(model);
            }
            else
            {
                return NotFound();
            }
        }
        public async Task<IActionResult> AcceptServiceRequest(int id)
        {
            string customerId = userManager.GetUserId(User);
            var customer = await context.Customers.GetBy(c => c.Id == customerId);
            var customerRequest = await context.CustomerRequests.GetEagerLodingAsync(r => r.RequestId == id,new string[] { "Service" });
            if(customerRequest == null || customer == null)
            {
                return NotFound();
            }
            if(customerRequest.Service.CustomerId != customerId)
            {
                return BadRequest();
            }
            var contract = AddContract(customerRequest);

            //add notification to the customer who sent the request to the service
            var content = $"{customer.FirstName} {customer.LastName} قد قبل طلبك للخدمة '{customerRequest.Service.Title}' !" +
                $"رجاءً تفقد العقود المنتظرة لبدء العمل";
            jobs.AddNotification(contract.CustomerId,
                $"{customer.FirstName} {customer.LastName} قد قبل طلبك للخدمة '{customerRequest.Service.Title}' !" +
                $"رجاءً تفقد العقود المنتظرة لبدء العمل");
            
            
            //delete customer request after accepting and become a contract
            await context.CustomerRequests.Delete(id);
            //saving changes in database
            await context.complete();
            //if (connection.State == HubConnectionState.Disconnected)
            //{
            //    await connection.StartAsync();
            //}
            //await connection.InvokeAsync("SendNotification", contract.CustomerId, content);

            return RedirectToAction(nameof(CustomerContracts));
        }

        public async Task<IActionResult> DeleteRequest(int id)
        {
            string customerId = userManager.GetUserId(User);
            var customerRequest = await context.CustomerRequests.GetEagerLodingAsync(r => r.RequestId == id, new string[] { "Service" });
            if (customerRequest != null)
            {
                if(customerRequest.CustomerId == customerId || customerRequest.Service.CustomerId == customerId)
                {
                    await context.CustomerRequests.Delete(id);
                    string content = $"لقد تم الغاء الطلب على الخدمة {customerRequest.Service.Title}";
                    jobs.AddNotification(customerRequest.Service.CustomerId,content);
                    jobs.AddNotification(customerRequest.CustomerId,content);
                    await context.complete();
                    return RedirectToAction(nameof(CustomerRequests));
                    
                }
                else
                {
                    return BadRequest();
                }
                
            }
            else
            {
                return BadRequest();
            }
             
        }

        public async Task<IActionResult> CustomerContracts()
        {
            string customerId = userManager.GetUserId(User);
            //var customer = await context.Customers.GetBy(c => c.Id == customerId);
            if (customerId != null)
            {

                
                var contracts = await context.Contracts
                                                .GetAllEagerLodingAsync(c => c.CustomerId == customerId || c.SericeOwnerId == customerId
                                                ,new string[] {"Customer","Service.Customer", "Complain" });
                ContractViewModel model = new ContractViewModel()
                {
                    PendingContracts            = contracts.Where(c => c.IsDone == false && c.StartDate == default && !(c.BuyerAccepted && c.SellerAccepted)).ToList(),
                    PendingPaymentContracts     = contracts.Where(c => c.IsDone == false && c.StartDate == default && (c.BuyerAccepted && c.SellerAccepted)).ToList(),
                    ActiveContracts             = contracts.Where(c => c.IsDone == false && c.StartDate != default && c.IsCanceled == false).ToList(),
                    CanceledContracts           = contracts.Where(c => c.IsCanceled == true).ToList(),
                    DoneContracts               = contracts.Where(c => c.IsDone == true).ToList(),
                    UserId                      = customerId
                };
                return View(model);
            }
            else
            {
                return NotFound();
            }
        }
        public async Task<IActionResult> AcceptContract(int id)
        {
            string customerId = userManager.GetUserId(User);
            var contract = await context.Contracts.GetById(id);
            if(contract != null)
            {
                if(contract.BuyerId == customerId)
                {
                    contract.BuyerAccepted = true;
                }
                else
                {
                    contract.SellerAccepted = true;
                }
                //context.Contracts.Update(contract);
                await context.complete();
                return RedirectToAction(nameof(CustomerContracts));
            }
            else
            {
                return NotFound();
            }
        }
        public async Task<IActionResult> ContractBuyerSellerDone(int id)
        {
            string customerId = userManager.GetUserId(User);
            var contract = await context.Contracts.GetEagerLodingAsync(c => c.ContractId == id,new string[] { "Service" });
            if (contract != null)
            {
                if (contract.BuyerId == customerId)
                {
                    //fire job before it's time
                    //BackgroundJob.Requeue(contract.JobId);
                    contract.EndDate = DateTime.Now;
                    contract.IsDone = true;

                    BackgroundJob.Delete(contract.JobId);
                    //after deleting the previos job we create a new job to transfere the price after 14 days to balance
                    var jobId = BackgroundJob.Schedule(() =>
                     jobs.AddBalanceFromPending(contract.SellerId, contract.ContractPrice)
                      , TimeSpan.FromDays(14));

                    contract.JobId = jobId;
                    //contract price goes to seller now
                    await jobs.AddPendingBalance(contract.SellerId, contract.ContractPrice);
                    var buyer = await context.Customers.GetById(customerId);
                    string content = $"تهانينا ، لقد استلم '{buyer.FirstName} {buyer.LastName}' الخدمة '{contract.Service.Title}'." +
                        $"وتم تحويل سعر الخدمة الى حسابك ";
                    jobs.AddNotification(contract.SellerId, content);
                    //buyer can rate the service now
                }
                else
                {
                    contract.SellerIsDone = true;
                    jobs.AddNotification(contract.BuyerId, "لقد أتم البائع الخدمة !اذا تم تسليم الخدمة لك اذهب للعقود الجارية ثم اضغط تم الاستلام.");
                }
                
                //context.Contracts.Update(contract);
                await context.complete();
                return RedirectToAction(nameof(CustomerContracts));
            }
            else
            {
                return NotFound();
            }
        }
        public async Task<IActionResult> ContractBuyerSellerCancel(int id)
        {
            string customerId = userManager.GetUserId(User);
            var contract = await context.Contracts.GetEagerLodingAsync(c => c.ContractId == id, new string[] { "Service" });
            if (contract != null)
            {
                if (contract.SellerId == customerId)
                {
                    contract.IsCanceled = true;
                    contract.EndDate = DateTime.Now;
                    BackgroundJob.Delete(contract.JobId);
                    // contract price goes to buyer now
                    await jobs.AddBalance(contract.BuyerId, contract.ContractPrice);
                    var seller = await context.Customers.GetById(customerId);
                    string content = $"تم الغاء العقد من قبل '{seller.FirstName} {seller.LastName}' لخدمة '{contract.Service.Title}'." +
                        $"وتم ارجاع سعر الخدمة الى حسابك ";
                    jobs.AddNotification(contract.BuyerId, content);
                }
                else
                {
                    contract.BuyerCanceled = true;
                    jobs.AddNotification(contract.SellerId, $"لقد طلب المشتري إلغاء الخدمة '{contract.Service.Title}' قم بإلغاء العقد اذا لم يتم التوصل لاتفاق.");
                    //by the end date a function runs to determine who will take the price
                }

                //context.Contracts.Update(contract);
                await context.complete();
                return RedirectToAction(nameof(CustomerContracts));
            }
            else
            {
                return NotFound();
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> RateContract(int Id,int ContractRateStars,string ContractRateComment)
        {
            if (ModelState.IsValid)
            {
                var contract = await context.Contracts.GetEagerLodingAsync(c => c.ContractId == Id, new string[] { "Service" });
                contract.ContractRateStars = ContractRateStars;
                contract.ContractRateComment = ContractRateComment;
                contract.ContractRateDone = true;
                //context.Contracts.Update(contract);
                jobs.AddNotification(contract.BuyerId, $"تم إضافة تقييمك لخدمة '{contract.Service.Title}' بنجاح");
                await jobs.CalculateOverallRate(contract.ServiceId, contract.ContractRateStars);
                await context.complete();
            }
            return RedirectToAction(nameof(CustomerContracts));
            
        }
        [HttpPost]
        public async Task<IActionResult> AddComplain(int Id,string ComplainContent)
        {
            if (ModelState.IsValid)
            {
                var contract = await context.Contracts.GetEagerLodingAsync(c => c.ContractId == Id, new string[] { "Service" });
                contract.Complain = context.Complains.Add(new Complain() 
                { 
                    ContractId = Id,
                    ComplainContent = ComplainContent 
                });
                jobs.AddNotification(contract.BuyerId, $"تم إضافة الشكوى الخاصة بخدمة {contract.Service.Title}" +
                    $" بنجاح وجاري العمل عليها وسيتم الرد خلال ثلاثة ايام عمل");
                //context.Contracts.Update(contract);
                await context.complete();
            }
            return RedirectToAction(nameof(CustomerContracts));
        }
        
        [HttpPost]
        public async Task<IActionResult> EditContractPrice(int Id,float Price,int DeliveryTime)
        {
            if (ModelState.IsValid)
            {
                var contract = await context.Contracts.GetById(Id);
                contract.ContractPrice = Price;
                contract.DeliveryTime = DeliveryTime;
                //context.Contracts.Update(contract);
                await context.complete();
                return RedirectToAction(nameof(AcceptContract),new {Id });
            }
            else
            {
                return RedirectToAction(nameof(CustomerContracts));
            }
        }
        public async Task<IActionResult> DeleteContract(int id)
        {
            string customerId = userManager.GetUserId(User);
            var contract = await context.Contracts.GetEagerLodingAsync(c => c.ContractId == id, new string[] { "Service" });
            if (contract != null)
            {
                if (contract.CustomerId == customerId || contract.Service.CustomerId == customerId)
                {
                    if(contract.IsDone == false && contract.StartDate == default)
                    {
                        await context.Contracts.Delete(id);
                        string content = $"لقد تم فسخ العقد لخدمة {contract.Service.Title}";
                        AddNotification(contract.CustomerId, content);
                        AddNotification(contract.Service.CustomerId, content);
                        await context.complete();
                        return RedirectToAction(nameof(CustomerContracts));
                    }
                    else
                    {
                        return BadRequest();
                    }
                    
                }
                else
                {
                    return BadRequest();
                }

            }
            else
            {
                return BadRequest();
            }

        }

        public async Task<IActionResult> RequestService(int id)
        {
            string customerId = userManager.GetUserId(User);
            var customer = await context.Customers.GetEagerLodingAsync(c => c.Id == customerId,new string[] { "CustomerRequests" });
            if (customer != null)
            {
                if (customer.CustomerRequests.FirstOrDefault(r => r.ServiceId == id) != null)
                {
                    return RedirectToAction(nameof(CustomerRequests));
                }

                var RequestedService = await context.Services.GetBy(s => s.ServiceId == id);
                if(RequestedService != null)
                {
                    if (RequestedService.CustomerId == customerId)
                    {
                        return BadRequest();
                    }

                    if (!RequestedService.IsCash)
                    {
                        CustomerRequest customerRequest = new CustomerRequest()
                        {
                            CustomerId = customerId,
                            ServiceId = id
                        };
                        context.CustomerRequests.Add(customerRequest);

                        //sending a notification to the Service owner
                        //RequestedService.Customer.Notifications.Add(new Notification()
                        //{
                        //    Content = $"You have a new request from" +
                        //    $" {customer.FirstName} {customer.LastName} about your service : {RequestedService.Title} "
                        //});
                        var content = $"لديك طلب جديد من" +
                            $" {customer.FirstName} {customer.LastName} عن خدمة : {RequestedService.Title} ";
                        AddNotification(RequestedService.CustomerId, content);

                        await context.complete();
                        //if (connection.State == HubConnectionState.Disconnected)
                        //{
                        //    await connection.StartAsync();
                        //}
                        //await connection.InvokeAsync("SendNotification", RequestedService.CustomerId, content);
                        return RedirectToAction(nameof(CustomerRequests));
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return NotFound();
                }
                    
            }
            else
            {
                return NotFound();
            }
        }

        private void AddNotification(string customerId,string content)
        {
            Notification notification = new Notification()
            {
                CustomerId = customerId,
                Content = content
            };
            context.Notifications.Add(notification);
        }
        private Contract AddContract(CustomerRequest customerRequest)
        {
            //converting request to a pending contract
            Contract contract = new Contract()
            {
                CustomerId = customerRequest.CustomerId,
                ServiceId = customerRequest.ServiceId,
                SericeOwnerId = customerRequest.Service.CustomerId
            };
            if (customerRequest.Service.IsFreelancer)
            {
                contract.BuyerId = contract.CustomerId;
                contract.SellerId = contract.SericeOwnerId;
            }
            else
            {
                contract.BuyerId = contract.SericeOwnerId;
                contract.SellerId = contract.CustomerId;
            }
            //add contract to database
            context.Contracts.Add(contract);
            return contract;
        }
        public async Task<IActionResult> CustomerNotifcations()
        {
            var customerId = userManager.GetUserId(User);
            var customer=await context.Customers.GetBy(s=>s.Id==customerId);
            var notification = (await context.Notifications.GetAllBy(c => c.CustomerId == customerId)).OrderByDescending(s=>s.Date);
            var model = new UserNotifcationViewModel()
            {
                customerImage = customer.ProfilePicture,
                Notifications = notification
            };
            if (customerId == null)
            {
                return NotFound();
            };
            return View(model);
        }
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> CustomerPay(string buyerId, int ContractId)
        {
            var customerId = userManager.GetUserId(User);
            var customer = await context.Customers.GetBy(s => s.Id == customerId);
            var contract = await context.Contracts.GetBy(s => s.ContractId == ContractId);
            if (contract.BuyerId != customerId)
            {
                return LocalRedirect("~/Identity/Account/AccessDenied");
            }
            if (contract.StartDate != default)
            {
                return BadRequest();
            }
            if (customer.Balance - contract.ContractPrice < 0)
            {
                return BadRequest();
            }
            Payments payment = new Payments()
            {
                PaymentId = Guid.NewGuid().ToString(),
                CustomerId = customerId,
                ContractId = ContractId,
                StripCustId = null,
                CreatedDate = DateTime.Now,
                PaymentType = "Balance",
                TotalAmount = (int)contract.ContractPrice * 100,
            };
            context.Payments.Add(payment);
            contract.StartDate = DateTime.Now;
            contract.EndDate = contract.StartDate.AddDays(contract.DeliveryTime);
            //hangfire job will run at the end of the contract
            var jobId = BackgroundJob.Schedule(() => jobs.EndContract(ContractId), TimeSpan.FromDays(contract.DeliveryTime));
            contract.JobId = jobId;
            customer.Balance -= contract.ContractPrice;
            await context.complete();
            return RedirectToAction("CustomerContracts");
        }
        public async Task<IActionResult> BalanceDetails()
        {
            var customerId = userManager.GetUserId(User);
            var customer = await context.Customers.GetBy(s => s.Id == customerId);
            if (customer == null)
            {
                return LocalRedirect("~/Identity/Account/AccessDenied");
            }
            var payments=await context.Payments.GetAllEagerLodingAsync(s => s.CustomerId == customerId||s.Contract.CustomerId==customerId, new string[] { "Contract", "Contract.Service", "Customer" });
            var boughtservices = payments.Where(s => s.Contract.BuyerId == customerId && s.Contract.SericeOwnerId != customerId).OrderByDescending(s => s.CreatedDate);
            var soldservices = payments.Where(s => s.Contract.SellerId == customerId && s.Contract.SericeOwnerId == customerId && s.Contract.IsDone == true).OrderByDescending(s => s.CreatedDate);
            var orders = payments.Where(s => s.CustomerId == customerId && s.Contract.SericeOwnerId == customerId).OrderByDescending(s => s.CreatedDate);
            var charges = (await context.BalanceCharges.GetAllEagerLodingAsync(s => s.CustomerId == customerId, new string[] { "Customer" })).OrderByDescending(s => s.CreatedDate);
            var model = new CustomerBalanceDetailsViewModel()
            {
                balanceCharges = charges,
                boughtServices = boughtservices,
                orders = orders,
                payedServices = soldservices
            };
            return View(model);
        }


    }
}
