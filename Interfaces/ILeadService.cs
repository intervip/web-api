using Intervip.Core.Models;

namespace Intervip.WebApi.Interfaces;

public interface ILeadService
{
	Task AddLeadAsync(Lead lead);
}
