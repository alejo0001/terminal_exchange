using System.Collections.Generic;
using System.Threading.Tasks;
using CrmAPI.Application.Annotations.Commands.CreateAnnotation;
using CrmAPI.Application.Annotations.Commands.DeleteAnnotation;
using CrmAPI.Application.Annotations.Commands.UpdateAnnotation;
using CrmAPI.Application.Annotations.Queries.GetAnnotationsByContact;
using CrmAPI.Application.Common.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CrmAPI.Presentation.Controllers;

public class AnnotationsController : ApiControllerBase
{
    /// <summary>
    /// Retrieves a list of annotations associated with a specific contact.
    /// </summary>
    /// <param name="query">
    /// Parameters needed to retrieve the contact's annotations:
    /// - <see cref="GetAnnotationsByContactQuery.ContactId"/>: Unique identifier of the contact.
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="List{AnnotationDto}"/>: List of annotations associated with the specified contact.
    /// </returns>
    [HttpGet("Contact/List")]
    public async Task<ActionResult<List<AnnotationDto>>> GetAnnotationsByContact(
        [FromQuery] GetAnnotationsByContactQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Creates a new annotation.
    /// </summary>
    /// <param name="command">
    /// Object containing the necessary data to create an annotation:
    /// - <see cref="CreateAnnotationCommand"/>: DTO with the details of the annotation to be created.
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="int"/>: Identifier of the created annotation.
    /// </returns>
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateAnnotationCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Updates an existing annotation.
    /// </summary>
    /// <param name="id">
    /// Unique identifier of the annotation to be updated.
    /// </param>
    /// <param name="command">
    /// Object containing the updated data of the annotation:
    /// - <see cref="UpdateAnnotationCommand.Id"/>: Must match the `id` parameter.
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="NoContent"/> if the operation completes successfully.
    /// - <see cref="BadRequest"/> if the `id` does not match the `Id` field in the command.
    /// </returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateAnnotationCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Deletes an annotation.
    /// </summary>
    /// <param name="id">
    /// Unique identifier of the annotation to be deleted.
    /// </param>
    /// <returns>
    /// An HTTP response with:
    /// - <see cref="NoContent"/> if the operation completes successfully.
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteAnnotationCommand { Id = id });

        return NoContent();
    }
}