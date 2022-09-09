using Intervip.Core.Enums;
using Intervip.Core.Models;
using Intervip.WebApi.Controllers.Api;
using Intervip.WebApi.Interfaces;

using Microsoft.AspNetCore.Mvc;

using Moq;

namespace Intervip.WebApi.Tests;

public class PostalCodeControllerTests
{
	private readonly Mock<IPostalCodeService> postalCodeService;

	public PostalCodeControllerTests()
	{
		postalCodeService = new Mock<IPostalCodeService>();
	}

	[Fact]
	public async Task GetPostalCode_ShouldReturnPostalCode_WhenExists()
	{
		// Arrange
		var code = "29166710";
		var postalCode = new PostalCode
		{
			Id = new Guid(),
			Code = code,
			Street = "Rua dos Uirapurus",
			Neighbourhood = "Morada de Laranjeiras",
			City = "Serra",
			State = States.ES
		};
		postalCodeService.Setup(x => x.GetPostalCodeByCode(code)).ReturnsAsync(postalCode);
		var postalCodeController = new PostalCodeController(postalCodeService.Object);

		// Act
		ActionResult<PostalCode>? response = await postalCodeController.OnGetAsync(code);
		var result = response?.Result as OkObjectResult;

		// Assert
		Assert.NotNull(result);
		Assert.True(result.StatusCode == 200);
		Assert.Equal(result.Value, postalCode);
	}
}