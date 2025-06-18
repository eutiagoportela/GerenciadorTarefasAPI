using System.ComponentModel.DataAnnotations;

namespace Gerenciador.Dominio.Entidades;

public class Carteiras
{
    public int Id { get; set; }

    [Required]
    public decimal Saldo { get; set; } = 0;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    // Relacionamentos
    [Required]
    public int UsuarioId { get; set; }
    public Usuarios Usuario { get; set; } = null!;

    // Métodos de negócio
    public void AdicionarSaldo(decimal valor)
    {
        if (valor <= 0)
            throw new ArgumentException("O valor deve ser maior que zero");

        Saldo += valor;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void DebitarSaldo(decimal valor)
    {
        if (valor <= 0)
            throw new ArgumentException("O valor deve ser maior que zero");

        if (Saldo < valor)
            throw new InvalidOperationException("Saldo insuficiente para esta operação");

        Saldo -= valor;
        DataAtualizacao = DateTime.UtcNow;
    }
}