using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sho8lana.Models;
using Sho8lana.Models.ViewModels;
using Sho8lana.Unit_Of_Work;


namespace Sho8lana.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IUnitOfWork context;
        private readonly UserManager<Customer> userManager;

        public CustomerController(IUnitOfWork context,UserManager<Customer> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }
        public async Task<IActionResult> account(string id)
        {
            string customerId = userManager.GetUserId(User);
            var customer = await context.Customers.GetBy(c => c.Id == id);
            if(customer == null) { return NotFound(); }
            return View(customer);
            

        }
     
        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> IncomingRequests()
        //{
        //    string customerId = userManager.GetUserId(User);
        //    var customer = await context.Customers.GetBy(c => c.Id == customerId);
        //    if (customer != null)
        //    {

        //        //first approach to get customer incoming requests
        //        var incomingRequests = await context.CustomerRequests.GetAllBy(r => r.Service.CustomerId == customer.Id);
        //        return View(incomingRequests);
        //        //secondApproach approach to get customer incoming requests
        //        //var customerServices = customer.Services.ToList();
        //        //if (customerServices.Count == 0)
        //        //{
        //        //    return View(null);
        //        //}
        //        //else
        //        //{
                    
        //        //    List<CustomerRequest> IncomingRequests = new List<CustomerRequest>();
        //        //    foreach (var service in customerServices)
        //        //    {
        //        //        foreach (var request in service.CustomerRequests)
        //        //        {
        //        //            IncomingRequests.Add(request);
        //        //        }
        //        //    }
        //        //    return View(IncomingRequests);
        //        //}
                
        //    }
        //    else
        //    {
        //        return NotFound();
        //    }
            
        //}
        //public async Task<IActionResult> OutgoingRequests()
        //{
        //    string customerId = userManager.GetUserId(User);
        //    var customer = await context.Customers.GetBy(c => c.Id == customerId);
        //    if (customer != null)
        //    {
        //        var OutgoingRequests = customer.CustomerRequests.ToList();                   
        //        return View(OutgoingRequests);
        //    }
        //    else
        //    {
        //        return NotFound();
        //    }
        //}

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
            var customer = await context.Customers.GetBy(c => c.Id == customerId);
            if (customer != null)
            {
                var incomingRequests = await context.CustomerRequests.GetAllBy(r => r.Service.CustomerId == customer.Id);
                var outgoingRequests = customer.CustomerRequests.ToList();
                CustomerRequestViewModel model = new CustomerRequestViewModel()
                {
                    IncomingRequests = incomingRequests.ToList(),
                    OutgoingRequests = outgoingRequests
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
            var customerRequest = await context.CustomerRequests.GetById(id);
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
            AddNotification(contract.CustomerId,
                $"{customer.FirstName} {customer.LastName} قد قبل طلبك للخدمة '{contract.Service.Title}' !" +
                $"رجاءً تفقد العقود المنتظرة لبدء العمل");
            //delete customer request after accepting and become a contract
            await context.CustomerRequests.Delete(id);
            //saving changes in database
            await context.complete();
            
            return RedirectToAction(nameof(CustomerContracts));
        }

        public async Task<IActionResult> DeleteRequest(int id)
        {
            string customerId = userManager.GetUserId(User);
            var customerRequest = await context.CustomerRequests.GetById(id);
            if(customerRequest != null)
            {
                if(customerRequest.CustomerId == customerId || customerRequest.Service.CustomerId == customerId)
                {
                    await context.CustomerRequests.Delete(id);
                    string content = $"لقد تم الغاء الطلب على الخدمة {customerRequest.Service.Title}";
                    AddNotification(customerRequest.Service.CustomerId,content);
                    AddNotification(customerRequest.CustomerId,content);
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
                                                .GetAllBy(c => c.CustomerId == customerId || c.SericeOwnerId == customerId);
                ContractViewModel model = new ContractViewModel()
                {
                    PendingContracts            = contracts.Where(c => c.IsDone == false && c.StartDate == default && !(c.BuyerAccepted && c.SellerAccepted)).ToList(),
                    PendingPaymentContracts     = contracts.Where(c => c.IsDone == false && c.StartDate == default && (c.BuyerAccepted && c.SellerAccepted)).ToList(),
                    ActiveContracts             = contracts.Where(c => c.IsDone == false && c.StartDate != default).ToList(),
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
                context.Contracts.Update(contract);
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
            var contract = await context.Contracts.GetById(id);
            if (contract != null)
            {
                if (contract.BuyerId == customerId)
                {
                    contract.IsDone = true;
                    contract.EndDate = DateTime.Now;
                    // contract price goes to seller now
                    await AddPendingBalance(contract.SellerId, contract.ContractPrice);
                    var buyer = await context.Customers.GetById(customerId);
                    string content = $"تهانينا ، لقد استلم '{buyer.FirstName} {buyer.LastName}' الخدمة '{contract.Service.Title}'." +
                        $"وتم تحويل سعر الخدمة الى حسابك ";
                    AddNotification(contract.SellerId, content);
                    //buyer can rate the service now
                }
                else
                {
                    contract.SellerIsDone = true;
                    AddNotification(contract.BuyerId, "لقد أتم البائع الخدمة !اذا تم تسليم الخدمة لك اذهب للعقود الجارية ثم اضغط تم الاستلام.");
                }
                
                context.Contracts.Update(contract);
                await context.complete();
                return RedirectToAction(nameof(CustomerContracts));
            }
            else
            {
                return NotFound();
            }
        }
        private async Task AddPendingBalance(string sellerId,float balance)
        {
            var seller = await context.Customers.GetById(sellerId);
            seller.PendingBalance += balance;
            context.Customers.Update(seller);
            
        }
        //public async Task<IActionResult> EditContractPrice(int id)
        //{
        //    string customerId = userManager.GetUserId(User);
        //    var contract = await context.Contracts.GetById(id);
        //    if (contract == null || customerId == null)
        //    {
        //        return NotFound();
        //    }
        //    if(contract.CustomerId == customerId)//not the service owner
        //    {
        //        return NotFound();
        //    }
        //    EditContractPriceViewModel model = new EditContractPriceViewModel() { Id = id };
        //    return View(model);
        //}
        [HttpPost]
        public async Task<IActionResult> EditContractPrice(ContractViewModel model)
        {
            if (ModelState.IsValid)
            {
                var contract = await context.Contracts.GetById(model.Id);
                contract.ContractPrice = model.Price;
                contract.DeliveryTime = model.DeliveryTime;
                context.Contracts.Update(contract);
                await context.complete();
                return RedirectToAction(nameof(AcceptContract),new {id = model.Id });
            }
            else
            {
                return RedirectToAction(nameof(CustomerContracts));
            }
        }
            public async Task<IActionResult> DeleteContract(int id)
        {
            string customerId = userManager.GetUserId(User);
            var contract = await context.Contracts.GetById(id);
            if (contract != null)
            {
                if (contract.CustomerId == customerId || contract.Service.CustomerId == customerId)
                {
                    if(contract.IsDone == false && contract.StartDate == default && !(contract.BuyerAccepted && contract.SellerAccepted))
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
            var customer = await context.Customers.GetBy(c => c.Id == customerId);
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
                            Customer = customer,
                            Service = RequestedService
                        };
                        context.CustomerRequests.Add(customerRequest);

                        //sending a notification to the Service owner
                        //RequestedService.Customer.Notifications.Add(new Notification()
                        //{
                        //    Content = $"You have a new request from" +
                        //    $" {customer.FirstName} {customer.LastName} about your service : {RequestedService.Title} "
                        //});
                        AddNotification(RequestedService.CustomerId,
                            $"You have a new request from" +
                            $" {customer.FirstName} {customer.LastName} about your service : {RequestedService.Title} ");

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
            var notification = await context.Notifications.GetAllBy(c => c.CustomerId == customerId);
            if (customerId == null)
            {
                return NotFound();
            };
            return View(notification);
        }

    }
}
