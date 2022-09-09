using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System.Reflection;

namespace Intervip.WebApi.Configuration;

public class SwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
{
	private readonly IApiVersionDescriptionProvider provider;
	private readonly string xmlFilePath = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

	public SwaggerOptions(IApiVersionDescriptionProvider provider)
	{
		this.provider = provider;
	}

	public void Configure(SwaggerGenOptions options)
	{
		// Add swagger document for every API version discovered
		foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
		{
			options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
		}
	}

	public void Configure(string? name, SwaggerGenOptions options)
	{
		// Add XML comments to OpenAPI documentation
		options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilePath));
		Configure(options);
	}

	private static OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
	{
		var info = new OpenApiInfo()
		{
			Title = "Intervip Intranet API",
			Version = description.ApiVersion.ToString(),
			Contact = new OpenApiContact
			{
				Name = "Bruno Blanes",
				Email = "bruno.blanes@intervip.net.br"
			}
		};

		if (description.IsDeprecated)
		{
			info.Description += " This API version has been deprecated.";
		}

		return info;
	}
}