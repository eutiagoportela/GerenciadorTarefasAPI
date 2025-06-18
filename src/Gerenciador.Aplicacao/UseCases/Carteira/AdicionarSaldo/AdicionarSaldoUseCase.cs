using AutoMapper;
using Gerenciador.Comunicacao.Requests.Carteira;
using Gerenciador.Comunicacao.Responses.Carteira;
using Gerenciador.Dominio.Entidades;
using Gerenciador.Dominio.Enum;
using Gerenciador.Dominio.Repositorios.Interfaces;
using Gerenciador.Exceptions;

namespace Gerenciador.Aplicacao.UseCases.Carteira.AdicionarSaldo;

public class AdicionarSaldoUseCase : IAdicionarSaldoUseCase
{
    private readonly ICarteiraRepository _carteiraRepository;
    private readonly ITransferenciaRepository _transferenciaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;

    public AdicionarSaldoUseCase(
        ICarteiraRepository carteiraRepository,
        ITransferenciaRepository transferenciaRepository,
        IUsuarioRepository usuarioRepository,
        IMapper mapper)
    {
        _carteiraRepository = carteiraRepository;
        _transferenciaRepository = transferenciaRepository;
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
    }

    public async Task<CarteiraResponse> ExecuteAsync(int usuarioId, AdicionarSaldoRequest request)
    {
        // Validar usuário
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario == null)
            throw new UsuarioNaoEncontradoException();

        // Obter carteira
        var carteira = await _carteiraRepository.ObterPorUsuarioIdAsync(usuarioId);
        if (carteira == null)
            throw new CarteiraException("Carteira não encontrada para o usuário informado");

        // Adicionar saldo
        carteira.AdicionarSaldo(request.Valor);
        var carteiraAtualizada = await _carteiraRepository.AtualizarAsync(carteira);

        // Registrar transferência como depósito
        var transferencia = new Transferencias
        {
            Valor = request.Valor,
            Descricao = request.Descricao ?? "Depósito",
            Tipo = TipoTransferencia.Deposito,
            RemetenteId = usuarioId,
            DestinatarioId = usuarioId,
            DataTransferencia = DateTime.UtcNow
        };

        await _transferenciaRepository.CriarAsync(transferencia);

        return _mapper.Map<CarteiraResponse>(carteiraAtualizada);
    }
}