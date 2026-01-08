namespace SSO.Admin.Domain.Interfaces;

public interface IAuthService
{
    Task<string> GerarTokenAsync(string login, string senha, CancellationToken cancellationToken = default);
}



