namespace AutoPulse.Application.Application.Authentication.Common.Dto
{
    public record AuthDto(
        string AccessToken, 
        string RefreshToken,
        double ExpiresIn
    );
}
