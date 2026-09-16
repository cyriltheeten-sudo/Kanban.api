using Kanban.Api.Hubs;
using Kanban.Api.Models;
using Kanban.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Kanban.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CardsController : ControllerBase
{
    private readonly IHubContext<KanbanHub> _hub;
    private readonly BoardService _boardService;
    private readonly CardService _cardService;
    public CardsController(IHubContext<KanbanHub> hub, CardService cardService, BoardService boardService)
    {
        _hub = hub;
        _cardService = cardService;
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
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var boardId = await _boardService.GetBoardIdFromColumn(request.ColumnId);
        if (!await _boardService.IsBoardOwnedBy(boardId, userId)) return NotFound();

        var card = await _cardService.CreateCard(request);
        if (card is null) return BadRequest();

        await NotifyBoardChanged(boardId);
        return CreatedAtAction(nameof(Create), new { id = card.Id }, card);
    }

    // DELETE /api/cards/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var card = await _cardService.GetCardById(id);
        if (card is null) return NotFound();

        var boardId = await _boardService.GetBoardIdFromColumn(card.ColumnId);
        if (!await _boardService.IsBoardOwnedBy(boardId, userId)) return NotFound();

        await _cardService.DeleteCard(card);
        await NotifyBoardChanged(boardId);
        return NoContent();
    }

    // PUT /api/cards/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCardRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var card = await _cardService.GetCardById(id);
        if (card is null) return NotFound();

        var boardId = await _boardService.GetBoardIdFromColumn(card.ColumnId);
        if (!await _boardService.IsBoardOwnedBy(boardId, userId)) return NotFound();

        await _cardService.UpdateCard(card, request);
        await NotifyBoardChanged(boardId);
        return NoContent();
    }

    // PUT /api/cards/{id}/move
    [HttpPut("{id}/move")]
    public async Task<IActionResult> Move(int id, MoveCardRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var card = await _cardService.GetCardById(id);
        if (card is null) return NotFound();

        var boardId = await _boardService.GetBoardIdFromColumn(card.ColumnId);
        if (!await _boardService.IsBoardOwnedBy(boardId, userId)) return NotFound();

        var targetBoardId = await _boardService.GetBoardIdFromColumn(request.ColumnId);
        if (!await _boardService.IsBoardOwnedBy(targetBoardId, userId)) return NotFound();

        await _cardService.MoveCard(card, request);
        await NotifyBoardChanged(boardId);
        return NoContent();
    }

    [HttpPut("{cardId}/entries/{columnId}")]
    public async Task<IActionResult> UpsertEntry(int cardId, int columnId, UpsertCardEntryRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var boardId = await _boardService.GetBoardIdFromColumn(columnId);
        if (!await _boardService.IsBoardOwnedBy(boardId, userId)) return NotFound();

        var card = await _cardService.GetCardById(cardId);
        if (card is null) return NotFound();

        var cardBoardId = await _boardService.GetBoardIdFromColumn(card.ColumnId);
        if (cardBoardId != boardId) return NotFound();

        await _cardService.UpsertEntry(cardId, columnId, request);

        await NotifyBoardChanged(boardId);
        return NoContent();
    }

}