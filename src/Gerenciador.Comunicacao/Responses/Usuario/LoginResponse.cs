namespace Gerenciador.Comunicacao.Responses.Usuario;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiracaoToken { get; set; }
    public UsuarioResponse Usuario { get; set; } = null!;
}