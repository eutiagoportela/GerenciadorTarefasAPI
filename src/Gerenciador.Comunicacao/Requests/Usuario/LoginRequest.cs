using System.ComponentModel.DataAnnotations;

namespace Gerenciador.Comunicacao.Requests.Usuario;

public class LoginRequest
{
    [Required(ErrorMessage = "O e-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "O e-mail fornecido não é válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória")]
    public string Senha { get; set; } = string.Empty;
}