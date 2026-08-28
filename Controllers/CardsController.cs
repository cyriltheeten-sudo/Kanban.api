using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kanban.Api.Data;
using Kanban.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Kanban.Api.Hubs;
using Kanban.Api.Services;

namespace Kanban.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CardsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<KanbanHub> _hub;
    private readonly BoardService _boardService;
    private readonly CardService _cardServices;
    public CardsController(AppDbContext context, IHubContext<KanbanHub> hub, CardService cardService, BoardService boardService)
    {
        _context = context;
        _hub = hub;
        _cardServices = cardService;
        _boardService = boardService;
    }

    private async Task NotifyBoardChanged(int boardId)
    {
        var senderConnectionId = Request.Headers["X-Connection-Id"].FirstOrDefault();

        if (senderConnectionId is not null)
            await _hub.Clients.GroupExcept($"board-{boardId}", senderConnectionId)
                .SendAsync("BoardChanged");
        else
            await _hub.Clients.Group($"board-{boardId}").SendAsync("BoardChanged");
    }


    // POST /api/cards
    [HttpPost]
    public async Task<ActionResult<Card>> Create(CreateCardRequest request)
    {
        var card = await _cardServices.CreateCard(request);
        if (card is null) return BadRequest();

        await NotifyBoardChanged(await _boardService.GetBoardIdFromColumn(card.ColumnId));
        return CreatedAtAction(nameof(Create), new { id = card.Id }, card);
    }

    // DELETE /api/cards/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var card = await _cardServices.GetCard(id);
        if (card is null) return NotFound();

        var boardId = await _boardService.GetBoardIdFromColumn(card.ColumnId);

        bool deleteResponse = await _cardServices.DeleteCard(card);
        if (!deleteResponse) return NotFound();

        await NotifyBoardChanged(boardId);
        return NoContent();   // 204 : succès, rien à renvoyer
    }

    // PUT /api/cards/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCardRequest request)
    {
        var card = await _cardServices.GetCard(id);
        if (card is null) return NotFound();

        var boardId = await _boardService.GetBoardIdFromColumn(card.ColumnId);

        bool updateResponse = await _cardServices.UpdateCard(card, request);
        if(!updateResponse) return NotFound();


        await NotifyBoardChanged(await _boardService.GetBoardIdFromColumn(card.ColumnId));
        return NoContent();
    }

    // PUT /api/cards/{id}/move
    [HttpPut("{id}/move")]
    public async Task<IActionResult> Move(int id, MoveCardRequest request)
    {
        var card = await _cardServices.GetCard(id);
        if (card is null) return NotFound();

        var moveResponse = await _cardServices.MoveCard(card, request);

        await NotifyBoardChanged(await _boardService.GetBoardIdFromColumn(card.ColumnId));
        return NoContent();
    }

}

