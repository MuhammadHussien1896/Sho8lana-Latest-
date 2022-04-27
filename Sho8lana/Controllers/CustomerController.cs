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
        public async Task<IActionResult> account()
        {
            string customerId = userManager.GetUserId(User);
            var customer = await context.Customers.GetBy(c => c.Id == customerId);

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
            //converting request to a pending contract
            Contract contract = new Contract()
            {
                Customer = customerRequest.Customer,
                Service = customerRequest.Service
            };
            //add contract to database
            context.Contracts.Add(contract);
            //contract.Customer.Notifications.Add(new Notification() 
            //{
            //    Content = $"{customer.FirstName} {customer.LastName} has accepted your request for the service {contract.Service.Title} !" +
            //    $"Please check your pending contracts to begin the contract"
            //});

            //add notification to the customer who sent the request to the service
            AddNotification(contract.Customer,
                $"{customer.FirstName} {customer.LastName} has accepted your request for the service {contract.Service.Title} !" +
                $"Please check your pending contracts to begin the contract");
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
                    return context.complete().Result > 0 ? View(CustomerRequests()) : BadRequest();
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
            var customer = await context.Customers.GetBy(c => c.Id == customerId);
            if (customer != null)
            {

                //return the active contracts
                var contracts = await context.Contracts
                                                .GetAllBy(c => c.CustomerId == customerId || c.Service.CustomerId == customerId);
                ContractViewModel model = new ContractViewModel()
                {
                    PendingContracts    = contracts.Where(c => c.IsDone == false && c.StartDate == default).ToList(),
                    ActiveContracts     = contracts.Where(c => c.IsDone == false && c.StartDate != default).ToList(),
                    DoneContracts       = contracts.Where(c => c.IsDone == true).ToList()
                };
                return View(model);
            }
            else
            {
                return NotFound();
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
                    return Content("You cant request the service more than one time!");
                }

                var RequestedService = await context.Services.GetBy(s => s.ServiceId == id);
                if(RequestedService != null)
                {
                    if (RequestedService.CustomerId == customerId)
                    {
                        return Content("you cant request your own service !");
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
                        AddNotification(RequestedService.Customer,
                            $"You have a new request from" +
                            $" {customer.FirstName} {customer.LastName} about your service : {RequestedService.Title} ");

                        await context.complete();
                        return Content("Request added!");
                    }
                    else
                    {
                        return Content("You cant request a cash service!");
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

        private void AddNotification(Customer customer,string content)
        {
            Notification notification = new Notification()
            {
                Customer = customer,
                Content = content
            };
            context.Notifications.Add(notification);
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
