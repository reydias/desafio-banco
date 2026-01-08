using Lancamentos.Domain.Events;

namespace Lancamentos.Domain.Interfaces;

public interface IEventPublisher
{
    Task PublicarAsync<T>(T evento, CancellationToken cancellationToken = default) where T : class;
}

