using System.ComponentModel.DataAnnotations;

namespace Gerenciador.Comunicacao.Requests.Carteira;

public class AdicionarSaldoRequest
{
    [Required(ErrorMessage = "O valor é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
    public decimal Valor { get; set; }

    public string? Descricao { get; set; }
}