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
    public partial class AuthController : BaseTokenAuthController
    {
        public AuthController(ILogger<AuthController> logger, IMicroBus microBus, IUrlHelper urlHelper) : base(logger, microBus, urlHelper)
        {
        }
    }
}