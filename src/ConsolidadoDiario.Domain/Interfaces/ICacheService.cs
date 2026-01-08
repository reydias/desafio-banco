namespace ConsolidadoDiario.Domain.Interfaces;

public interface ICacheService
{
    Task<T?> ObterAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task DefinirAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoverAsync(string key, CancellationToken cancellationToken = default);
}

