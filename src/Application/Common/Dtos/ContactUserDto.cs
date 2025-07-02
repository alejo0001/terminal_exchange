namespace CrmAPI.Application.Common.Dtos;

public record ContactUserDto(
    int Id,
    string Name,
    string Surname,
    string Email
);