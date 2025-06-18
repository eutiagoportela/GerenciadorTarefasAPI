using Gerenciador.Dominio.Entidades;

namespace Gerenciador.Dominio.Repositorios.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuarios?> ObterPorIdAsync(int id);
    Task<Usuarios?> ObterPorEmailAsync(string email);
    Task<Usuarios> CriarAsync(Usuarios usuario);
    Task<List<Usuarios>> ListarTodosAsync();
    Task<bool> EmailExisteAsync(string email);
}