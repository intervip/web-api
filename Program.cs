using Intervip.Core.Data;

using Microsoft.EntityFrameworkCore;

namespace Intervip.Api.Server;

public class Program
{
	public static async Task Main(string[] args)
	{
		IHost? host = CreateHostBuilder(args).Build();
		ApplicationDbContext? context = host.Services.CreateScope()
			.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		await context.Database.EnsureCreatedAsync();
		await context.Database.MigrateAsync();
		await host.RunAsync();
	}

	private static IHostBuilder CreateHostBuilder(string[] args)
	{
		return Host.CreateDefaultBuilder(args)
			.ConfigureWebHostDefaults(webBuilder =>
				webBuilder.UseStartup<Startup>());
	}
}
