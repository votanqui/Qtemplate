using Qtemplate.Domain.Entities;

namespace Qtemplate.Application.Services.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    DateTime GetAccessTokenExpiry();
}