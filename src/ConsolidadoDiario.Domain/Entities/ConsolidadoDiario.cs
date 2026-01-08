namespace ConsolidadoDiario.Domain.Entities;

public class ConsolidadoDiario
{
    public Guid Id { get; internal set; }
    public Guid UsuarioId { get; internal set; }
    public DateTime Data { get; internal set; }
    public decimal TotalCreditos { get; internal set; }
    public decimal TotalDebitos { get; internal set; }
    public decimal SaldoDiario { get; internal set; }
    public int QuantidadeLancamentos { get; internal set; }
    public DateTime DataAtualizacao { get; internal set; }

    private ConsolidadoDiario() { } // Para EF Core

    public ConsolidadoDiario(Guid usuarioId, DateTime data)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        Data = data.Date;
        TotalCreditos = 0;
        TotalDebitos = 0;
        SaldoDiario = 0;
        QuantidadeLancamentos = 0;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void AdicionarCredito(decimal valor)
    {
        TotalCreditos += valor;
        QuantidadeLancamentos++;
        RecalcularSaldo();
    }

    public void AdicionarDebito(decimal valor)
    {
        TotalDebitos += valor;
        QuantidadeLancamentos++;
        RecalcularSaldo();
    }

    internal void RecalcularSaldo()
    {
        SaldoDiario = TotalCreditos - TotalDebitos;
        DataAtualizacao = DateTime.UtcNow;
    }

    internal void DefinirValores(decimal totalCreditos, decimal totalDebitos, int quantidadeLancamentos)
    {
        TotalCreditos = totalCreditos;
        TotalDebitos = totalDebitos;
        QuantidadeLancamentos = quantidadeLancamentos;
        RecalcularSaldo();
    }

    public static ConsolidadoDiario Criar(Guid usuarioId, DateTime data, decimal totalCreditos, decimal totalDebitos, int quantidadeLancamentos)
    {
        var consolidado = new ConsolidadoDiario(usuarioId, data);
        consolidado.DefinirValores(totalCreditos, totalDebitos, quantidadeLancamentos);
        return consolidado;
    }
}

