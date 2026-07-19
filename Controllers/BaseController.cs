using Microsoft.AspNetCore.Mvc;
using PharMarket.Models;

namespace PharMarket.Controllers;

public abstract class BaseController : Controller
{
    protected void SetSuccessMessage(string message)
    {
        TempData["SuccessMessage"] = message;
    }

    protected void SetErrorMessage(string message)
    {
        TempData["ErrorMessage"] = message;
    }

    protected void SetWarningMessage(string message)
    {
        TempData["WarningMessage"] = message;
    }

    protected void SetInfoMessage(string message)
    {
        TempData["InfoMessage"] = message;
    }

    protected int GetCurrentPage()
    {
        if (int.TryParse(Request.Query["page"], out var page) && page > 0)
            return page;
        return 1;
    }

    protected string GetReturnUrl(string? returnUrl = null)
    {
        return returnUrl ?? Url.Action("Index") ?? "/";
    }
}
