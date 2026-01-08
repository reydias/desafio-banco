namespace SSO.Admin.Application.Commands;

public class AtualizarSenhaCommand
{
    public Guid Id { get; set; }
    public string SenhaAtual { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}



