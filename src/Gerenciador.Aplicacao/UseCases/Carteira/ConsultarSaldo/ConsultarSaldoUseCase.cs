using AutoMapper;
using Gerenciador.Comunicacao.Responses.Carteira;
using Gerenciador.Dominio.Repositorios.Interfaces;
using Gerenciador.Exceptions;

namespace Gerenciador.Aplicacao.UseCases.Carteira.ConsultarSaldo;

public class ConsultarSaldoUseCase : IConsultarSaldoUseCase
{
    private readonly ICarteiraRepository _carteiraRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;

    public ConsultarSaldoUseCase(
        ICarteiraRepository carteiraRepository,
        IUsuarioRepository usuarioRepository,
        IMapper mapper)
    {
        _carteiraRepository = carteiraRepository;
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
    }

    public async Task<CarteiraResponse> ExecuteAsync(int usuarioId)
    {
        // Verificar se o usuário existe
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario == null)
            throw new UsuarioNaoEncontradoException();

        // Obter a carteira do usuário
        var carteira = await _carteiraRepository.ObterPorUsuarioIdAsync(usuarioId);
        if (carteira == null)
            throw new CarteiraException("Carteira não encontrada para o usuário informado");

        return _mapper.Map<CarteiraResponse>(carteira);
    }
}