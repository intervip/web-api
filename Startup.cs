using Intervip.Core.Data;
using Intervip.WebApi.Configuration;
using Intervip.WebApi.Interfaces;
using Intervip.WebApi.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;

using System.Text.Json.Serialization;

namespace Intervip.Api.Server;

public class Startup
{
	public IConfiguration Configuration { get; init; }

	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;
	}

	public void ConfigureServices(IServiceCollection services)
	{
		// Sets the database connection string
		// TODO: Null check
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseSqlServer(Configuration["INTERVIP_CONNECTION"]!));

		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
		.AddJwtBearer(options =>
		{
			options.Audience = "ivp.api";
			options.Authority = "https://localhost/accounts";
		});

		services.AddCors(options =>
		{
			options.AddDefaultPolicy(policy =>
			{
				policy.WithHeaders("X-Version", "Content-Type");
				policy.WithOrigins("http://localhost", "https://localhost");
			});
		});

		services.AddControllers().AddJsonOptions(options =>
		{
			options.JsonSerializerOptions.WriteIndented = true;
			options.JsonSerializerOptions.PropertyNamingPolicy = null;
			options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
			options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
		});

		// ViaCEP API HTTP client configuration
		services.AddHttpClient<IPostalCodeService, PostalCodeService>(options =>
			options.BaseAddress = new Uri("https://viacep.com.br/ws/"));

		// Add in-memory cache with 1024 cached items limit
		services.AddMemoryCache(options => options.SizeLimit = 1024);
		
		// Add API versioning with default version
		services.AddApiVersioning(options =>
		{
			options.ReportApiVersions = true;
			options.DefaultApiVersion = ApiVersion.Default;
			options.AssumeDefaultVersionWhenUnspecified = true;
			options.ApiVersionReader = new HeaderApiVersionReader("X-Version");
		}).AddVersionedApiExplorer(options => options.GroupNameFormat = "'v'VVV");

		// Add endpoint abstracted services
		services.AddScoped<ILeadService, LeadService>();

		services.AddEndpointsApiExplorer();

		// Add OpenAPI support
		services.AddSwaggerGen();
		services.ConfigureOptions<SwaggerOptions>();

		services.AddSignalR();
	}

	public void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider provider)
	{
		app.UseSwagger();

		// Add Swagger to the root path
		app.UseSwaggerUI(options =>
	{
			options.RoutePrefix = "swagger";

			foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
			{
				options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
			}
		});

		app.UseRouting();
		app.UseCors();

		app.UseAuthentication();
		app.UseAuthorization();

		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllerRoute("default", "{controller=name}/{action=index}");
		});
	}
}