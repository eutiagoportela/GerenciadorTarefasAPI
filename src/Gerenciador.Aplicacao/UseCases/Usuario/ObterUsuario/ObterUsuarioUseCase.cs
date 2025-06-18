using AutoMapper;
using Gerenciador.Comunicacao.Responses.Usuario;
using Gerenciador.Dominio.Repositorios.Interfaces;
using Gerenciador.Exceptions;

namespace Gerenciador.Aplicacao.UseCases.Usuario.ObterUsuario;

public class ObterUsuarioUseCase : IObterUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;

    public ObterUsuarioUseCase(
        IUsuarioRepository usuarioRepository,
        IMapper mapper)
    {
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
    }

    public async Task<UsuarioResponse> ExecuteAsync(int id)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(id);

        if (usuario == null)
            throw new UsuarioNaoEncontradoException();

        return _mapper.Map<UsuarioResponse>(usuario);
    }
}