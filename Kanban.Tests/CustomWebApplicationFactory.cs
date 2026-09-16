using System.Linq;
using Kanban.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kanban.Tests;

// Spins up the real ASP.NET Core pipeline (routing, auth, controllers) against
// an isolated in-memory database, so authorization rules are exercised exactly
// as a real HTTP client would trigger them.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    public CustomWebApplicationFactory()
    {
        // Program.cs reads Jwt:Key from configuration into a local variable
        // before WebApplicationFactory gets a chance to inject test config, so
        // the value has to be in place as an env var before the host is built.
        Environment.SetEnvironmentVariable("Jwt__Key", "integration-test-signing-key-do-not-use-in-production");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_dbName));
        });
    }
}
