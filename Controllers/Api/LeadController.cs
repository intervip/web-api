using Intervip.Core.Models;
using Intervip.WebApi.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace Intervip.WebApi.Controllers.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("[controller]")]
[Produces("application/json")]
public class LeadController : ControllerBase
{
	private readonly ILeadService leadService;

	public LeadController(ILeadService leadService)
	{
		this.leadService = leadService;
	}

	[HttpPost]
	[MapToApiVersion("1.0")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Lead>?> OnPostAsync([FromBody] Lead lead)
	{
		if (ModelState.IsValid is not true)
		{
			return BadRequest();
		}

		if (lead.Address.Square?.Length > 4 || lead.Address.Lot?.Length > 4)
		{
			return BadRequest();
		}

		await leadService.AddLeadAsync(lead);
		return Created(new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}/{lead.Id}"), lead);
	}
}
