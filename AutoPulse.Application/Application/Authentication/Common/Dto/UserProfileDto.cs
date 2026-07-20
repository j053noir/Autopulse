namespace AutoPulse.Application.Application.Authentication.Common.Dto
{
    public record UserProfileDto(
        Guid Id,
        string UserName,
        string Email,
        bool IsActive,
        HashSet<string>? Permissions
    );
}
