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
        public async Task<IActionResult> IncomingRequests()
        {
            string customerId = userManager.GetUserId(User);
            var customer = await context.Customers.GetBy(c => c.Id == customerId);
            if (customer != null)
            {

                //first approach to get customer incoming requests
                var incomingRequests = await context.CustomerRequests.GetAllBy(r => r.Service.CustomerId == customer.Id);
                return View(incomingRequests);
                //secondApproach approach to get customer incoming requests
                //var customerServices = customer.Services.ToList();
                //if (customerServices.Count == 0)
                //{
                //    return View(null);
                //}
                //else
                //{
                    
                //    List<CustomerRequest> IncomingRequests = new List<CustomerRequest>();
                //    foreach (var service in customerServices)
                //    {
                //        foreach (var request in service.CustomerRequests)
                //        {
                //            IncomingRequests.Add(request);
                //        }
                //    }
                //    return View(IncomingRequests);
                //}
                
            }
            else
            {
                return NotFound();
            }
            
        }
        public async Task<IActionResult> OutgoingRequests()
        {
            string customerId = userManager.GetUserId(User);
            var customer = await context.Customers.GetBy(c => c.Id == customerId);
            if (customer != null)
            {
                var OutgoingRequests = customer.CustomerRequests.ToList();                   
                return View(OutgoingRequests);
            }
            else
            {
                return NotFound();
            }
        }

        //customer balance
        public  async Task< IActionResult> GetCustomerBalance()
        {
            string customerId = userManager.GetUserId(User);
            var customer = await context.Customers.GetBy(c => c.Id == customerId);
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

        public async Task<IActionResult> DeleteRequest(int requestId)
        {

                context.CustomerRequests.Delete(requestId);
                var deletedRecords = await context.complete();
                if (deletedRecords > 0)
                {
                    return Content($"deleted records : {deletedRecords}");

                }
                else
                {
                    return NotFound();
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
                    ActiveContracts     = contracts.Where(c => c.IsDone == false).ToList(),
                    DoneContracts       = contracts.Where(c => c.IsDone == true).ToList()
                };
                return View(model);
            }
            else
            {
                return NotFound();
            }
        }







        //dump actions
        //public async Task<IActionResult> AddService()
        //{
        //    string customerId = userManager.GetUserId(User);
        //    var customer = await context.Customers.GetBy(c => c.Id == customerId);
        //    if (customer != null)
        //    {
                
                
        //        Category category = new Category() { Description = "freelance jobs", Name = "freelance" };
        //        context.Categories.Add(category);
        //        Service service = new Service() {
        //            CustomerId = customer.Id
        //            ,Description="bla bla",
        //            Title="blaaa",
        //            Category=category
        //        };
        //        context.Services.Add(service);
        //        await context.complete();
        //        return Content("service added!");
        //    }
        //    else
        //    {
        //        return NotFound();
        //    }
            
        //}

        public async Task<IActionResult> RequestService(int id)
        {
            string customerId = userManager.GetUserId(User);
            var customer = await context.Customers.GetBy(c => c.Id == customerId);
            if (customer != null)
            {
                
                
                if(customer.Services.Any(s => s.ServiceId == id))
                {
                    return Content("you cant request your own service !");
                }
                var RequestedService = await context.Services.GetBy(s => s.ServiceId == id);
                if(RequestedService != null)
                {
                    CustomerRequest customerRequest = new CustomerRequest()
                    {
                        Customer = customer,
                        Service = RequestedService
                    };
                    context.CustomerRequests.Add(customerRequest);

                    //sending a notification to the other user
                    RequestedService.Customer.Notifications.Add(new Notification()
                    {
                        Content = $"You have a new request from" +
                        $" {customer.FirstName} {customer.LastName} about your service : {customerRequest.Service.Title} "
                    });
                    context.Customers.Update(customerRequest.Service.Customer);

                    await context.complete();
                    return Content("Request added!");
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

    }
}
