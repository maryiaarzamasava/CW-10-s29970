using Cwiczenia10.Exceptions;
using Cwiczenia10.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cwiczenia10.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController(IDbService service) : ControllerBase
{
    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClientAsync([FromRoute] int idClient)
    {
        try
        {
            await service.RemoveClientAsync(idClient);
            return NoContent();
        }
        catch (ClientHasTripsException e)
        {
            return Conflict(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}