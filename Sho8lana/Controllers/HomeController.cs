using Microsoft.AspNetCore.Mvc;
using Sho8lana.Models;
using Sho8lana.Unit_Of_Work;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;

namespace Sho8lana.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IUnitOfWork _context;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.GetAll();
            return View(categories);

        }


        public async Task<IActionResult> Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //about
        public IActionResult About()
        {
            return View();
        }

        //contact_us
        [HttpGet]
        public IActionResult Contact_Us(contactClass _objModelMail)
        {
            if (ModelState.IsValid)
            {
                MailMessage mail = new MailMessage();
                mail.To.Add("sho8lanaservices@gmail.com");
                mail.From = new MailAddress(_objModelMail.Email);
                mail.Subject = _objModelMail.Subject;

                StringBuilder Body = new StringBuilder();
                Body.Append("Name :   " + _objModelMail.Name);
                Body.Append("Email :   " + _objModelMail.Email);


                mail.Body = Body.ToString();
                mail.IsBodyHtml = true;
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential
                ("username", "password");// Enter seders User name and password
                smtp.EnableSsl = true;
                smtp.Send(mail);
                return View("Index", _objModelMail);
            }

            else
            {
                return View();
            }
        }
    }
}