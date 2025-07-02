using System;
using CrmAPI.Application;
using CrmAPI.Infrastructure;
using CrmAPI.Infrastructure.Monitoring;
using CrmAPI.Infrastructure.Persistence;
using CrmAPI.Infrastructure.Utilities;
using CrmAPI.Presentation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

SerilogConfiguration.SetupBootstrapLogging();

var builder = WebApplication.CreateBuilder(args);

// This check ensures detection of bad DI config early on development time. This misconfiguration is hard to detect.
// ASP.NET by default does this check *only when run in Development mode*. Below we enforce that if env. is not
// Production or Staging, this check is always performed. Reason is that our house has habit to develop with "Local".
// This check comes from real life fights, I had lost a ton of time debugging bad DI config :-D.
if (!builder.Environment.IsLikeProduction())
{
    builder.WebHost.UseDefaultServiceProvider(
        opts =>
        {
            opts.ValidateOnBuild = true;
            opts.ValidateScopes = true;
        });
}

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment, builder.Host, builder.Logging);
builder.Services.AddApplication();
builder.Services.AddPresentation(builder.Configuration);
//builder.Services.UseHttpClientMetrics();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseAuthentication();

// Configure the HTTP request pipeline.
if (!app.Environment.IsLikeProduction())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();

    if (!app.Configuration.IsNSwagRunning())
    {
        // Initialise and seed database
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextSeed>();
        await initializer.RunSeeders();
    }
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors("CorsPolicy");

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwaggerUi3(
    settings =>
    {
        settings.DocExpansion = "list";
        settings.DocumentTitle = "CRM API Swagger";
        settings.OAuth2Client = new()
        {
            ClientId = builder.Configuration["Jwt:Audience"],
            AppName = builder.Configuration["Jwt:Audience"],
            AdditionalQueryStringParameters = { { "nonce", "1" } },
            Realm = builder.Configuration["Jwt:Realm"],
        };
        settings.Path = "/swagger";
        settings.DocumentPath = "/swagger/specification.json";
    });

app.UseRouting();

app.UseAuthorization();

app.Use(
    async (context, next) =>
    {
        context.Response.Headers.Add("X-Replica-ID", Environment.GetEnvironmentVariable("REPLICANAME") ?? "Unknown");
        await next.Invoke();
    });

/*app.MapMetrics();
app.UseHttpMetrics(o => o.ReduceStatusCodeCardinality());*/
app.MapControllers();

await app.RunAsync();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
