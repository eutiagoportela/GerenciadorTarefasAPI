using System.ComponentModel.DataAnnotations;

namespace Gerenciador.Dominio.Entidades;

public class Usuarios
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string SenhaHash { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    // Relacionamentos
    public Carteiras? Carteira { get; set; }
    public ICollection<Transferencias> TransferenciasEnviadas { get; set; } = new List<Transferencias>();
    public ICollection<Transferencias> TransferenciasRecebidas { get; set; } = new List<Transferencias>();
}