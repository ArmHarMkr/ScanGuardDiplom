using MGOBankApp.BLL.Interfaces;
using MGOBankApp.DAL.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MGOBankApp.Controllers;

/// ASP.NET Core MVC Chat Feature with Database Cleanup

[Authorize]
public class ChatController : Controller
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    public IActionResult Index()
    {
        var messages = _chatService.GetAllMessages();
        return View(messages);
    }

}
