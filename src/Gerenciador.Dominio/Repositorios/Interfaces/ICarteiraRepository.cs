using Gerenciador.Dominio.Entidades;

namespace Gerenciador.Dominio.Repositorios.Interfaces;

public interface ICarteiraRepository
{
    Task<Carteiras?> ObterPorIdAsync(int id);
    Task<Carteiras?> ObterPorUsuarioIdAsync(int usuarioId);
    Task<Carteiras> CriarAsync(Carteiras carteira);
    Task<Carteiras> AtualizarAsync(Carteiras carteira);
}