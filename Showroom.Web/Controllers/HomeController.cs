using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Showroom.Web.Models;
using Showroom.Web.Services;

namespace Showroom.Web.Controllers;

public class HomeController : Controller
{
    private readonly IShowroomAssistantService _showroomAssistantService;

    public HomeController(IShowroomAssistantService showroomAssistantService)
    {
        _showroomAssistantService = showroomAssistantService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AskAssistant([FromBody] string? message, CancellationToken cancellationToken)
    {
        var reply = await _showroomAssistantService.GetReplyAsync(message, cancellationToken);
        return Json(reply);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
