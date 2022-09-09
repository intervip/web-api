using Intervip.Core.Models;

namespace Intervip.WebApi.Interfaces;

public interface IPostalCodeService
{
	internal Task<PostalCode?> GetPostalCodeByCode(string code);
}
