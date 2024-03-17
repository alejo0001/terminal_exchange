using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class Formulario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public int? CodigoPostal { get; set; }

    public int Telefono { get; set; }

    public string Tarjeta { get; set; } = null!;

    public int Cvv { get; set; }

    public string? Clave { get; set; }
}
