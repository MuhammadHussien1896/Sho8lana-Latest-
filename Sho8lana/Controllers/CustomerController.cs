using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sho8lana.Models;
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
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult IncomingRequests()
        {
            var customerId = userManager.GetUserId(User);
            var task = context.Customers.GetBy(c => c.Id == customerId);
            if (task.IsCompletedSuccessfully)
            {
                var customer = task.Result;
                //if this customer has a request that contain a service that he owned it
                //then this is an incoming request to him
                var IncomingRequests = customer.CustomerRequests
                                        .Where(r => r.Service.CustomerId == customerId);
                return View(IncomingRequests);
            }
            else
            {
                return NotFound();
            }
            
        }
        public IActionResult OutgoingRequests()
        {
            var customerId = userManager.GetUserId(User);
            var task = context.Customers.GetBy(c => c.Id == customerId);
            if (task.IsCompletedSuccessfully)
            {
                var customer = task.Result;
                //return the requested requests
                var OutgoingRequests = customer.CustomerRequests
                                        .Where(r => r.CustomerId == customerId);
                return View(OutgoingRequests);
            }
            else
            {
                return NotFound();
            }
        }

        public IActionResult DeleteRequest(int requestId)
        {
            context.CustomerRequests.Delete(requestId);
            var task = context.complete();
            if (task.IsCompletedSuccessfully)
            {
                return View();

            }
            else
            {
                return NotFound();
            }
            
        }

        public IActionResult ActiveContracts()
        {
            var customerId = userManager.GetUserId(User);
            var task = context.Customers.GetBy(c => c.Id == customerId);
            if (task.IsCompletedSuccessfully)
            {
                var customer = task.Result;
                //return the active contracts
                var contracts = customer.Contracts.Where(c => c.IsDone == false);
                return View(contracts);
            }
            else
            {
                return NotFound();
            }
        }

        public IActionResult DoneContracts()
        {
            var customerId = userManager.GetUserId(User);
            var task = context.Customers.GetBy(c => c.Id == customerId);
            if (task.IsCompletedSuccessfully)
            {
                var customer = task.Result;
                //return contracts from history
                var contracts = customer.Contracts.Where(c => c.IsDone == true);
                return View(contracts);
            }
            else
            {
                return NotFound();
            }
        }







        //dump actions
        public async Task<IActionResult> AddService()
        {
            //var customerId = userManager.GetUserId(User);
            var customer = await context.Customers.GetBy(c => c.Email == User.Identity.Name);
            if (customer != null)
            {
                
                
                Category category = new Category() { Description = "freelance jobs", Name = "freelance" };
                context.Categories.Add(category);
                Service service = new Service() {
                    CustomerId = customer.Id
                    ,Description="bla bla",
                    Title="blaaa",
                    Category=category
                };
                context.Services.Add(service);
                await context.complete();
                return Content("service added!");
            }
            else
            {
                return NotFound();
            }
            
        }

        public async Task<IActionResult> RequestService()
        {
            //var customerId = userManager.GetUserId(User);
            var customer = await context.Customers.GetBy(c => c.Email == User.Identity.Name);
            if (customer != null)
            {
                
                
                if(customer.Services.Any(s => s.ServiceId == 1))
                {
                    return Content("you cant request your own service !");
                }
                CustomerRequest customerRequest = new CustomerRequest() {Customer = customer,ServiceId=1 };
                context.CustomerRequests.Add(customerRequest);
                await context.complete();
                return Content("Request added!");
            }
            else
            {
                return NotFound();
            }
        }

    }
}
