# Endpoints

#### OrderImported

CreateOrdersImportedFromTlmkCommand: _recogemos la información de una orden que nos llegará desde el back de tlmk. Asocisaremos el process que nos envia con una nueva orden (OrderImported) y guardamos la información_
SetProcessStatusByOrderNumber: _Desde la web se llamará a este endpoint cuando el cliente rellene correctamente los datos bancarios. Se cambiará el estado y el outcome del proceso según se haya o no cumplimentado bien la información del pago_


#### Procesos
GetProcessByUserWithPagination: _devolvemos procesos según usuarioordenados de manera que primero aparecen los impagos y luego ordenados por fecha de próxima interacción. Si el desde front se pasan QueryParams el orden es el de dichos QueryParams_
UpdateProcessCommandHandler: _Si nos viene un proceso con venta fallida y cierre, quiere decir que es un descarte. Agregamos una acción de tipo descarte en la base de datos_

GetSalesByUserWithPagination: _devolvemos las ventas completadas según usuario ordenadas por fecha_

GetProcessSaleStatusQuery: _le devolvemos al front si el proceso que nos ha enviado  está en el estado correcto y si tiene una orden de compra asociada_

GetProcessSaleAttemtps: _devolvemos al back los intentos de venta de un proceso_

#### Contactos
GetContactDetails: _devolvemos toda la información de contacto junto a sus ContactLeads, sus OrdersImported (entidad relacionada con pedidos de pedido_tlmk) y si tiene una llamada activa. Para obtenerlo necesitamos que el front nos pase el contactId y el processId al que quieren acceder. Las acciones van dentro del objeto Process. 
    También se muestra el campo Colour del proceso en el que se encuentra el contacto y el número de intentos de venta. 
    También se incluye el currencySaleCountry que nos indicará si un curso ha sido cobrado en una moneda distinta al país de venta
    También se incluye el CurrencySymbol del país de la dirección favorita del contacto, en caso de no tener direcciones se toma por defecto el de España._
    También se incluye el ContactLead.

UpdateContactCommand: _permite editar el contacto: en caso de que su estado cambie a No Válido, Blacklist, Jubilado o Fallecido se borran todas las citas que pueda tener pendientes_

DeleteContactCommand: _permite borrar a un contacto (IsDeleted = true), un contacto borrado de esta manera también elimina todas las citas que pueda tener pendientes. NextInteraction se establece a null_

CheckContactPhone: _devuelve un ContactInfoDto si existe y tiene un proceso comercial abierto (true/false)_

CheckContactEmail: _devuelve un ContactInfoDto si existe y tiene un proceso comercial abierto (true/false)_

CheckContactIdCard: _devuelve un ContactInfoDto si existe y tiene un proceso comercial abierto (true/false)_

GetContactCountry: _devuelve un objeto CountryDto del país en el que está el visitador_

CheckContactPhoneQuery: _devuelve si un contacto esta o no ocupado para poder abrile un proceso comercial y tamibién informa si es un blackList en base a su teléfono_

CheckContactEmailQuery: _devuelve si un contacto esta o no ocupado para poder abrile un proceso comercial y tamibién informa si es un blackList en base a su email_

CheckContactIdCardQueryHandler: _devuelve si un contacto esta o no ocupado para poder abrile un proceso comercial y tamibién informa si es un blackList en base a su DNI_

CreateContactCommand: _cuando se crea un contacto el campo origin se establece a "manual", si no se le asigna país, el back asigna por defecto el país en el que opera el comercial que lo creó. Devuelve un ContactProcessDto donde pasa el ID de nuevo contacto creado, así como el ID del proceso_
                      _cuando se crea un contacto, se estable por defecto una dirección si esta viene nula desde el front. Se pone por defecto españa o si lo tiene asignado, el país del usuario que lo creó_

GetContactTitlesQuery: _devuelve los títulos de un contacto_

GetLanguagesQuery: _devuelve tabla de Lenguages_

AddContactToBlacklistCommand: _Cuando se agrega un contacto a la black list, creamos una acción de este tipo en la base de datos. NextInteraction se establece a null_

GetContactInfoForTlmkQuery: _Devuelve información de contacto vinculado a un proceso para su consumo desde el front de TLMK, aumenta así mismo el número de intentos de venta del proceso_

CanCreateRecoverProcess: _Devuelve un bool en el cual informa si el comercial ha execido o no el númerno de veces que puede recuperar un contacto con venta fallida_

#### Citas

GetAppointmentsByContact: _devuelve la próxima cita de un contacto con el usuario de la aplicación, si no existe devuelve una cita de tipo llamada para AHORA. Esto suena a petición rara del front. No hay que fiarse de esa gente, tanto pixel les afecta._

GetActionCallInfo: _Obtenemos en número de acciones del día y obtenemos el día del proceso en base a los días libres y las ausencias del comercial. También se obtiene si el 3x3x3 está activo en caso de que no existan citas a futuro_

CreateAppointmentCommand: _Cuando se crea una cita, también creamos una acción de este tipo en la base de datos, y el evento en el calendario. Se mantiene el histórico de citas creadas, existiendo sólo una activa (IsDeleted = false). En la tabla Appointments la columna EventId hace referencia al evento en el calendario, habiendo sólo una cita de calendario por proceso/contacto_

#### Appointents
CreateAppointment:  _si no existe una cita anterior, se crea la nueva cita, pero si existe, la buscamos y se establece el valor de IsDeleted a true y el de EventId (para el calendario) a null. Para el Front, un usuario solo puede tener una cita._
GetAppointmentsByContact: _Obtenemos la cita del contacto seleecionado, sólo debería de tener una sola cita._

#### EmailTemplates
GetEmailTemplates: _devuelve lista de plantillas de correos electrónicos para que los comerciales puedan seleccionar cuál les interesa aplicar_
GetEmailTemplate: _devuelve plantilla personalizada según contacto, proceso y curso seleccionado, también se puede enviar EmailTemplateId para forzar que se aplique una plantilla_
SendEmail: _A la hora de enviar el email, actualizamos también el proceso a pending. Si el contacto tiene cursos de interés se le adjuntará el dossier del curso correspondiente_

### WhatsappTemplates
GetWhatsappTemplates: _devuelve lista de plantillas para mensajes de whatsapp para que los comerciales puedan seleccionar cuál les interesa aplicar_
GetWhatsappTemplate: _devuelve plantilla personalizada según contacto, proceso y curso seleccionado, también se puede enviar WhatsappTemplateId para forzar que se aplique una plantilla_

#### CourseCountries
GetCourseCountriesByCountryCodeAndLanguageCode: _devuelve CourseCountry (entidad Country en db de prod en web/portalsmigrator) según código de país y de lenguaje_

#### Sync
CourseCountries: _Sincronización de la tabla de courseCountries entre las bases de datos de Courses a la de Intranet_
Courses: _Sincronización de la tabla de Courses entre las bases de datos de Courses a la de Intranet_
Faculties: _Sincronización de la tabla de Faculties entre las bases de datos de Courses a la de Intranet_
Specialities: _Sincronización de la tabla de Specialities entre las bases de datos de Courses a la de Intranet_
CountryFacultySpecialities: _Sincronización de la tabla de CountryFacultySpecialities entre las bases de datos de Courses a la de Intranet_
CountryFaculties: _Sincronización de la tabla de CountryAreas entre las bases de datos de Courses a la de Intranet_
FacultySpecialities: _Sincronización de la tabla de AreaSpecialities entre las bases de datos de Courses a la de Intranet_
Guarantors: _Sincronización de la tabla de Avalistas entre las bases de datos de Courses a la de Intranet_
CourseData: _Sincronización de la tabla de CourseData entre las bases de datos de Courses a la de Intranet (**CUIDADO CON ESTO**)_
CourseDataGuarantors: _Sincronización de la tabla de CourseDataGuarantors (relación entre CourseData y avalistas) entre las bases de datos de Courses a la de Intranet_

# Validadores

#### Actions
GetActionsQuery: _Devolvemos las acciones de un proceso sin paginado_
CreateActionCommandValidator: _se comprueba tanto para llamadas como para el envío de mails la política del 3x3x3 (en un proceso al contacto se le pueden enviar durante el periodo de 3 días, 3 llamadas/emails en cada uno de esos 3 días) excepto cuando el proceso tiene cita programada_
CreateCommandValidator: _se comprueba que el contacto tenga estado, género y tipo válidos. También se comprueba que el IdCard(DNI...), email y teléfono no existan_
UpdateActionCommandValidator: _A la hora de modificar una accion de tipo llamada y con el resultado de "No responde" guardamos en NextInteration de la tabla de contactos la próxima interacción, que será 3 horas después de haber hecho la llamada. Si es la última acción, calculamos el próximo día ideal para guardar la próxima interacción_

#### Contacts
CreateContactCommandValidator: _Se comprueba si el usuario tiene permisos. Se comprueba que el idCard no se haya registrado anteriormente para otro contacto._
GetContactDetailsQueryValidator: _Se comprueba si el usuario tiene permisos._
UpdateContactCommandValidator: _Se comprueba si el usuario tiene permisos. Se comprueba que el idCard no se haya registrado anteriormente para otro contacto._
DeleteContactCommandValidator: _Se comprueba si el usuario tiene permisos._
AddEmailContact: _Enpoint para agregar un email a un contacto_

#### OrdersImported
GetOrdersImportedByUserWithPaginationQueryValidator: _Se comprueba si el usuario tiene permisos._

#### Processes
GetProcessesByUserWithPaginationQueryValidator: _Se comprueba si el usuario tiene permisos._
CheckContactHasProcessOpened: _Antes de crear un proceso comercial, se comprueba si ese contacto ya tiene un proceso abierto_

#### Sales
GetSalesByUserWithPaginationQueryValidator: _Se comprueba si el usuario tiene permisos._

# Servicios

#### WorkingDays:
_Obtiene el número de días laborales a partir de una fecha de inicio y fecha de fin (excluye sábados y domingos)_

### IsDefaultService:
_Comprueba que sólo exista un valor por defecto para teléfonos, email y direcciones de contacto, en caso de haber más de uno se establece como IsDefault el primero que tenga dicha propiedad a true_

#### GetEmailTemplate:
_Nos devuelve las plantillas en base a la última acción realizada en el proceso. Si no se sabe o no se han realizado acciones, se retorna la básica_

#### ServiceBusService:
_Servicio para cola de azure_

#### CalendarService:
_Llamadas a Calendar API para crear, editar y eliminar eventos de citas de los contactos_

#### RoleService:
_Comprueba los permisos de usuario en función de sus roles. Ver tabla UserRoles_

# DTOs

#### ProcessDto: 
_Tener en cuenta que si varía la lógica del 3x3x3 (en lo que se refiere al tiempo para la siguiente interacción) se debe actualizar la lógica en el mapeo (AddMinutes)_
_Este dto nos devuelve un campo "ActiveCall" que estará a true si el contacto tiene una llamada activa (sin colgar)_

Colour: _Puede tomar los valores null/"None" (sin color), "Green" (venta segura), "Yellow" (posible venta) o "Red" (descartado) para ayudar al comercial a gestionar sus procesos_

#### ContactAddressDto
IsDefault: _del listado de direcciones de un contacto, solo debe de haber una dirección por defecto_

#### ContactPhoneDto
IsDefault: _del listado de teléfonos de un contacto, solo debe de haber un teléfono por defecto_

#### ContactEmailDto
IsDefault: _del listado de correos de un contacto, solo debe de haber un correo por defecto_

#### ContactLeadDto
EmailSent: _booleano para saber si se ha enviado un correo sobre dicho curso_

CourseId: _campo de relación con curso_

CourseDataId: _campo de relación con CourseData_

CourseCountryId: _es el campo que equivale a "countryId" en la apiCourse. Nos servirá en el front para que luego pueda hacer consultas a esa api_

Currency: _Es el código de moneda en la que se visualizó o agregó el contactLead. Lo necesitará el front para representar el precio en la moneda correcta_

CurrencySymbol: _Símbolo de la moneda en la que se visualizó o agregó el contactLead. Lo necesitará el front para representar el precio en la moneda correcta_

Price: _Precio que recibimos del front o de la web_

Url: _le anteponemos la urlBase y le agregamos el idioma, para construir la url del curso._

    _Ejemplos:_

    - Curso de españa en español: https://www.techtitute.com/educacion/experto-universitario/experto-diseno-analisis-investigacion-educativa
    - Curso de españa en ingles: https://www.techtitute.com/education/expert-universitary/experty-designer-analitics-reachs-education
    - Curso de méxico en español: https://www.techtitute.com/mx/educacion/experto-universitario/experto-diseno-analisis-investigacion-educativa"
    - Curso de méxico en ingles: https://www.techtitute.com/mx/education/expert-universitary/experty-designer-analitics-reachs-education"

#### CourseTypeDto
_Id y descripción del tipo de curso_

### EmailTemplateDto
Dto para las plantillas de emails

### WhatsappTemplateDto
Dto para las plantillas de whatsapps
