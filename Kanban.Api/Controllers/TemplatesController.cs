using Kanban.Api.Models;
using Kanban.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Kanban.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TemplatesController : ControllerBase
{
    private readonly TemplateService _templateService;

    public TemplatesController(TemplateService templateService)
    {
        _templateService = templateService;
    }

    // GET /api/templates
    [HttpGet]
    public async Task<List<Template>> GetAll()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return await _templateService.GetTemplatesForUser(userId);
    }
}