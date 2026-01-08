using Lancamentos.Domain.Entities;

namespace Lancamentos.Application.Commands;

public class CriarLancamentoCommand
{
    public Guid UsuarioId { get; set; }
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
    public TipoLancamento Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
}

