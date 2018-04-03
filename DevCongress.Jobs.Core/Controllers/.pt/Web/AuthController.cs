using Enexure.MicroBus;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DevCongress.Jobs.Core.Domain.DTO;
using DevCongress.Jobs.Core.Features.Auth.Token.InitLogin;
using DevCongress.Jobs.Core.Features.Auth.Password.Login;
using DevCongress.Jobs.Core.Features.Auth.Token.InitRegistration;
using DevCongress.Jobs.Core.Features.Auth.Token.Login;
using DevCongress.Jobs.Core.ViewModels.Auth;
using System;
using System.Linq;
using System.Threading.Tasks;
using DevCongress.Jobs.Core.Features.Auth.Token.Register;
using DevCongress.Jobs.Core.Features.User.Fetch;
using DevCongress.Jobs.Core.Features.User.UpdateProfile;




namespace DevCongress.Jobs.Core.Controllers.Web
{
    public partial class AuthController
    {
    }

    #region Double-Derived

    /// <summary>
    /// base auth class supporting password auth flows
    /// useful for internal facing apps where the users are managed by an admin
    /// currently supports only login
    /// </summary>
    public abstract class BasePasswordAuthController : BaseAuthController
    {
        protected BasePasswordAuthController(
            ILogger<AuthController> logger,
            IMicroBus microBus,
            IUrlHelper urlHelper) : base(logger, microBus, urlHelper)
        {
        }

        #region Login

        [AllowAnonymous]
        [HttpGet("Login", Name = "login")]
        public virtual IActionResult PasswordLogin()
        {
            if (CurrentUser.IsAuthenticated)
            {
                return RedirectToRoute("dashboard", null);
            }

            return View(new PasswordLoginViewModel() { Remember = true });
        }

        [AllowAnonymous]
        [HttpPost("Login", Name = "login")]
        public virtual async Task<IActionResult> PasswordLogin(
            [FromForm] string Username,
            [FromForm] string Password,
            [FromForm] bool Remember
        )
        {
            if (CurrentUser.IsAuthenticated)
            {
                AddInformation("You are already logged in");
                return RedirectToRoute("dashboard", null);
            }

            var vm = new PasswordLoginViewModel()
            {
                Username = Username,
                Remember = Remember,
            };

            try
            {
                var query = new PasswordLoginQuery(
                            Username: Username,
                            Password: Password);
                var res = await _microBus.QueryAsync(query);

                if (res.IsSuccess)
                {
                    AddSuccess(res.Successes);

                    PersistBearerToken(res.BearerToken, Remember);

                    return RedirectToRoute("dashboard");
                }
                else
                {
                    AddError(res.Errors);
                }
            }
            catch (ValidationException ve)
            {
                AddError(ve);
            }

            return View(vm);
        }

        #endregion Login
    }

    /// <summary>
    /// base auth class supporting token auth flows
    /// uses the magic link pattern to avoid a host of flows such as password reset etc.
    /// supports both login and registration
    /// </summary>
    public abstract class BaseTokenAuthController : BaseAuthController
    {
        protected BaseTokenAuthController(
            ILogger<AuthController> logger,
            IMicroBus microBus,
            IUrlHelper urlHelper) : base(logger, microBus, urlHelper)
        {
        }

        #region Login

        [AllowAnonymous]
        [HttpGet("Login", Name = "login")]
        public virtual IActionResult TokenLogin()
        {
            if (CurrentUser.IsAuthenticated)
            {
                return RedirectToRoute("dashboard", null);
            }

            return View(new TokenLoginViewModel() { Remember = true });
        }

        [AllowAnonymous]
        [HttpPost("InitLogin", Name = "init_token_login")]
        public virtual async Task<IActionResult> InitLogin(
            [FromForm] string EmailAddress,
            [FromForm] bool Remember
        )
        {
            if (CurrentUser.IsAuthenticated)
            {
                AddInformation("You are already logged in");
                return RedirectToRoute("dashboard", null);
            }

            var vm = new TokenLoginViewModel()
            {
                Email = EmailAddress,
                Remember = Remember,
            };

            try
            {
                var src = new TaskCompletionSource<Result>();
                var command = new InitTokenLoginCommand(
                            emailAddress: EmailAddress,
                            confirmUrl: _urlHelper.RouteUrl("token_login", null, Request.Scheme),
                            remember: Remember,
                            result: src);
                await _microBus.SendAsync(command);
                var res = await src.Task;

                if (res.IsSuccess)
                {
                    AddSuccess(res.Successes);

                    return RedirectToRoute("complete_login");
                }
                else
                {
                    AddError(res.Errors);
                }
            }
            catch (ValidationException ve)
            {
                AddError(ve);
            }

            return View("TokenLogin", vm);
        }

        [AllowAnonymous]
        [HttpGet("CompleteLogin", Name = "complete_login")]
        public virtual IActionResult CompleteTokenLogin()
        {
            if (CurrentUser.IsAuthenticated)
            {
                AddInformation("You are already logged in");
                return RedirectToRoute("dashboard", null);
            }

            return View();
        }

        [AllowAnonymous]
        [HttpGet("TokenLogin", Name = "token_login")]
        public virtual async Task<IActionResult> TokenLogin(
            [FromQuery] Guid Token
        )
        {
            if (CurrentUser.IsAuthenticated)
            {
                AddInformation("You are already logged in");
                return RedirectToRoute("dashboard", null);
            }

            try
            {
                var query = new TokenLoginQuery(
                            Token: Token);
                var res = await _microBus.QueryAsync(query);

                if (res.IsSuccess)
                {
                    AddSuccess(res.Successes);

                    PersistBearerToken(res.BearerToken, res.Remember);

                    return RedirectToRoute("dashboard");
                }
                else
                {
                    AddError(res.Errors);
                }
            }
            catch (ValidationException ve)
            {
                AddError(ve);
            }

            return RedirectToRoute("complete_login");
        }

        #endregion Login

        #region Register

        [AllowAnonymous]
        [HttpGet("Register", Name = "register")]
        public virtual IActionResult Register()
        {
            if (CurrentUser.IsAuthenticated)
            {
                AddInformation("You are already logged in");
                return RedirectToRoute("dashboard", null);
            }

            return View("TokenRegistration", new TokenRegistrationViewModel());
        }

        [AllowAnonymous]
        [HttpPost("InitRegistration", Name = "init_token_registration")]
        public virtual async Task<IActionResult> InitTokenRegistration(
              [FromForm] string Name
            , [FromForm] string Email
                , [FromForm] string CompanyName = "N/A"                , [FromForm] string CompanyEmail = null                , [FromForm] string CompanyWebsite = null                , [FromForm] string CompanyDescription = null                )
        {
            if (CurrentUser.IsAuthenticated)
            {
                AddInformation("You are already logged in");
                return RedirectToRoute("dashboard", null);
            }

            var vm = new TokenRegistrationViewModel()
            {
                Email = Email,
                Name = Name,
                                CompanyName = CompanyName,
                                CompanyEmail = CompanyEmail,
                                CompanyWebsite = CompanyWebsite,
                                CompanyDescription = CompanyDescription,
                
            };

            var registrationDetails = new RegistrationDetails
            {
                Name = Name,
                                CompanyName = CompanyName,
                                CompanyEmail = CompanyEmail,
                                CompanyWebsite = CompanyWebsite,
                                CompanyDescription = CompanyDescription,
                
            };

            try
            {
                var src = new TaskCompletionSource<Result>();
                var command = new InitTokenRegistrationCommand(
                            email: Email,
                            confirmUrl: _urlHelper.RouteUrl("token_register", null, Request.Scheme),
                            registrationDetails: registrationDetails,
                            result: src);
                await _microBus.SendAsync(command);
                var res = await src.Task;

                if (res.IsSuccess)
                {
                    AddSuccess(res.Successes);

                    return RedirectToRoute("complete_registration");
                }
                else
                {
                    AddError(res.Errors);
                }
            }
            catch (ValidationException ve)
            {
                AddError(ve);
            }

            return View("TokenRegistration", vm);
        }

        [AllowAnonymous]
        [HttpGet("CompleteRegistration", Name = "complete_registration")]
        public virtual IActionResult CompleteTokenRegistration()
        {
            if (CurrentUser.IsAuthenticated)
            {
                AddInformation("You are already logged in");
                return RedirectToRoute("dashboard", null);
            }

            return View();
        }

        [AllowAnonymous]
        [HttpGet("TokenRegister", Name = "token_register")]
        public virtual async Task<IActionResult> TokenRegister(
            [FromQuery] Guid Token
        )
        {
            if (CurrentUser.IsAuthenticated)
            {
                AddInformation("You are already logged in");
                return RedirectToRoute("dashboard", null);
            }

            try
            {
                var query = new TokenRegisterQuery(
                            Token: Token);
                var res = await _microBus.QueryAsync(query);

                if (res.IsSuccess)
                {
                    AddSuccess(res.Successes);

                    PersistBearerToken(res.BearerToken);

                    return RedirectToRoute("dashboard");
                }
                else
                {
                    AddError(res.Errors);
                }
            }
            catch (ValidationException ve)
            {
                AddError(ve);
            }

            return RedirectToRoute("complete_registration");
        }

        #endregion Register
    }

    public abstract class BaseAuthController : BaseWebController
    {
        protected readonly ILogger<AuthController> _logger;
        protected readonly IMicroBus _microBus;
        protected readonly IUrlHelper _urlHelper;

        protected BaseAuthController(
            ILogger<AuthController> logger,
            IMicroBus microBus,
            IUrlHelper urlHelper)
        {
            _logger = logger;
            _microBus = microBus;
            _urlHelper = urlHelper;
        }

        [HttpGet("Profile", Name = "my_profile")]
        public virtual async Task<IActionResult> Profile()
        {
            var vm = new ProfileViewModel();
            try
            {
                var query = new FetchUserQuery(CurrentUser.Id, includeLogs: true, includeDetailedProperties: true);
                var res = await _microBus.QueryAsync(query);

                if (res.IsSuccess)
                {
                    vm.User = res.User;
                    vm.AuditLogs = res.AuditLogs;
                }
                else
                {
                    AddError(res.Errors);
                }
            }
            catch (ValidationException ve)
            {
                AddError(ve);
            }

            return View(vm);
        }

        [HttpPost("Profile", Name = "my_profile")]
        public virtual async Task<IActionResult> Profile(
              [FromForm] string Name
                , [FromForm] string CompanyName = "N/A"                , [FromForm] string CompanyEmail = null                , [FromForm] string CompanyWebsite = null                , [FromForm] string CompanyDescription = null                )
        {
            try
            {
                var result = new TaskCompletionSource<Result>();
                var command = new UpdateUserProfileCommand(
                    UserId: CurrentUser.Id,
                    Name: Name,
                                        CompanyName : CompanyName,
                                        CompanyEmail : CompanyEmail,
                                        CompanyWebsite : CompanyWebsite,
                                        CompanyDescription : CompanyDescription,
                    

                    result: result);

                await _microBus.SendAsync(command);
                var res = await result.Task;

                AddSuccess(res.Successes);
                AddError(res.Errors);

                if (res.IsSuccess)
                {
                    return RedirectToRoute("my_profile");
                }
            }
            catch (ValidationException ve)
            {
                AddError(ve);
            }

            return RedirectToRoute("my_profile");
        }

        [HttpGet("Logout", Name = "logout")]
        public virtual IActionResult Logout()
        {
            foreach (var key in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(key);
            }

            AddSuccess("You've been logged out successfully");

            return RedirectToRoute("login");
        }

        protected virtual void PersistBearerToken(string bearerToken, bool remember = false)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            };

            //if (remember)
            //  options.Expires = DateTimeOffset.Now.AddYears(1);

            Response.Cookies.Append("Authorization", bearerToken, options);
        }
    }

    #endregion Double-Derived
}
