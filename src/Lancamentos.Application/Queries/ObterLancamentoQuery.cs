using Lancamentos.Domain.Entities;

namespace Lancamentos.Application.Queries;

public class ObterLancamentoQuery
{
    public Guid UsuarioId { get; set; }
    public Guid? Id { get; set; }
    public DateTime? Data { get; set; }
    public TipoLancamento? Tipo { get; set; }
}

