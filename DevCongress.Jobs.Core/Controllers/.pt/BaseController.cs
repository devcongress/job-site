using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace DevCongress.Jobs.Core.Controllers
{
  public abstract partial class BaseController : Controller
  {
    protected readonly List<string> Successes = new List<string>();
    protected readonly List<string> Information = new List<string>();
    protected readonly List<string> Warnings = new List<string>();
    protected readonly List<string> Errors = new List<string>();

    public void AddSuccess(params string[] msgs)
    {
      Successes.AddRange(msgs);
    }

    public void AddSuccess(List<Success> reasons)
    {
      Successes.AddRange(reasons.Select(m => m.Message));
    }

    public void AddInformation(params string[] msgs)
    {
      Information.AddRange(msgs);
    }

    public void AddWarning(params string[] msgs)
    {
      Warnings.AddRange(msgs);
    }

    public void AddError(params string[] msgs)
    {
      Errors.AddRange(msgs);
    }

    public void AddError(List<Error> reasons)
    {
      Errors.AddRange(reasons.Select(m => m.Message));
    }

    public void AddError(ValidationException validationException, int limit = 10)
    {
      Errors.AddRange(validationException.Errors.Take(limit).Select(e => e.ErrorMessage).ToArray());
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
      if (TempData.ContainsKey("Successes"))
        Successes.InsertRange(0, JsonConvert.DeserializeObject<string[]>(TempData["Successes"].ToString()));
      TempData["Successes"] = JsonConvert.SerializeObject(Successes.ToArray());

      if (TempData.ContainsKey("Information"))
        Information.InsertRange(0, JsonConvert.DeserializeObject<string[]>(TempData["Information"].ToString()));
      TempData["Information"] = JsonConvert.SerializeObject(Information.ToArray());

      if (TempData.ContainsKey("Warnings"))
        Warnings.InsertRange(0, JsonConvert.DeserializeObject<string[]>(TempData["Warnings"].ToString()));
      TempData["Warnings"] = JsonConvert.SerializeObject(Warnings.ToArray());

      if (TempData.ContainsKey("Errors"))
        Errors.InsertRange(0, JsonConvert.DeserializeObject<string[]>(TempData["Errors"].ToString()));
      TempData["Errors"] = JsonConvert.SerializeObject(Errors.ToArray());
    }
  }
}
