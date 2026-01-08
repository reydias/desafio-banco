namespace SSO.Admin.Domain.Entities;

public class UsuarioToken
{
    public Guid Id { get; private set; }
    public string Login { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    private UsuarioToken() { } // Para EF Core

    public UsuarioToken(string login, string senhaHash, string nome, string email)
    {
        Id = Guid.NewGuid();
        Login = login ?? throw new ArgumentNullException(nameof(login));
        SenhaHash = senhaHash ?? throw new ArgumentNullException(nameof(senhaHash));
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Ativo = true;
        DataCriacao = DateTime.UtcNow;
    }

    public void Atualizar(string nome, string email)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        DataAtualizacao = DateTime.UtcNow;
    }

    public void AtualizarSenha(string senhaHash)
    {
        SenhaHash = senhaHash ?? throw new ArgumentNullException(nameof(senhaHash));
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Ativar()
    {
        Ativo = true;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Desativar()
    {
        Ativo = false;
        DataAtualizacao = DateTime.UtcNow;
    }
}



