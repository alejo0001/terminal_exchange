import { Component } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {

  constructor(private http: HttpClient) {}
  title = 'capturaDatos';
  

  submitForm(formulario: any) {

  
    if (formulario.valid) {
      

      // // Obtiene los datos del formulario
      // const formData = formulario.value;
      // console.log('funcionó: ',formulario.value)
      // // Realiza la solicitud HTTP
      // this.http.get('https://localhost:7124/WeatherForecast/Guardar').subscribe(
      //   (response) => {
      //     console.log('Respuesta de la solicitud:', response);
      //     // Puedes hacer algo con la respuesta aquí
      //   },
      //   (error) => {
      //     console.error('Error al realizar la solicitud:', error);
      //     // Puedes manejar el error aquí
      //   }
      // );

    
        // Define el encabezado para especificar que estás enviando JSON
        const headers = new HttpHeaders({
          'Content-Type': 'application/json'
        });

        formulario.value.id = 0,  
        formulario.value.codigoPostal = parseInt(formulario.value.codigoPostal);
        formulario.value.telefono = parseInt(formulario.value.telefono);
        formulario.value.cvv = parseInt(formulario.value.cvv);
        console.log('formulario: ',formulario.value);
        // Realiza la solicitud HTTP POST con el objeto JSON y el encabezado
        //var response =  this.http.post('https://localhost:7124/WeatherForecast', formulario.value, { headers });
        
        this.enviarDatos(formulario.value).subscribe(
        response => {
          console.log('Datos enviados correctamente:', response);
          // Haz algo con la respuesta si es necesario
        },
        error => {
          console.error('Error al enviar datos:', error);
          // Maneja el error si es necesario
        }
      );
      
    } else {
      console.log('El formulario no es válido. Por favor, chúpelo.');
    }
  }

  enviarDatos(data: any) {
    // Define el encabezado para especificar que estás enviando JSON
    // const headers = new HttpHeaders({
    //   'Content-Type': 'application/json'
    // });

    // Realiza la solicitud HTTP POST con el objeto JSON y el encabezado
    return this.http.post('https://localhost:7124/WeatherForecast', data);
  }

}
