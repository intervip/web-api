using Intervip.Core.Models;
using Intervip.WebApi.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace Intervip.WebApi.Controllers.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("[controller]")]
[Produces("application/json")]
public class PostalCodeController : ControllerBase
{
	private readonly IPostalCodeService postalCodeService;

	public PostalCodeController(IPostalCodeService postalCodeService)
	{
		this.postalCodeService = postalCodeService;
	}

	/// <summary>
	/// Returns a PostalCode object.
	/// </summary>
	/// <param name="code">The CEP identifier as an eight number string.</param>
	/// <returns>A PostalCode object who's Id member matches the provided "id" parameter.</returns>
	/// <response code="200">Returns the found PostalCode object from the database or memory cache.</response>
	/// <response code="400">The provided "id" parameter is not in the correct format.</response>
	/// <response code="404">The provided "id" parameter didn't match any known postal code.</response>
	[HttpGet("{code}")]
	[MapToApiVersion("1.0")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<PostalCode>?> OnGetAsync(string code)
	{
		// Code is invalid
		if (!ModelState.IsValid || code.Length > 8 || int.TryParse(code, out _) is false)
		{
			return BadRequest();
		}

		if (await postalCodeService.GetPostalCodeByCode(code) is PostalCode postalCode)
		{
			return Ok(postalCode);
		}

		// Postal code not found
		return NotFound();
	}
}