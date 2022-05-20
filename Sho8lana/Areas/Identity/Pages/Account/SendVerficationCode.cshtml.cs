using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Sho8lana.Models;
using Sho8lana.Services;
using Twilio.Rest.Verify.V2.Service;

namespace Sho8lana.Areas.Identity.Pages.Account
{
        [Authorize]
        public class SendVerficationCodeModel : PageModel
        {
            private readonly TwilioVerifySettings _settings;
            private readonly UserManager<Customer> _userManager;

            public SendVerficationCodeModel(IOptions<TwilioVerifySettings> settings, UserManager<Customer> userManager)
            {
                _settings = settings.Value;
                _userManager = userManager;
            }

            public string PhoneNumber { get; set; }

            public async Task<IActionResult> OnGetAsync()
            {
                await LoadPhoneNumber();
                return Page();
            }

            public async Task<IActionResult> OnPostAsync()
            {
                await LoadPhoneNumber();

                try
                {
                    var verification = await VerificationResource.CreateAsync(
                        to: PhoneNumber,
                        channel: "sms",
                        pathServiceSid: _settings.VerificationServiceSID
                    );

                    if (verification.Status == "pending")
                    {
                        return RedirectToPage("PhoneNumberConfirmation");
                    }

                    ModelState.AddModelError("", $"There was an error sending the verification code: {verification.Status}");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("",
                        "There was an error sending the verification code, please check the phone number is correct and try again");
                }

                return Page();
            }

            private async Task LoadPhoneNumber()
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    throw new Exception($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }
                PhoneNumber = user.PhoneNumber;
            }
        }
    }
