using Gerenciador.Dominio.Entidades;

namespace Gerenciador.Infraestrutura.Security;

public interface IJwtTokenGenerator
{
    string GenerateToken(Usuarios usuario);
}
