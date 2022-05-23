// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Sho8lana.Models;
using Sho8lana.Unit_Of_Work;

namespace Sho8lana.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Customer> _signInManager;
        private readonly UserManager<Customer> _userManager;
        private readonly IUserStore<Customer> _userStore;
        private readonly IUserEmailStore<Customer> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;

        //private ICategoryService categoryService;
        //public CascadingDropdownsModel(IUnitOfWork categoryService) => this.categoryService = categoryService;

        public RegisterModel(
            UserManager<Customer> userManager,
            IUserStore<Customer> userStore,
            SignInManager<Customer> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IUnitOfWork unitOfWork
            
            )
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        /// 
        public List<Governorate> govern { get; set; }
        public List<City> cities { get; set; }

        [BindProperty(SupportsGet = true)]
        public int GovernorateId { get; set; }
        public int SelArea { get; set; }
        public class InputModel
        {

            [RegularExpression(@"[a-zA-z|\u0621-\u064A]+", ErrorMessage = "من فضلك ادلخل الإسم بطريقة صحيحة ")]
            [Required(ErrorMessage = "من فضلك ادلخل الإسم الأول للمستخدم "), StringLength(15, MinimumLength = 3, ErrorMessage = "يجب أن يكون الإسم الأول لا يقل عن 3 حروف أو يذيد عن 15 حرف ")]
            public string FirstName { get; set; }
            [Required(ErrorMessage = "من فضلك ادلخل الإسم الثانى للمستخدم "), StringLength(15, MinimumLength = 3, ErrorMessage = "يجب أن يكون الإسم الأول لا يقل عن 3 حروف أو يذيد عن 15 حرف ")]
            [RegularExpression(@"[a-zA-z|\u0621-\u064A]+", ErrorMessage = "من فضلك ادلخل الإسم بطريقة صحيحة ")]

            public string LastName { get; set; }

           
            [Required(ErrorMessage = "من فضلك ادخل الرقم السرى")]
            [StringLength(100, ErrorMessage = "يجب أن يحتوى الرقم السري على حروف وارقام ولا يقل عن 6 حروف ولا يزيد عن 15 حرف", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "الرقم السرى وتأكيد الرقم السرى لا يتطابقان ")]
            public string ConfirmPassword { get; set; }

            [Remote(action: "ConfirmUniqueEmail", controller: "JsonApi",ErrorMessage = "هذا البريد الإلكترونى موجود مسبقاً ")]
            [Required(ErrorMessage = "من فضلك أدخل البريد الإلكترونى")]
            [EmailAddress(ErrorMessage = "من فضلك أدخل بريد إلكترونى صحيح ")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Remote(action: "ConfirmUniquePhoneNumber", controller: "JsonApi",ErrorMessage = "هذا الرقم موجود بالفعل ")]
            [RegularExpression("^01[0-2,5]{1}[0-9]{8}$")]
            [Display(Name = "PhoneNumber")]
            public string PhoneNumber { get; set; }
            public string? AboutMe { get; set; }
            [Required(ErrorMessage = "من فضلك أدخل ذكر أم أنثى "), MaxLength(6)]

            public string Gender { get; set; }
            // Address
            [Required(ErrorMessage = "من فضلك أدخل الدولة ")]
            public string Country { get; set; }
            [Required(ErrorMessage = "من فضلك أدخل المحافظة ")]
            public string City { get; set; }
            [Required(ErrorMessage = "من فضلك أدخل إسم المدينة ")]
            public string Area { get; set; }

            public string ProfileImage { get; set; }
            //image
            public string NationalIdImage { get; set; }



        }


        public async Task OnGetAsync(string returnUrl = null)
        
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            cities=_unitOfWork.Cities.GetAll().Result;
            govern = _unitOfWork.Governorates.GetAll().Result;

            ViewData["CityId"]=_unitOfWork.Governorates.GetAll().Result.Select(c=>new SelectListItem { Value= c.GovernorateId.ToString(),Text=c.Governorate_name_ar}).ToList();

            
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {

                var user = new Customer()
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    PasswordHash=Input.Password,
                    Gender=Input.Gender,
                    Email=Input.Email,
                    PhoneNumber=Input.PhoneNumber,
                    AboutMe=Input.AboutMe,
                    Country=Input.Country,
                    City=Input.City,
                    Area=Input.Area

                };
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    await _userManager.AddToRoleAsync(user, "User");
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            cities = _unitOfWork.Cities.GetAll().Result;
            govern = _unitOfWork.Governorates.GetAll().Result;
            // If we got this far, something failed, redisplay form
            return Page();
        }

        private Customer CreateUser()
        {
            try
            {
                return Activator.CreateInstance<Customer>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Customer)}'. " +
                    $"Ensure that '{nameof(Customer)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<Customer> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<Customer>)_userStore;
        }
    }
}
