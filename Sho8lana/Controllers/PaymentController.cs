//using Microsoft.AspNetCore.Mvc;
//using Stripe;

//namespace Sho8lana.Controllers
//{

//    public class PaymentController : Controller
//    {
//        public IActionResult Index()
//        {
//            return View("Checkout");
//        }

//        public IActionResult CustomerPayment(string stripeEmail, string stripeToken)
//        {
//            var customers = new CustomerService();
//            var charges = new ChargeService();
//            var customer = customers.Create(new CustomerCreateOptions
//            {
//                Email = stripeEmail,
//                Source = stripeToken
//            });
//            var charge = charges.Create(new ChargeCreateOptions
//            {
//                Amount = 500,
//                Description="Test Payment",
//                Currency="Usd",
//                Customer=customer.Id,
//                ReceiptEmail=stripeEmail,
//                Metadata=new Dictionary<string, string>
//                {
//                    {"orderId","1111" },
//                    {"postCode","1111fffff" }
//                }

//            });
//            if (charge.Status == "Succeeded")
//            {
//                string BalanceTransactionId = charge.BalanceTransactionId;
//                return View("Checkout");
//            }

//            return View("Checkout");
//        }
//    }
//}
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace server.Controllers
{
    public class PaymentController : Controller
    {
        public PaymentController()
        {
            StripeConfiguration.ApiKey = "sk_test_51KtYXkEZF7e3vou0wl1X7ldWcbh4MUH7TdxiJfz3Ce4b4KYuqC2S7rSPHeZ8z8YMjdWjIARMCV2K2XvrrQn2GDzW00nB9Lf7gf";
        }
        public IActionResult index()
        {
            return View();
        }

        [HttpPost("create-checkout-session")]
        public ActionResult CreateCheckoutSession()
        {
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
        {
          new SessionLineItemOptions
          {
            PriceData = new SessionLineItemPriceDataOptions
            {
              UnitAmount = 2000,
              Currency = "usd",
              ProductData = new SessionLineItemPriceDataProductDataOptions
              {
                Name = "T-shirt",
              },

            },
            Quantity = 1,
          },
        },
                Mode = "payment",
                SuccessUrl = "https://example.com/success",
                CancelUrl = "https://example.com/cancel",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
    }
}
