using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Contacts.Commands.AddContactFaculties;
using CrmAPI.Application.Contacts.Commands.AddContactSpecialities;
using CrmAPI.Application.Contacts.Commands.AddContactToBlacklist;
using CrmAPI.Application.Contacts.Commands.AddCourseToFavourite;
using CrmAPI.Application.Contacts.Commands.AddEmailContact;
using CrmAPI.Application.Contacts.Commands.ConsolidateContactsByIdCommand;
using CrmAPI.Application.Contacts.Commands.ConsolidateContactsCommand;
using CrmAPI.Application.Contacts.Commands.CreateContact;
using CrmAPI.Application.Contacts.Commands.CreateContactLead;
using CrmAPI.Application.Contacts.Commands.DeleteContact;
using CrmAPI.Application.Contacts.Commands.DeleteContactLead;
using CrmAPI.Application.Contacts.Commands.RecoverContactActivations;
using CrmAPI.Application.Contacts.Commands.RemoveContactFaculty;
using CrmAPI.Application.Contacts.Commands.RemoveContactSpecialities;
using CrmAPI.Application.Contacts.Commands.UpdateContact;
using CrmAPI.Application.Contacts.Commands.UpdateContactLead;
using CrmAPI.Application.Contacts.Commands.UpdateContactLeadPrice;
using CrmAPI.Application.Contacts.Commands.UpdateContactLeads;
using CrmAPI.Application.Contacts.Commands.UpdateCountryCode;
using CrmAPI.Application.Contacts.Commands.UpdateCurrency;
using CrmAPI.Application.Contacts.Queries.CheckContactEmail;
using CrmAPI.Application.Contacts.Queries.CheckContactIdCard;
using CrmAPI.Application.Contacts.Queries.CheckContactPhone;
using CrmAPI.Application.Contacts.Queries.GetAddressTypes;
using CrmAPI.Application.Contacts.Queries.GetConctacById;
using CrmAPI.Application.Contacts.Queries.GetContactByEmailOrPhone;
using CrmAPI.Application.Contacts.Queries.GetContactDetails;
using CrmAPI.Application.Contacts.Queries.GetContactEmails;
using CrmAPI.Application.Contacts.Queries.GetContactFaculties;
using CrmAPI.Application.Contacts.Queries.GetContactGenders;
using CrmAPI.Application.Contacts.Queries.GetContactInfoForTlmk;
using CrmAPI.Application.Contacts.Queries.GetContactLeadsByContact;
using CrmAPI.Application.Contacts.Queries.GetContactLeadsByContactAndProcess;
using CrmAPI.Application.Contacts.Queries.GetContactPhones;
using CrmAPI.Application.Contacts.Queries.GetContactSpecialities;
using CrmAPI.Application.Contacts.Queries.GetEmailTypes;
using CrmAPI.Application.Contacts.Queries.GetIsContactClient;
using CrmAPI.Application.Contacts.Queries.GetPhoneTypes;
using CrmAPI.Application.Contacts.Queries.GetTitleTypes;
using CrmAPI.Application.Contacts.Queries.GetUserContact;
using CroupierAPI.Contracts.Events;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace CrmAPI.Presentation.Controllers;

public class ContactsController : ApiControllerBase
{
    /// <summary>
    /// Obtains the details of a specific contact.
    /// </summary>
    /// <param name="id">Identifier of the contact to query.</param>
    /// <returns>The details of the requested contact.</returns>
    /// <remarks>
    /// This method returns detailed information about a contact.
    /// 
    /// Example request:
    /// 
    ///     GET /api/Contacts/GetDetails/123
    /// </remarks>
    /// <response code="200">If the contact details were successfully retrieved.</response>
    /// <response code="404">If the contact does not exist.</response>
    /// <response code="500">If an internal server error occurs.</response>    
    [HttpGet("{contactId}/{processId}")]
    public async Task<ActionResult<ContactFullDto>> GetDetails(int contactId, int processId) =>
        await Mediator.Send(new GetContactDetailsQuery { ContactId = contactId, ProcessId = processId });

    /// <summary>
    /// Creates a new contact in the system.
    /// </summary>
    /// <param name="command">Command with the information of the new contact.</param>
    /// <returns>The identifier of the created contact.</returns>
    /// <remarks>
    /// This method allows registering a new contact.
    /// 
    /// Example request:
    /// 
    ///     POST /api/Contacts/Create
    ///     {
    ///         "Name": "New Contact",
    ///         "Email": "email@example.com"
    ///     }
    /// </remarks>
    /// <response code="201">If the contact was successfully created.</response>
    /// <response code="400">If the provided data is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost]
    public async Task<ActionResult<ContactCreated>> Create(CreateContactCommand command) =>
        await Mediator.Send(command);

    /// <summary>
    /// Updates the information of an existing contact.
    /// </summary>
    /// <param name="command">Command with the updated data of the contact.</param>
    /// <returns>The number of records affected by the operation.</returns>
    /// <remarks>
    /// This method allows updating the data of a contact in the system.
    /// 
    /// Example request:
    /// 
    ///     PUT /api/Contacts/Update
    ///     {
    ///         "ContactId": 123,
    ///         "Name": "New Name",
    ///         "Email": "newemail@example.com"
    ///     }
    /// </remarks>
    /// <response code="200">If the contact information was successfully updated.</response>
    /// <response code="404">If the contact does not exist.</response>
    /// <response code="500">If an internal server error occurs.</response>    
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateContactCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Adds a contact to the blacklist.
    /// </summary>
    /// <param name="command">Command that contains the information necessary to perform the operation.</param>
    /// <returns>
    /// An integer representing the number of records affected by the operation.
    /// </returns>
    /// <remarks>
    /// This method updates the status of a contact, closes the related process, removes associated events,
    /// and logs an action of type "Blacklist". It also synchronizes the contact's status in the database.
    /// 
    /// Example request:
    /// 
    ///     PUT /api/Contacts/AddToBlacklist
    ///     {
    ///         "ContactId": 123,
    ///         "NewContactStatusId": 2,
    ///         "NewStatusReason": "Reason for change",
    ///         "NewStatusObservations": "Additional observations",
    ///         "ProcessId": 456
    ///     }
    /// 
    /// </remarks>
    /// <response code="200">If the contact was successfully added to the blacklist.</response>
    /// <response code="403">If the current user does not have permission to perform this operation.</response>
    /// <response code="404">If the specified contact or process cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPut("AddToBlacklist")]
    public async Task<int> AddToBlacklist(AddContactToBlacklistCommand command) => await Mediator.Send(command);

    /// <summary>
    /// Adds a course to the favorites list or removes it from it.
    /// </summary>
    /// <param name="command">
    /// Object that contains the parameters necessary to add or remove a course from favorites:
    /// - <see cref="AddCourseToFavouriteCommand.ContactLeadId"/>: Identifier of the associated contact.
    /// - <see cref="AddCourseToFavouriteCommand.ProcessId"/>: Identifier of the associated process.
    /// - <see cref="AddCourseToFavouriteCommand.CourseFavourite"/>: Indicates whether the course should be added to favorites (true) or removed (false).
    /// </param>
    /// <returns>
    /// An integer representing the number of changes made in the database.
    /// </returns>
    /// <remarks>
    /// This method adds a course to the favorites list if <paramref name="command.CourseFavourite"/> is true,
    /// or removes it if false. The course's status is updated in the database.
    /// 
    /// Example request:
    /// 
    ///     PUT /api/Courses/AddCourseToFavourite
    ///     {
    ///         "ContactLeadId": 123,
    ///         "ProcessId": 456,
    ///         "CourseFavourite": true
    ///     }
    /// 
    /// </remarks>
    /// <response code="200">If the course was successfully added or removed from the favorites list.</response>
    /// <response code="400">If the provided data is invalid.</response>
    /// <response code="404">If the specified contact or process cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response>    
    [HttpPut("AddCourseToFavourite")]
    public async Task<int> AddCourseToFavourite(AddCourseToFavouriteCommand command) => await Mediator.Send(command);

    /// <summary>
    /// Deletes a contact from the database.
    /// </summary>
    /// <param name="id">
    /// Identifier of the contact to be deleted.
    /// </param>
    /// <returns>
    /// An object of type <see cref="ActionResult"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// This method marks the contact as deleted and closes all associated processes.
    /// It also deletes all related calendar events and marks the contact's notes,
    /// phone numbers, emails, and titles as deleted.
    /// 
    /// Example request:
    /// 
    ///     DELETE /api/Contacts/123
    /// 
    /// </remarks>
    /// <response code="204">If the contact was successfully deleted.</response>
    /// <response code="404">If the specified contact cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response>        
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteContactCommand { Id = id });

        return NoContent();
    }

    /// <summary>
    /// Deletes a contact from the database.
    /// </summary>
    /// <param name="id">
    /// Identifier of the contact to be deleted.
    /// </param>
    /// <returns>
    /// An object of type <see cref="ActionResult"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// This method marks the contact as deleted and closes all associated processes.
    /// It also deletes all related calendar events and marks the contact's notes,
    /// phone numbers, emails, and titles as deleted.
    /// 
    /// Example request:
    /// 
    ///     DELETE /api/Contacts/123
    /// 
    /// </remarks>
    /// <response code="204">If the contact was successfully deleted.</response>
    /// <response code="404">If the specified contact cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response>    
    [HttpGet("Gender")]
    public async Task<ActionResult<List<ContactGenderDto>>> GetContactGenders() =>
        await Mediator.Send(new GetContactGendersQuery());

    /// <summary>
    /// Obtains a list of available address types.
    /// </summary>
    /// <returns>
    /// A list of objects of type <see cref="AddressTypeDto"/> representing the address types.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve all available address types.
    /// 
    /// Example request:
    /// 
    ///     GET /api/Address/Types
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the address types are returned.</response>
    /// <response code="500">If an internal server error occurs.</response>    
    [HttpGet("Address/Types")]
    public async Task<ActionResult<List<AddressTypeDto>>> GetAddressTypes() =>
        await Mediator.Send(new GetAddressTypesQuery());

    /// <summary>
    /// Obtains a list of available email types.
    /// </summary>
    /// <returns>
    /// A list of objects of type <see cref="EmailTypeDto"/> representing the email types.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve all available email types.
    /// 
    /// Example request:
    /// 
    ///     GET /api/Email/Types
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the email types are returned.</response>
    /// <response code="500">If an internal server error occurs.</response>    
    [HttpGet("Email/Types")]
    public async Task<ActionResult<List<EmailTypeDto>>> GetEmailTypes() =>
        await Mediator.Send(new GetEmailTypesQuery());

    /// <summary>
    /// Obtains a list of available phone types.
    /// </summary>
    /// <returns>
    /// A list of objects of type <see cref="PhoneTypeDto"/> representing the phone types.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve all available phone types.
    /// 
    /// Example request:
    /// 
    ///     GET /api/Phone/Types
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the phone types are returned.</response>
    /// <response code="500">If an internal server error occurs.</response>    
    [HttpGet("Phone/Types")]
    public async Task<ActionResult<List<PhoneTypeDto>>> GetPhoneTypes() =>
        await Mediator.Send(new GetPhoneTypesQuery());

    /// <summary>
    /// Verifies the contact information associated with an identification number.
    /// </summary>
    /// <param name="query">
    /// Object that contains the identification number of the contact to verify:
    /// - <see cref="CheckContactIdCardQuery.IdCard"/>: Identification number of the contact.
    /// </param>
    /// <returns>
    /// An object of type <see cref="ContactInfoDto"/> containing the contact information.
    /// </returns>
    /// <remarks>
    /// This method searches the database for a contact associated with the provided identification number.
    /// If the contact does not exist, it returns a <see cref="ContactInfoDto"/> object with default values.
    /// If the contact exists, it checks if there are open processes and if a new process can be created.
    /// 
    /// Example request:
    /// 
    ///     GET /api/Check/IdCard?IdCard=12345678
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the contact information is returned.</response>
    /// <response code="500">If an internal server error occurs.</response>    
    [HttpGet("Check/IdCard")]
    public async Task<ContactInfoDto> CheckContactIdCard([FromQuery] CheckContactIdCardQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Verifies the contact information associated with an email address.
    /// </summary>
    /// <param name="query">
    /// Object that contains the email address of the contact to verify:
    /// - <see cref="CheckContactEmailQuery.Email"/>: Email address of the contact.
    /// </param>
    /// <returns>
    /// An object of type <see cref="ContactInfoDto"/> containing the contact information.
    /// </returns>
    /// <remarks>
    /// This method searches the database for a contact associated with the provided email address.
    /// If the contact does not exist, it returns a <see cref="ContactInfoDto"/> object with default values.
    /// If the contact exists, it checks if there are open processes and if a new process can be created.
    /// 
    /// Example request:
    /// 
    ///     GET /api/Check/Email?Email=example@example.com
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the contact information is returned.</response>
    /// <response code="500">If an internal server error occurs.</response>    
    [HttpGet("Check/Email")]
    public async Task<ContactInfoDto> CheckContactEmail([FromQuery] CheckContactEmailQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Verifies the contact information associated with a phone number.
    /// </summary>
    /// <param name="query">
    /// Object that contains the prefix and phone number of the contact to verify:
    /// - <see cref="CheckContactPhoneQuery.PhonePrefix"/>: Phone number prefix.
    /// - <see cref="CheckContactPhoneQuery.Phone"/>: Phone number of the contact.
    /// </param>
    /// <returns>
    /// An object of type <see cref="ContactInfoDto"/> containing the contact information.
    /// </returns>
    /// <remarks>
    /// This method searches the database for a contact associated with the provided phone number.
    /// If the contact does not exist, it returns a <see cref="ContactInfoDto"/> object with default values.
    /// If the contact exists, it checks if there are open processes and if a new process can be created.
    /// 
    /// Example request:
    /// 
    ///     GET /api/Check/Phone?PhonePrefix=1&Phone=5551234567
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the contact information is returned.</response>
    /// <response code="500">If an internal server error occurs.</response>    
    [HttpGet("Check/Phone")]
    public async Task<ContactInfoDto> CheckContactPhone([FromQuery] CheckContactPhoneQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Recovers a new lead contact based on the provided information.
    /// </summary>
    /// <param name="command">
    /// Object that contains the parameters necessary to create a new lead contact:
    /// - <see cref="CreateContactLeadCommand"/>: Information of the lead contact to create.
    /// </param>
    /// <returns>
    /// An integer representing the identifier of the newly created lead contact.
    /// </returns>
    /// <remarks>
    /// This method creates a new lead contact in the database. 
    /// If the course country is not found, an exception is thrown.
    /// 
    /// Example request:
    /// 
    ///     POST /api/Leads
    ///     {
    ///         "PhonePrefix": "1",
    ///         "Phone": "5551234567",
    ///         "Email": "example@example.com",
    ///         "CourseCountryId": 1,
    ///         "University": "TechUniversity",
    ///         "CourseId": 123,
    ///         "ContactId": 456,
    ///         "IsFavourite": true,
    ///         "Types": [ "Recommended", "OtherType" ],
    ///         "StartDateCourse": "2024-01-01",
    ///         "FinishDateCourse": "2024-12-31"
    ///     }
    /// 
    /// </remarks>
    /// <response code="201">If the lead contact was created successfully.</response>
    /// <response code="400">If the provided data is invalid.</response>
    /// <response code="404">If the specified course country is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>   
    [HttpPost("Leads")]
    public async Task<ActionResult<int>> Recover(CreateContactLeadCommand command) => await Mediator.Send(command);

    /// <summary>
    /// Obtains a list of lead contacts associated with a specific contact and process.
    /// </summary>
    /// <param name="query">
    /// Object that contains the parameters necessary for the query:
    /// - <see cref="GetContactLeadsByContactAndProcessQuery.ContactId"/>: Identifier of the contact.
    /// - <see cref="GetContactLeadsByContactAndProcessQuery.ProcessId"/>: Identifier of the process.
    /// </param>
    /// <returns>
    /// A list of objects of type <see cref="ContactLeadDto"/> representing the lead contacts.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve all lead contacts associated with the specified contact and process.
    /// 
    /// Example request:
    /// 
    ///     GET /api/Leads/List?ContactId=123&ProcessId=456
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the lead contacts are returned.</response>
    /// <response code="404">If no lead contacts are found for the specified contact and process.</response>
    /// <response code="500">If an internal server error occurs.</response>    
    [HttpGet("Leads/List")]
    public async Task<ActionResult<List<ContactLeadDto>>> GetContactLeadsByContactAndProcess(
        [FromQuery] GetContactLeadsByContactAndProcessQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Obtains a list of lead contacts associated with a specific contact.
    /// </summary>
    /// <param name="query">
    /// Object that contains the parameter necessary for the query:
    /// - <see cref="GetContactLeadsByContactQuery.ContactId"/>: Identifier of the contact.
    /// </param>
    /// <returns>
    /// A list of objects of type <see cref="ContactLeadDto"/> representing the lead contacts.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve all lead contacts associated with the specified contact.
    /// 
    /// Example request:
    /// 
    ///     GET /api/Leads/List/contactId?ContactId=123
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the lead contacts are returned.</response>
    /// <response code="404">If no lead contacts are found for the specified contact.</response>
    /// <response code="500">If an internal server error occurs.</response>        
    [HttpGet("Leads/List/contactId")]
    public async Task<ActionResult<List<ContactLeadDto>>> GetContactLeadsByContact(
        [FromQuery] GetContactLeadsByContactQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Deletes a specific lead contact.
    /// </summary>
    /// <param name="id">
    /// Identifier of the lead contact to be deleted.
    /// </param>
    /// <returns>
    /// An object of type <see cref="ActionResult"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// This method marks the lead contact as deleted in the database.
    /// If the lead contact is not found, an exception is thrown.
    /// 
    /// Example request:
    /// 
    ///     DELETE /api/Leads/123
    /// 
    /// </remarks>
    /// <response code="204">If the lead contact was successfully deleted.</response>
    /// <response code="404">If the specified lead contact cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpDelete("Leads/{id}")]
    public async Task<ActionResult> DeleteContactLead(int id)
    {
        await Mediator.Send(new DeleteContactLeadCommand { Id = id });

        return NoContent();
    }

    /// <summary>
    /// Obtains a list of faculties associated with a specific contact.
    /// </summary>
    /// <param name="query">
    /// Object that contains the parameter necessary for the query:
    /// - <see cref="GetContactFacultiesQuery.ContactId"/>: Identifier of the contact.
    /// </param>
    /// <returns>
    /// A list of objects of type <see cref="FacultyDto"/> representing the faculties associated with the contact.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve all faculties associated with the specified contact.
    /// 
    /// Example request:
    /// 
    ///     GET /api/ContactFaculties?ContactId=123
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the faculties are returned.</response>
    /// <response code="404">If no faculties are found for the specified contact.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("ContactFaculties")]
    public async Task<ActionResult<List<FacultyDto>>> GetContactFaculties([FromQuery] GetContactFacultiesQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Adds faculties to a specific contact.
    /// </summary>
    /// <param name="command">
    /// Object that contains the parameters necessary to add faculties to the contact:
    /// - <see cref="AddContactFacultiesCommand.ContactId"/>: Identifier of the contact to which the faculties will be added.
    /// - <see cref="AddContactFacultiesCommand.FacultiesId"/>: List of identifiers of the faculties to add.
    /// </param>
    /// <returns>
    /// An integer representing the identifier of the contact to which the faculties have been added.
    /// </returns>
    /// <remarks>
    /// This method adds the specified faculties to the contact in the database.
    /// If the contact is not found, an exception is thrown.
    /// 
    /// Example request:
    /// 
    ///     POST /api/Faculties
    ///     {
    ///         "ContactId": 123,
    ///         "FacultiesId": [1, 2, 3]
    ///     }
    /// 
    /// </remarks>
    /// <response code="200">If the faculties were successfully added to the contact.</response>
    /// <response code="404">If the specified contact cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("Faculties")]
    public async Task<ActionResult<int>> AddFaculties(AddContactFacultiesCommand command) =>
        await Mediator.Send(command);

    /// <summary>
    /// Deletes a faculty associated with a specific contact.
    /// </summary>
    /// <param name="command">
    /// Object that contains the parameters necessary to delete the faculty:
    /// - <see cref="RemoveContactFacultyCommand.FacultyId"/>: Identifier of the faculty to be deleted.
    /// - <see cref="RemoveContactFacultyCommand.ContactId"/>: Identifier of the contact from which the faculty will be removed.
    /// </param>
    /// <returns>
    /// An object of type <see cref="ActionResult"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// This method removes the specified faculty from the contact in the database.
    /// If the contact is not found, an exception is thrown.
    /// 
    /// Example request:
    /// 
    ///     DELETE /api/Faculty
    ///     {
    ///         "FacultyId": 1,
    ///         "ContactId": 123
    ///     }
    /// 
    /// </remarks>
    /// <response code="204">If the faculty was successfully removed from the contact.</response>
    /// <response code="404">If the specified contact cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpDelete("Faculty")]
    public async Task<ActionResult> RemoveFaculty(RemoveContactFacultyCommand command)
    {
        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Obtains a list of specialties associated with a specific contact.
    /// </summary>
    /// <param name="query">
    /// Object that contains the parameter necessary for the query:
    /// - <see cref="GetContactSpecialitiesQuery.ContactId"/>: Identifier of the contact.
    /// </param>
    /// <returns>
    /// A list of objects of type <see cref="SpecialityDto"/> representing the specialties associated with the contact.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve all specialties associated with the specified contact.
    /// 
    /// Example request:
    /// 
    ///     GET /api/ContactSpecialities?ContactId=123
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the specialties are returned.</response>
    /// <response code="404">If no specialties are found for the specified contact.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("ContactSpecialities")]
    public async Task<ActionResult<List<SpecialityDto>>> GetContactSpecialities(
        [FromQuery] GetContactSpecialitiesQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Adds specialties to a specific contact.
    /// </summary>
    /// <param name="command">
    /// Object that contains the parameters necessary to add specialties to the contact:
    /// - <see cref="AddContactSpecialitiesCommand.ContactId"/>: Identifier of the contact to which the specialties will be added.
    /// - <see cref="AddContactSpecialitiesCommand.SpecialitiesId"/>: List of identifiers of the specialties to add.
    /// </param>
    /// <returns>
    /// An integer representing the identifier of the contact to which the specialties have been added.
    /// </returns>
    /// <remarks>
    /// This method adds the specified specialties to the contact in the database.
    /// If the contact is not found, an exception is thrown.
    /// 
    /// Example request:
    /// 
    ///     POST /api/Specialities
    ///     {
    ///         "ContactId": 123,
    ///         "SpecialitiesId": [1, 2, 3]
    ///     }
    /// 
    /// </remarks>
    /// <response code="200">If the specialties were successfully added to the contact.</response>
    /// <response code="404">If the specified contact cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("Specialities")]
    public async Task<ActionResult<int>> AddSpecialities(AddContactSpecialitiesCommand command) =>
        await Mediator.Send(command);

    [HttpDelete("Specialities")]
    public async Task<ActionResult> RemoveSpecialities(RemoveContactSpecialitiesCommand command)
    {
        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Deletes specialties associated with a specific contact.
    /// </summary>
    /// <param name="command">
    /// Object that contains the parameters necessary to delete the specialties:
    /// - <see cref="RemoveContactSpecialitiesCommand.ContactId"/>: Identifier of the contact from which the specialties will be removed.
    /// - <see cref="RemoveContactSpecialitiesCommand.SpecialitiesId"/>: List of identifiers of the specialties to delete.
    /// </param>
    /// <returns>
    /// An object of type <see cref="ActionResult"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// This method removes the specified specialties from the contact in the database.
    /// If the contact is not found, an exception is thrown.
    /// 
    /// Example request:
    /// 
    ///     DELETE /api/Specialities
    ///     {
    ///         "ContactId": 123,
    ///         "SpecialitiesId": [1, 2, 3]
    ///     }
    /// 
    /// </remarks>
    /// <response code="204">If the specialties were successfully removed from the contact.</response>
    /// <response code="404">If the specified contact cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("Title/Types")]
    public async Task<ActionResult<List<TitleTypeDto>>> GetTitleTypes() =>
        await Mediator.Send(new GetTitleTypesQuery());

    /// <summary>
    /// Obtains the contact information for the Tlmk system.
    /// </summary>
    /// <param name="query">
    /// Object that contains the parameters necessary for the query:
    /// - <see cref="GetContactInfoForTlmkQuery.ProcessId"/>: Identifier of the process.
    /// - <see cref="GetContactInfoForTlmkQuery.ApiKey"/>: API key for authentication.
    /// </param>
    /// <returns>
    /// An object of type <see cref="ContactInfoTlmkDto"/> representing the contact information.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve the contact information associated with the specified process.
    /// The information is used to auto-fill a form in the Tlmk system.
    /// 
    /// Example request:
    /// 
    ///     GET /api/Tlmk?ProcessId=123&ApiKey=your_api_key
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the contact information is returned.</response>
    /// <response code="404">If the specified process cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("Tlmk")]
    public async Task<ActionResult<ContactInfoTlmkDto>> GetContactInfoForTlmk(
        [FromQuery] GetContactInfoForTlmkQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Adds a new email address to a specific contact.
    /// </summary>
    /// <param name="command">
    /// Object that contains the parameters necessary to add the email address:
    /// - <see cref="AddNewContactEmailCommand.ContactId"/>: Identifier of the contact to which the email address will be added.
    /// - <see cref="AddNewContactEmailCommand.Email"/>: Email address to add.
    /// - <see cref="AddNewContactEmailCommand.EmailTypeId"/>: Identifier of the email type.
    /// - <see cref="AddNewContactEmailCommand.IsDefault"/>: Indicates whether the email address is the default one.
    /// </param>
    /// <returns>
    /// An integer representing the identifier of the newly added email address.
    /// </returns>
    /// <remarks>
    /// This method adds a new email address to the contact in the database.
    /// If the contact is not found or if the email address already exists, an exception is thrown.
    /// 
    /// Example request:
    /// 
    ///     POST /api/AddEmailContact
    ///     {
    ///         "ContactId": 123,
    ///         "Email": "example@example.com",
    ///         "EmailTypeId": 1,
    ///         "IsDefault": true
    ///     }
    /// 
    /// </remarks>
    /// <response code="200">If the email address was successfully added to the contact.</response>
    /// <response code="404">If the specified contact cannot be found or if the email address has already been added previously.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("AddEmailContact")]
    public async Task<ActionResult<int>> AddEmailContact(AddNewContactEmailCommand command) =>
        await Mediator.Send(command);

    /// <summary>
    /// Obtains a list of email addresses associated with a specific contact.
    /// </summary>
    /// <param name="query">
    /// Object that contains the parameter necessary for the query:
    /// - <see cref="GetContactEmailsQuery.ContactId"/>: Identifier of the contact.
    /// </param>
    /// <returns>
    /// A list of objects of type <see cref="ContactEmailDto"/> representing the email addresses associated with the contact.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve all email addresses associated with the specified contact.
    /// 
    /// Example request:
    /// 
    ///     GET /api/ContactEmails?ContactId=123
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the email addresses are returned.</response>
    /// <response code="404">If no email addresses are found for the specified contact.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("ContactEmails")]
    public async Task<ActionResult<List<ContactEmailDto>>> GetContactEmails([FromQuery] GetContactEmailsQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Obtains a list of phone numbers associated with a specific contact.
    /// </summary>
    /// <param name="query">
    /// Object that contains the parameter necessary for the query:
    /// - <see cref="GetContactPhonesQuery.ContactId"/>: Identifier of the contact.
    /// </param>
    /// <returns>
    /// A list of objects of type <see cref="ContactPhoneDto"/> representing the phone numbers associated with the contact.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve all phone numbers associated with the specified contact.
    /// 
    /// Example request:
    /// 
    ///     GET /api/ContactPhones?ContactId=123
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the phone numbers are returned.</response>
    /// <response code="404">If no phone numbers are found for the specified contact.</response>
    /// <response code="500">If an internal server error occurs.</response> 
    [HttpGet("ContactPhones")]
    public async Task<ActionResult<List<ContactPhoneDto>>> GetContactPhones([FromQuery] GetContactPhonesQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Obtains the complete information of a specific contact by its identifier.
    /// </summary>
    /// <param name="query">
    /// Object that contains the parameter necessary for the query:
    /// - <see cref="GetContactByIdQuery.ContactId"/>: Identifier of the contact.
    /// </param>
    /// <returns>
    /// An object of type <see cref="ContactFullDto"/> representing the complete information of the contact.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve all details of the specified contact.
    /// 
    /// Example request:
    /// 
    ///     GET /api/GetContactById?ContactId=123
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the contact information is returned.</response>
    /// <response code="404">If the specified contact cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response>   
    [HttpGet("GetContactById")]
    public async Task<ActionResult<ContactFullDto>> GetContactById([FromQuery] GetContactByIdQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Updates the country code of a specific contact.
    /// </summary>
    /// <param name="command">
    /// Object that contains the parameters necessary to update the country code:
    /// - <see cref="UpdateCountryCodeCommand.CountryCode"/>: New country code.
    /// - <see cref="UpdateCountryCodeCommand.ContactId"/>: Identifier of the contact whose country code will be updated.
    /// </param>
    /// <returns>
    /// An object of type <see cref="ActionResult"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// This method updates the country code of the contact in the database.
    /// If the contact is not found, an exception is thrown.
    /// 
    /// Example request:
    /// 
    ///     PUT /api/UpdateCountryCode
    ///     {
    ///         "CountryCode": "US",
    ///         "ContactId": 123
    ///     }
    /// 
    /// </remarks>
    /// <response code="204">If the country code was successfully updated.</response>
    /// <response code="404">If the specified contact cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPut("UpdateCountryCode")]
    public async Task<ActionResult> UpdateCountryCode(UpdateCountryCodeCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Updates the currency of a specific contact.
    /// </summary>
    /// <param name="command">
    /// Object that contains the parameters necessary to update the currency:
    /// - <see cref="UpdateCurrencyCommand.CurrencyId"/>: Identifier of the new currency.
    /// - <see cref="UpdateCurrencyCommand.ContactId"/>: Identifier of the contact whose currency will be updated.
    /// </param>
    /// <returns>
    /// An object of type <see cref="ActionResult"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// This method updates the currency of the contact in the database.
    /// If the contact is not found, an exception is thrown.
    /// 
    /// Example request:
    /// 
    ///     PUT /api/UpdateCurrency
    ///     {
    ///         "CurrencyId": 1,
    ///         "ContactId": 123
    ///     }
    /// 
    /// </remarks>
    /// <response code="204">If the currency was successfully updated.</response>
    /// <response code="404">If the specified contact cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response> 
    [HttpPut("UpdateCurrency")]
    public async Task<ActionResult> UpdateCurrency(UpdateCurrencyCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Updates the information of a specific lead contact.
    /// </summary>
    /// <param name="contactLeadId">
    /// Identifier of the lead contact to be updated.
    /// </param>
    /// <param name="command">
    /// Object that contains the parameters necessary for the update:
    /// - <see cref="UpdateContactLeadCommand.ContactLeadId"/>: Identifier of the lead contact.
    /// - Other fields from <see cref="ContactLeadUpdateDto"/> that can be updated.
    /// </param>
    /// <returns>
    /// An object of type <see cref="ActionResult"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// This method updates the information of the lead contact in the database.
    /// If the identifier of the lead contact does not match that of the command, a bad request error is returned.
    /// If the lead contact is not found, an error code is returned.
    /// 
    /// Example request:
    /// 
    ///     PUT /api/ContactLead/123
    ///     {
    ///         "ContactLeadId": 123,
    ///         "Discount": 10,
    ///         "FinalPrice": 200,
    ///         "EnrollmentPercentage": 50,
    ///         "Fees": 5,
    ///         "CourseTypeBaseCode": "CT001",
    ///         "StartDateCourse": "2023-01-01",
    ///         "FinishDateCourse": "2023-12-31",
    ///         "ConvocationDate": "2023-12-01",
    ///         "CourseCode": "C001",
    ///         "Types": [1, 2, 3]
    ///     }
    /// 
    /// </remarks>
    /// <response code="204">If the lead contact information was successfully updated.</response>
    /// <response code="400">If the identifier of the lead contact does not match that of the command.</response>
    /// <response code="404">If the specified lead contact cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response>  
    [HttpPut("ContactLead")]
    public async Task<ActionResult> UpdateContactLead(int contactLeadId, UpdateContactLeadCommand command)
    {
        if (contactLeadId != command.ContactLeadId)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Updates the information of multiple lead contacts.
    /// </summary>
    /// <param name="command">
    /// Object that contains the parameters necessary for the update:
    /// - <see cref="UpdateContactLeadsCommand.ContactLeads"/>: List of objects representing the lead contacts to be updated.
    /// </param>
    /// <returns>
    /// An object of type <see cref="ActionResult"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// This method updates the information of the lead contacts in the database.
    /// 
    /// Example request:
    /// 
    ///     PUT /api/ContactLeads
    ///     {
    ///         "ContactLeads": [
    ///             {
    ///                 "ContactLeadId": 1,
    ///                 "Discount": 10,
    ///                 "FinalPrice": 200,
    ///                 "EnrollmentPercentage": 50,
    ///                 "Fees": 5,
    ///                 "CourseTypeBaseCode": "CT001",
    ///                 "StartDateCourse": "2023-01-01",
    ///                 "FinishDateCourse": "2023-12-31",
    ///                 "ConvocationDate": "2023-12-01",
    ///                 "Types": [1, 2]
    ///             },
    ///             {
    ///                 "ContactLeadId": 2,
    ///                 "Discount": 15,
    ///                 "FinalPrice": 250,
    ///                 "EnrollmentPercentage": 60,
    ///                 "Fees": 10,
    ///                 "CourseTypeBaseCode": "CT002",
    ///                 "StartDateCourse": "2023-02-01",
    ///                 "FinishDateCourse": "2023-11-30",
    ///                 "ConvocationDate": "2023-11-15",
    ///                 "Types": [2, 3]
    ///             }
    ///         ]
    ///     }
    /// 
    /// </remarks>
    /// <response code="204">If the information of the lead contacts was successfully updated.</response>
    /// <response code="400">If there is an error in the request, such as missing or incorrect data.</response>
    /// <response code="404">If any of the specified lead contacts cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPut("ContactLeads")]
    public async Task<ActionResult> UpdateContactLeads(UpdateContactLeadsCommand command)
    {
        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Updates the price of a specific lead contact.
    /// </summary>
    /// <param name="contactLeadId">
    /// Identifier of the lead contact to be updated.
    /// </param>
    /// <param name="command">
    /// Object that contains the parameters necessary for the update:
    /// - <see cref="UpdateContactLeadPriceCommand.ContactLeadId"/>: Identifier of the lead contact.
    /// - Other fields from <see cref="ContactLeadPriceUpdateDto"/> that can be updated.
    /// </param>
    /// <returns>
    /// An object of type <see cref="ActionResult"/> indicating the result of the operation.
    /// </returns>
    /// <remarks>
    /// This method updates the price of the lead contact in the database.
    /// If the identifier of the lead contact does not match that of the command, a bad request error is returned.
    /// If the lead contact is not found, an error code is returned.
    /// 
    /// Example request:
    /// 
    ///     PUT /api/ContactLeadPrice/123
    ///     {
    ///         "ContactLeadId": 123,
    ///         "Discount": 10,
    ///         "Price": 150,
    ///         "FinalPrice": 140,
    ///         "EnrollmentPercentage": 50,
    ///         "Fees": 5
    ///     }
    /// 
    /// </remarks>
    /// <response code="204">If the price of the lead contact was successfully updated.</response>
    /// <response code="400">If the identifier of the lead contact does not match that of the command.</response>
    /// <response code="404">If the specified lead contact cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPut("ContactLeadPrice")]
    public async Task<ActionResult> UpdateContactLeadPrice(int contactLeadId, UpdateContactLeadPriceCommand command)
    {
        if (contactLeadId != command.ContactLeadId)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Obtains a specific contact using their email address or phone number.
    /// </summary>
    /// <param name="query">
    /// Object that contains the parameters necessary for the query:
    /// - <see cref="GetContactByEmailOrPhoneQuery.Data"/>: Object that includes the email address or phone number of the contact.
    /// </param>
    /// <returns>
    /// An object of type <see cref="ContactGetted"/> representing the information of the retrieved contact.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve a contact based on the provided email address or phone number.
    /// 
    /// Example request:
    /// 
    ///     GET /api/GetContactByEmailOrPhone?Email=example@example.com
    ///     or
    ///     GET /api/GetContactByEmailOrPhone?Phone=1234567890
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the contact information is returned.</response>
    /// <response code="404">If the specified contact cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response>   
    [HttpGet("GetContactByEmailOrPhone")]
    public async Task<ActionResult<ContactGetted>> GetContactByEmailOrPhone(
        [FromQuery] GetContactByEmailOrPhoneQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Checks if a specific contact is a client.
    /// </summary>
    /// <param name="query">
    /// Object that contains the parameters necessary for the query:
    /// - <see cref="GetIsContactClientQuery.ContactId"/>: Identifier of the contact.
    /// - <see cref="GetIsContactClientQuery.OriginContactId"/>: Identifier of the origin contact (optional).
    /// </param>
    /// <returns>
    /// A boolean value indicating whether the contact is a client.
    /// </returns>
    /// <remarks>
    /// This method queries the database to determine if the specified contact has imported orders that are neither canceled nor pending.
    /// 
    /// Example request:
    /// 
    ///     GET /api/GetContactIsClient?ContactId=123
    ///     or
    ///     GET /api/GetContactIsClient?OriginContactId=456
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the contact's client status is returned.</response>
    /// <response code="404">If the specified contact cannot be found.</response>
    /// <response code="500">If an internal server error occurs.</response>  
    [HttpGet("GetContactIsClient")]
    public async Task<ActionResult<bool>> GetContactIsClient([FromQuery] GetIsContactClientQuery query) =>
        await Mediator.Send(query);

    /// <summary>
    /// Obtains a list of contact users based on the email address or phone number.
    /// </summary>
    /// <param name="email">
    /// Email address of the contact (optional).
    /// </param>
    /// <param name="phone">
    /// Phone number of the contact (optional).
    /// </param>
    /// <returns>
    /// A list of objects of type <see cref="ContactUser Dto"/> representing the found contact users.
    /// </returns>
    /// <remarks>
    /// This method queries the database to retrieve contact users that match the provided email address or phone number.
    /// 
    /// Example request:
    /// 
    ///     GET /api/GetContactUser ?email=example@example.com
    ///     or
    ///     GET /api/GetContactUser ?phone=1234567890
    /// 
    /// </remarks>
    /// <response code="200">If the request is processed successfully and the contact users are returned.</response>
    /// <response code="404">If no contact users matching the specified criteria are found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("GetContactUser")]
    public async Task<ActionResult<List<ContactUserDto>>> GetContactUser(string? email, string? phone) =>
        await Mediator.Send(new GetUserContactQuery { Email = email, Phone = phone });

    /// <summary>
    /// Recupera y reactiva un proceso en el sistema, actualizando su fecha de creación
    /// y las acciones asociadas al mismo.
    /// </summary>
    /// <param name="query">El comando que contiene el ID del proceso a recuperar.</param>
    /// <returns>
    /// Retorna el ID del proceso si la operación es exitosa.
    /// Lanza una excepción si el ID del proceso no es válido o si ocurre un error interno.
    /// </returns>
    /// <response code="200">Operación exitosa. Retorna el ID del proceso actualizado.</response>
    /// <response code="400">Error de validación. El ID del proceso no es válido.</response>
    /// <response code="404">No se encontró un proceso con el ID proporcionado.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpPut("RecoverContactActivations")]
    public async Task<ActionResult<int>> RecoverContactActivations(RecoverContactActivationsCommand query) =>
        await Mediator.Send(query);

    [HttpPost("ConsolidateToOtherContact")]
    [Description("Consolidates Contact related data from origin to destination contact. Origin will be soft-deleted.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, null)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ValidationProblemDetails),
        Description = "Validation failures")]
    [SwaggerResponse(StatusCodes.Status404NotFound, typeof(ProblemDetails),
        Description = "Contacts cannot be found by provided emails")]
    [SwaggerResponse(StatusCodes.Status409Conflict, typeof(ProblemDetails),
        Description = "Contacts cannot refer to the same Contact ID.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, typeof(ProblemDetails),
        Description = "Among others, problem saving changes.")]
    public async Task<IActionResult> ConsolidateToOtherContact(ConsolidateContactsCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);

        return result.MatchFirst<IActionResult>(
            _ => NoContent(),
            ConvertToProblemResult);
    }

    [HttpPost("ConsolidateToOtherContactById")]
    [Description(
        "Consolidates Contact related data from origin to destination contact *by IDs*. Origin will be soft-deleted. " +
        "As of now uses ApiKey and not token security model.")]
    [SwaggerResponse(StatusCodes.Status204NoContent, null)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ValidationProblemDetails),
        Description = "Validation failures")]
    [SwaggerResponse(StatusCodes.Status404NotFound, typeof(ProblemDetails),
        Description = "Contacts cannot be found by provided IDs")]
    [SwaggerResponse(StatusCodes.Status409Conflict, typeof(ProblemDetails),
        Description = "Contacts cannot refer to the same Contact ID.")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, typeof(ProblemDetails),
        Description = "Among others, problem saving changes.")]
    public async Task<IActionResult> ConsolidateToOtherContactById(
        ConsolidateContactsByIdCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);

        return result.MatchFirst<IActionResult>(
            _ => NoContent(),
            ConvertToProblemResult);
    }

    private ObjectResult ConvertToProblemResult(Error error)
    {
        var problemDetail = ConvertToProblemDetails(error);

        return error.Type switch
        {
            ErrorType.NotFound => base.NotFound(problemDetail),
            ErrorType.Conflict => base.Conflict(problemDetail),
            ErrorType.Unexpected => base.StatusCode(StatusCodes.Status500InternalServerError, problemDetail),
            _ => base.StatusCode(StatusCodes.Status500InternalServerError, problemDetail),
        };
    }

    private static ProblemDetails ConvertToProblemDetails(Error error)
    {
        var problemDetails = new ProblemDetails
        {
            Detail = error.Description,
            Title = error.Type.ToString(),
        };

        if (error.Metadata is null)
        {
            return problemDetails;
        }

        foreach (var (key, value) in error.Metadata)
        {
            problemDetails.Extensions.Add(key, value);
        }

        return problemDetails;
    }
}
