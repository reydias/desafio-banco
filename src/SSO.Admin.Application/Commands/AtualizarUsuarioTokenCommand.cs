namespace SSO.Admin.Application.Commands;

public class AtualizarUsuarioTokenCommand
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}



