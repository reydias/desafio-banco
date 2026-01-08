using Lancamentos.Domain.Entities;

namespace Lancamentos.Domain.Events;

public class LancamentoCriadoEvent
{
    public Guid LancamentoId { get; set; }
    public Guid UsuarioId { get; set; }
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
    public TipoLancamento Tipo { get; set; }
    public DateTime DataCriacao { get; set; }

    public LancamentoCriadoEvent(Guid lancamentoId, Guid usuarioId, DateTime data, decimal valor, TipoLancamento tipo, DateTime dataCriacao)
    {
        LancamentoId = lancamentoId;
        UsuarioId = usuarioId;
        Data = data;
        Valor = valor;
        Tipo = tipo;
        DataCriacao = dataCriacao;
    }
}
