using Intervip.Core.Data;
using Intervip.Core.Enums;
using Intervip.Core.Models;
using Intervip.WebApi.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using System.Text.Json.Nodes;

namespace Intervip.WebApi.Services;

public class PostalCodeService : IPostalCodeService
{
	private readonly ApplicationDbContext context;
	private readonly IMemoryCache memoryCache;
	private readonly HttpClient httpClient;

	public PostalCodeService(ApplicationDbContext context, IMemoryCache memoryCache, HttpClient httpClient)
	{
		this.context = context;
		this.httpClient = httpClient;
		this.memoryCache = memoryCache;
	}

	public async Task<PostalCode?> GetPostalCodeByCode(string code)
	{
		// Found in cache
		if (memoryCache.TryGetValue(code, out PostalCode? postalCode))
		{
			return postalCode;
		}

		postalCode = await context.PostalCodes.FirstOrDefaultAsync(x => x.Code == code);

		// Found in local database
		if (postalCode is not null)
		{
			// Add postal code to memory cache for a day
			memoryCache.Set(postalCode.Code, postalCode, new MemoryCacheEntryOptions
			{
				SlidingExpiration = TimeSpan.FromDays(1),
				Size = 1
			});
			return postalCode;
		}

		// Request postal code from ViaCEP API
		if (await httpClient.GetAsync($"{code}/json") is HttpResponseMessage response && response.IsSuccessStatusCode)
		{
			if (JsonNode.Parse(await response.Content.ReadAsStringAsync()) is JsonNode json)
			{
				// Postal code not found at ViaCEP
				if (json["erro"]?.GetValue<bool>() ?? false)
				{
					return null;
				}

				postalCode = new PostalCode
				{
					Code = json["cep"]!.GetValue<string>().Replace("-", string.Empty),
					Street = json["logradouro"]!.GetValue<string>(),
					Neighbourhood = json["bairro"]!.GetValue<string>(),
					City = json["localidade"]!.GetValue<string>(),
					State = Enum.Parse<States>(json["uf"]!.GetValue<string>(), true),
				};
			}
		}

		if (postalCode is not null)
		{
			// Add the new postal code to local database
			context.PostalCodes.Add(postalCode);
			await context.SaveChangesAsync();

			// Return the newly created postal code
			return postalCode;
		}

		// Postal code not found
		return null;
	}
}
