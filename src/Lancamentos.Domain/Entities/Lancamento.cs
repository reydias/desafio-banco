namespace Lancamentos.Domain.Entities;

public class Lancamento
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public DateTime Data { get; private set; }
    public decimal Valor { get; private set; }
    public TipoLancamento Tipo { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public DateTime DataCriacao { get; private set; }

    private Lancamento() { } // Para EF Core

    public Lancamento(Guid usuarioId, DateTime data, decimal valor, TipoLancamento tipo, string descricao)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        Data = data;
        Valor = valor;
        Tipo = tipo;
        Descricao = descricao ?? throw new ArgumentNullException(nameof(descricao));
        DataCriacao = DateTime.UtcNow;
    }

    public void Atualizar(decimal valor, TipoLancamento tipo, string descricao)
    {
        Valor = valor;
        Tipo = tipo;
        Descricao = descricao ?? throw new ArgumentNullException(nameof(descricao));
    }
}

public enum TipoLancamento
{
    Credito = 'C',
    Debito = 'D'
}

