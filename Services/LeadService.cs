using Azure.Core;

using Intervip.Core.Data;
using Intervip.Core.Models;
using Intervip.WebApi.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace Intervip.WebApi.Services;

internal class LeadService : ILeadService
{
	private readonly ApplicationDbContext context;

	public LeadService(ApplicationDbContext context)
	{
		this.context = context;
	}

	public async Task AddLeadAsync(Lead lead)
	{
		// Postal code is registered in the database
		if (await context.PostalCodes.FirstOrDefaultAsync(postalCode =>
			postalCode.Code == lead.Address.PostalCode.Code) is PostalCode postalCode)
		{
			lead.Address.PostalCode = postalCode;
		}

		// Address is registered in the database
		if (await context.Addresses.FirstOrDefaultAsync(address =>
			address.Lot == lead.Address.Lot &&
			address.Square == lead.Address.Square &&
			address.Number == lead.Address.Number &&
			address.PostalCodeId == lead.Address.PostalCodeId) is Address address)
		{
			lead.Address = address;
		}

		// Lead is not in database
		if (await context.Leads.FirstOrDefaultAsync(l =>
			l.Name == lead.Name &&
			l.Email == lead.Email &&
			l.PhoneNumber == lead.PhoneNumber &&
			l.Address == lead.Address) is null)
		{
			// Set lot and square to upper
			lead.Address.Lot = lead.Address.Lot?.ToUpperInvariant();
			lead.Address.Square = lead.Address.Square?.ToUpperInvariant();

			// Save to database
			context.Leads.Add(lead);
			await context.SaveChangesAsync();
		}
	}
}
