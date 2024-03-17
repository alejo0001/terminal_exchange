using Microsoft.Extensions.DependencyInjection;
namespace Backend
{
    public class Startup
    {


// ConfigureServices method
     void ConfigureServices(IServiceCollection services)
    {
        // Configuración de otros servicios aquí

        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin",
                builder =>
                {
                    builder.WithOrigins("http://localhost:4200")
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
        });

        // Más configuración de servicios si es necesario
    }

    void Configure(IApplicationBuilder app)
    {
        // Otros middlewares aquí

        app.UseCors("AllowSpecificOrigin");

        // Más configuración de middleware si es necesario
    }
}
}
