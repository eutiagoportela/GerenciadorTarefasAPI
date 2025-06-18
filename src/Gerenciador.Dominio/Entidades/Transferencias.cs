using System.ComponentModel.DataAnnotations;
using Gerenciador.Dominio.Enum;

namespace Gerenciador.Dominio.Entidades;

public class Transferencias
{
    public int Id { get; set; }

    [Required]
    public decimal Valor { get; set; }

    public string? Descricao { get; set; }

    [Required]
    public TipoTransferencia Tipo { get; set; }

    public DateTime DataTransferencia { get; set; } = DateTime.UtcNow;

    // Relacionamentos
    [Required]
    public int RemetenteId { get; set; }
    public Usuarios Remetente { get; set; } = null!;

    [Required]
    public int DestinatarioId { get; set; }
    public Usuarios Destinatario { get; set; } = null!;

    // Validação de transferência
    public void ValidarTransferencia()
    {
        if (Valor <= 0)
            throw new ArgumentException("O valor da transferência deve ser maior que zero");

        if (RemetenteId == DestinatarioId)
            throw new ArgumentException("O remetente e o destinatário não podem ser o mesmo usuário");
    }
}