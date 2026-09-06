using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        var card = await _cardService.CreateCard(request);
        if (card is null) return BadRequest();

        await NotifyBoardChanged(await _boardService.GetBoardIdFromColumn(card.ColumnId));
        return CreatedAtAction(nameof(Create), new { id = card.Id }, card);
    }

    // DELETE /api/cards/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var card = await _cardService.GetCardById(id);
        if (card is null) return NotFound();

        var boardId = await _boardService.GetBoardIdFromColumn(card.ColumnId);

        bool deleteResponse = await _cardService.DeleteCard(card);
        if (!deleteResponse) return NotFound();

        await NotifyBoardChanged(boardId);
        return NoContent();
    }

    // PUT /api/cards/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCardRequest request)
    {
        var card = await _cardService.GetCardById(id);
        if (card is null) return NotFound();

        bool updateResponse = await _cardService.UpdateCard(card, request);
        if(!updateResponse) return NotFound();


        await NotifyBoardChanged(await _boardService.GetBoardIdFromColumn(card.ColumnId));
        return NoContent();
    }

    // PUT /api/cards/{id}/move
    [HttpPut("{id}/move")]
    public async Task<IActionResult> Move(int id, MoveCardRequest request)
    {
        var card = await _cardService.GetCardById(id);
        if (card is null) return NotFound();

        var moveResponse = await _cardService.MoveCard(card, request);

        await NotifyBoardChanged(await _boardService.GetBoardIdFromColumn(card.ColumnId));
        return NoContent();
    }

}

