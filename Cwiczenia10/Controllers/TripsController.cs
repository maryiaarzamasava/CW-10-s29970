using Cwiczenia10.DTOs;
using Cwiczenia10.Exceptions;
using Cwiczenia10.Models;
using Cwiczenia10.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cwiczenia10.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController(IDbService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTripsDetails([FromQuery] int? page = null, [FromQuery] int? pageSize = null)
    {
        return Ok(await service.GetTripsDetailsAsync(page, pageSize));
    }

    [HttpGet("{idTrip}/clients/{clientId}")]
    public async Task<IActionResult> GetClientTripDetails([FromRoute] int idTrip,[FromRoute] int clientId)
    {
        try
        {
            return Ok(await service.GetClientTripDetailsByIdAsync(clientId, idTrip));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AddClient([FromRoute] int idTrip, [FromBody] ClientTripCreateDto clientTripData)
    {
        try
        {
            var clientTrip = await service.CreateClientTripAsync(idTrip, clientTripData);
            return CreatedAtAction(nameof(GetClientTripDetails), new { idTrip = clientTrip.IdTrip, clientId = clientTrip.IdClient }, clientTrip);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ClientAlreadyOnTripException e)
        {
            return Conflict(e.Message);
        }
        catch (TripAlreadyHappenedException e)
        {
            return BadRequest(e.Message);
        }
    }
}