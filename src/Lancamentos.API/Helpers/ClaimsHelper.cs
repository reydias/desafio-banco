using System.Security.Claims;

namespace Lancamentos.API.Helpers;

public static class ClaimsHelper
{
    public static Guid? ObterUsuarioId(ClaimsPrincipal user)
    {
        var nameIdentifier = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(nameIdentifier) || !Guid.TryParse(nameIdentifier, out var usuarioId))
        {
            return null;
        }
        return usuarioId;
    }

    public static Guid ObterUsuarioIdOuLancarExcecao(ClaimsPrincipal user)
    {
        var usuarioId = ObterUsuarioId(user);
        if (usuarioId == null)
        {
            throw new UnauthorizedAccessException("Usuário não identificado no token.");
        }
        return usuarioId.Value;
    }
}


