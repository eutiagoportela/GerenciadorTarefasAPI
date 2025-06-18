using System.ComponentModel.DataAnnotations;

namespace Gerenciador.Comunicacao.Requests.Transferencia;

public class CriarTransferenciaRequest
{
    [Required(ErrorMessage = "O ID do destinatário é obrigatório")]
    public int DestinatarioId { get; set; }

    [Required(ErrorMessage = "O valor é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
    public decimal Valor { get; set; }

    [StringLength(200, ErrorMessage = "A descrição deve ter no máximo 200 caracteres")]
    public string? Descricao { get; set; }
}