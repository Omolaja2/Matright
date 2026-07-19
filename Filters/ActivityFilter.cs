using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace PharMarket.Filters;

public class ActivityFilter : IActionFilter
{
    private readonly ILogger<ActivityFilter> _logger;
    private Stopwatch? _stopwatch;

    public ActivityFilter(ILogger<ActivityFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Executing action {Action} on controller {Controller}",
            context.ActionDescriptor.RouteValues["action"],
            context.ActionDescriptor.RouteValues["controller"]);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _stopwatch?.Stop();
        var elapsed = _stopwatch?.ElapsedMilliseconds ?? 0;

        if (context.Exception != null && !context.ExceptionHandled)
        {
            _logger.LogError(context.Exception,
                "Action {Action} on controller {Controller} failed after {Elapsed}ms",
                context.ActionDescriptor.RouteValues["action"],
                context.ActionDescriptor.RouteValues["controller"],
                elapsed);
        }
        else
        {
            _logger.LogInformation("Action {Action} on controller {Controller} completed in {Elapsed}ms with status {StatusCode}",
                context.ActionDescriptor.RouteValues["action"],
                context.ActionDescriptor.RouteValues["controller"],
                elapsed,
                context.HttpContext.Response.StatusCode);
        }
    }
}
