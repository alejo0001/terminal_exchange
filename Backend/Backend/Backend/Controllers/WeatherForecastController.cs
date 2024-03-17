using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<Formulario> Get()
        {    


            using (Models.MasterContext db = new Models.MasterContext())
            {

                List<Formulario> response = new List<Formulario>();

                response = (from i in db.Formularios
                            select i).ToList();

                return response;
            }
        }

        [HttpPost(Name = "GetWeatherForecast")]
        public Formulario Guardar(Formulario nuevo)
        {
            using (Models.MasterContext db = new Models.MasterContext())
            {
                //var _errorModel = new ErrorModel();
                try
                {

                    db.Formularios.Add(nuevo);
                    
                    db.SaveChanges();

                    return nuevo;
                }
                catch (Exception ex)
                {
                    return nuevo;
                }
            }
        }

    }
}
