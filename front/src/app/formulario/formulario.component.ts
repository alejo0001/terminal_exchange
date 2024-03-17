import { Component } from '@angular/core';

@Component({
  selector: 'app-formulario',
  templateUrl: './formulario.component.html',
  styleUrls: ['./formulario.component.css']
})
export class FormularioComponent {
  submitForm(formulario: any) {
    if (formulario.valid) {
      // Aquí puedes enviar los datos del formulario a través de una solicitud HTTP, por ejemplo
      console.log('Formulario válido. Datos:', formulario.value);
    } else {
      console.log('El formulario no es válido. Por favor, complete todos los campos correctamente.');
    }
  }
}