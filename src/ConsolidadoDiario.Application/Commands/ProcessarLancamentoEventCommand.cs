namespace ConsolidadoDiario.Application.Commands;

public class ProcessarLancamentoEventCommand
{
    public Guid LancamentoId { get; set; }
    public Guid UsuarioId { get; set; }
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
    public string Tipo { get; set; } = string.Empty; // "C" = Credito, "D" = Debito
    public DateTime DataCriacao { get; set; }
}

