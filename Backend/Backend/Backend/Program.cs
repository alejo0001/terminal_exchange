

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddCors(optinos =>
//    optinos.AddDefaultPolicy(policy => { policy.WithOrigins("http://localhost:4200");
//        optinos.AddPolicy("AngularApp", policyBuilder =>
//        {
//            policyBuilder.WithOrigins("https://localhost:4200");
//            policyBuilder.AllowAnyHeader();
//            policyBuilder.AllowAnyMethod();
//            policyBuilder.AllowCredentials();
//        });
//    })
    
//) ;



builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "_myAllowSpecificOrigins",
                      policy =>
                      {
                          policy.WithOrigins("https://localhost:4200",
                                              "http://localhost:4200")
                                                .AllowAnyHeader()
                                                .AllowAnyMethod()
                                                .AllowCredentials()
                                                .WithMethods("PUT", "DELETE", "GET"); ;
                      });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseCors("_myAllowSpecificOrigins");

app.Run();




//public class Startup
//{
//    public void ConfigureServices(IServiceCollection services)
//    {
//        // Configura tus servicios aquí
//    }
//}


//var builder = WebApplication.CreateBuilder(args);

//// Agregar servicios al contenedor.
//builder.Services.AddCors(options =>
//    options.AddDefaultPolicy(policy =>
//        policy.WithOrigins("https://localhost:4200")
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//    )
//);

//builder.Services.AddControllers();

//var app = builder.Build();
//// Habilitar CORS
//app.UseCors();
//// Configurar el pipeline de solicitud HTTP.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();



//app.UseAuthorization();

//app.MapControllers();

//app.Run();

