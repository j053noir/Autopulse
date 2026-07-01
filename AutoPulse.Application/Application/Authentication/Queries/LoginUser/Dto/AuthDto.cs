namespace AutoPulse.Application.Application.Authentication.Queries.LoginUser.Dto
{
    public record AuthDto(
        string AcessToken, 
        double ExpiresIn
    );
}
