namespace ConsolidadoDiario.Application.DTOs;

public class ConsolidadoDiarioDTO
{
    public DateTime Data { get; set; }
    public decimal TotalCreditos { get; set; }
    public decimal TotalDebitos { get; set; }
    public decimal SaldoDiario { get; set; }
    public int QuantidadeLancamentos { get; set; }
}

