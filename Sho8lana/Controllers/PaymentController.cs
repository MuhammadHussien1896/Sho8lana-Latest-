using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sho8lana.Models;
using Sho8lana.Unit_Of_Work;
using Stripe;
using Stripe.Checkout;

namespace server.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IUnitOfWork _context;
        string _sessionId;
        public PaymentController(IUnitOfWork context)
        {
            StripeConfiguration.ApiKey = "sk_test_51KtYXkEZF7e3vou0wl1X7ldWcbh4MUH7TdxiJfz3Ce4b4KYuqC2S7rSPHeZ8z8YMjdWjIARMCV2K2XvrrQn2GDzW00nB9Lf7gf";
            
            _context = context;
        }
        public IActionResult index()
        {
            return View();
        }

        [HttpPost("create-checkout-session")]
        public ActionResult CreateCheckoutSession(int ContractId, string customerId)
        {
            var Target=_context.Contracts.GetById(ContractId).Result;
            var TargetCustomer=_context.Customers.GetById(customerId).Result;
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
                SuccessUrl = "https://localhost:7009/payment/success?session_id={CHECKOUT_SESSION_ID}"+ $"&&ContractId={ContractId}"+ $"&&customerId={customerId}",
                CancelUrl = "https://example.com/cancel",
            };
            var service = new SessionService();
            Session session = service.Create(options);
            _sessionId = session.Id;
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        [HttpGet("/payment/success")]
        public  async Task<IActionResult> OrderSuccess([FromQuery] string session_id,int ContractId, string customerId)
        {
            var Target = _context.Contracts.GetById(ContractId).Result;
            var TargetCustomer = _context.Customers.GetById(customerId).Result;

            var options = new SessionGetOptions();
            options.AddExpand("payment_intent");

            var sessionService = new SessionService();
            Session session = sessionService.Get(session_id,options);

            var customerService = new CustomerService();
            var customer = customerService.Get(session.CustomerId);

            PaymentIntent paymentIntent = session.PaymentIntent;

            Payments payment = new Payments()
            {
                PaymentId=paymentIntent.Id,
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
            _context.Contracts.Update(Target);
            _context.Notifications.Add(new Notification
            {
                CustomerId = customerId,
                Content = "تهانينا تم دفع سعر الخدمة بنجاح !"
            });
            await _context.complete();
            
            return RedirectToAction("customercontracts","customer");
        }

    }
}
