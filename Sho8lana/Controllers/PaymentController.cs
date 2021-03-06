using System.Collections.Generic;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Sho8lana.Hangfire;
using Sho8lana.Hubs;
using Sho8lana.Models;
using Sho8lana.Unit_Of_Work;
using Stripe;
using Stripe.Checkout;

namespace server.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IUnitOfWork _context;
        private readonly UserManager<Sho8lana.Models.Customer> userManager;
        private readonly ContractJobs jobs;
        string _sessionId;
        public PaymentController(IUnitOfWork context, UserManager<Sho8lana.Models.Customer> userManager, IHubContext<ChatHub> hubContext)
        {
            StripeConfiguration.ApiKey = "sk_test_51KtYXkEZF7e3vou0wl1X7ldWcbh4MUH7TdxiJfz3Ce4b4KYuqC2S7rSPHeZ8z8YMjdWjIARMCV2K2XvrrQn2GDzW00nB9Lf7gf";

            _context = context;
            this.userManager = userManager;
            this.jobs = new ContractJobs(context, hubContext);
        }
        public IActionResult index()
        {
            return View();
        }

        [HttpPost("create-checkout-session")]
        public ActionResult CreateCheckoutSession(int ContractId, string customerId)
        {
            var Target = _context.Contracts.GetEagerLodingAsync(c => c.ContractId == ContractId, new string[] { "Service" }).Result;
            var TargetCustomer = _context.Customers.GetById(customerId).Result;
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
            {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount =(long)Target.ContractPrice*100,
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = Target.Service.Title,
                                Description=TargetCustomer.FirstName,
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = Url.ActionLink("OrderSuccess", "payment")+"?session_id={CHECKOUT_SESSION_ID}" + $"&&ContractId={ContractId}" + $"&&customerId={customerId}",
                CancelUrl = "https://example.com/cancel",
            };
            var service = new SessionService();
            Session session = service.Create(options);
            _sessionId = session.Id;
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        [HttpGet("/payment/success")]
        public async Task<IActionResult> OrderSuccess([FromQuery] string session_id, int ContractId, string customerId)
        {
            var Target = _context.Contracts.GetById(ContractId).Result;
            var TargetCustomer = _context.Customers.GetById(customerId).Result;

            var options = new SessionGetOptions();
            options.AddExpand("payment_intent");

            var sessionService = new SessionService();
            Session session = sessionService.Get(session_id, options);

            var customerService = new CustomerService();
            var customer = customerService.Get(session.CustomerId);

            PaymentIntent paymentIntent = session.PaymentIntent;

            Payments payment = new Payments()
            {
                PaymentId = paymentIntent.Id,
                CustomerId = customerId,
                ContractId = ContractId,
                StripCustId = customer.Id,
                CreatedDate = DateTime.Now,
                PaymentType = paymentIntent.PaymentMethodTypes.First(),
                TotalAmount = (int)paymentIntent.Amount,
            };
            _context.Payments.Add(payment);
            Target.StartDate = DateTime.Now;
            Target.EndDate = Target.StartDate.AddDays(Target.DeliveryTime);
            //hangfire job will run at the end of the contract

            var jobId = BackgroundJob.Schedule(() => jobs.EndContract(ContractId), TimeSpan.FromDays(Target.DeliveryTime));
            Target.JobId = jobId;//adding job id to be able to delete the job earlier 
            //_context.Contracts.Update(Target);
            await jobs.AddNotification(customerId, "تهانينا تم دفع سعر الخدمة بنجاح !");
            await jobs.AddNotification(Target.SellerId, "لقد دفع المشتري سعر الخدمة ، يمكنك البدأ بالعمل الان");
            await _context.complete();

            return RedirectToAction("customercontracts", "customer");
        }
        [HttpPost("charge-checkout-session")]
        public ActionResult ChargingBalance(string customerId, int amount)
        {
            var TargetCustomer = _context.Customers.GetById(customerId);
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
            {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount =(long)amount*100,
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "شحن رصيد",
                            },
                        },
                        Quantity=1
                    },
                },
                Mode = "payment",
                SuccessUrl = Url.ActionLink("ChargeSuccess","payment") +"?session_id={CHECKOUT_SESSION_ID}" + $"&&customerId={customerId}",
                CancelUrl = "https://example.com/cancel",
            };
            var service = new SessionService();
            Session session = service.Create(options);
            _sessionId = session.Id;
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        [HttpGet("/Charge/success")]
        public async Task<IActionResult> ChargeSuccess([FromQuery] string session_id, string customerId)
        {
            var TargetCustomer = _context.Customers.GetById(customerId).Result;

            var options = new SessionGetOptions();
            options.AddExpand("payment_intent");

            var sessionService = new SessionService();
            Session session = sessionService.Get(session_id, options);

            var customerService = new CustomerService();
            var customer = customerService.Get(session.CustomerId);

            PaymentIntent paymentIntent = session.PaymentIntent;

            BalanceCharge charge = new BalanceCharge()
            {
                PaymentId = paymentIntent.Id,
                CustomerId = customerId,
                StripCustId = customer.Id,
                CreatedDate = DateTime.Now,
                PaymentType = paymentIntent.PaymentMethodTypes.First(),
                TotalAmount = (int)paymentIntent.Amount,
            };
            _context.BalanceCharges.Add(charge);
            TargetCustomer.Balance += (int)charge.TotalAmount / 100;
            await jobs.AddNotification(customerId, $"تم اضافة {charge.TotalAmount / 100}  دولار الي الرصيد بنجاح");
            await _context.complete();
            return RedirectToAction("customercontracts", "customer");
        }
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CustomerCheckout(int contractId)
        {
            var customerId = userManager.GetUserId(User);
            var customer = await _context.Customers.GetBy(s => s.Id == customerId);
            var contract = await _context.Contracts
                                    .GetEagerLodingAsync(s => s.ContractId == contractId
                                    , new string[] { "Customer", "Service.Customer", "Service.Medias" });
            if (contract.BuyerId != customerId)
            {
                return LocalRedirect("~/Identity/Account/AccessDenied");
            }
            else if (customer == null)
            {
                return LocalRedirect("~/Identity/Account/Login");
            }
            else
            {
                return View(contract);
            }

        }
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> CustomerPay(string buyerId, int ContractId)
        {
            var customerId = userManager.GetUserId(User);
            var customer = await _context.Customers.GetBy(s => s.Id == customerId);
            var contract = await _context.Contracts.GetBy(s => s.ContractId == ContractId);
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
                TotalAmount = (int)contract.ContractPrice*100,
            };
            _context.Payments.Add(payment);
            contract.StartDate = DateTime.Now;
            contract.EndDate = contract.StartDate.AddDays(contract.DeliveryTime);
            //hangfire job will run at the end of the contract
            var jobId = BackgroundJob.Schedule(() => jobs.EndContract(ContractId), TimeSpan.FromDays(contract.DeliveryTime));
            contract.JobId = jobId;
            customer.Balance -= contract.ContractPrice;
            await jobs.AddNotification(customerId, $"تم دفع {contract.ContractPrice}$ من رصيدك بنجاح");
            await _context.complete();
            return RedirectToAction("CustomerContracts", "customer");
        }

    }
}
