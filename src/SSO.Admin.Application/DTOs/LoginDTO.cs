using System.ComponentModel.DataAnnotations;

namespace SSO.Admin.Application.DTOs;

public class LoginDTO
{
    [Required(ErrorMessage = "Login é obrigatório")]
    [StringLength(100, ErrorMessage = "Login deve ter no máximo 100 caracteres")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
    public string Senha { get; set; } = string.Empty;
}



