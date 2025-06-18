
using AutoMapper;
using Gerenciador.Comunicacao.Responses.Transferencia;
using Gerenciador.Dominio.Entidades;
using Gerenciador.Dominio.Repositorios.Interfaces;
using Gerenciador.Exceptions;

namespace Gerenciador.Aplicacao.UseCases.Transferencia.ListarTransferencias;

public class ListarTransferenciasUseCase : IListarTransferenciasUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITransferenciaRepository _transferenciaRepository;
    private readonly IMapper _mapper;

    public ListarTransferenciasUseCase(
        IUsuarioRepository usuarioRepository,
        ITransferenciaRepository transferenciaRepository,
        IMapper mapper)
    {
        _usuarioRepository = usuarioRepository;
        _transferenciaRepository = transferenciaRepository;
        _mapper = mapper;
    }

    public async Task<List<TransferenciaResponse>> ExecuteAsync(int usuarioId)
    {
        // Verificar se o usuário existe
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario == null)
            throw new UsuarioNaoEncontradoException();


        var transferencias = await _transferenciaRepository.ListarPorUsuarioAsync(usuarioId);

        return MapearTransferencias(transferencias);
    }

    public async Task<List<TransferenciaResponse>> ExecuteAsync(int usuarioId, DateTime dataInicio, DateTime dataFim)
    {
        // Verificar se o usuário existe
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario == null)
            throw new UsuarioNaoEncontradoException();

        // Validar datas
        if (dataInicio > dataFim)
            throw new ArgumentException("A data inicial não pode ser maior que a data final");

        // Ajustar dataFim para incluir todo o dia
        dataFim = dataFim.Date.AddDays(1).AddTicks(-1);

        var transferencias = await _transferenciaRepository.ListarPorUsuarioEPeriodoAsync(usuarioId, dataInicio, dataFim);

        return MapearTransferencias(transferencias);
    }


    public async Task<(List<TransferenciaResponse>, int)> ExecutePaginadoAsync(
        int usuarioId,
        int pagina = 1,
        int tamanhoPagina = 10)
    {
        // Verificar se o usuário existe
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario == null)
            throw new UsuarioNaoEncontradoException();

        // Validar parâmetros de paginação
        if (pagina < 1) pagina = 1;
        if (tamanhoPagina < 1 || tamanhoPagina > 100) tamanhoPagina = 10;

        // Obter transferências paginadas
        var (transferencias, totalCount) = await _transferenciaRepository.ListarPorUsuarioPaginadoAsync(
            usuarioId, pagina, tamanhoPagina);

        var transferenciasResponse = MapearTransferencias(transferencias);

        return (transferenciasResponse, totalCount);
    }

    private List<TransferenciaResponse> MapearTransferencias(List<Transferencias> transferencias)
    {
        var respostas = new List<TransferenciaResponse>();

        foreach (var transferencia in transferencias)
        {
            var resposta = _mapper.Map<TransferenciaResponse>(transferencia);

            resposta.RemetenteNome = transferencia.Remetente?.Nome ?? "Usuário não encontrado";
            resposta.DestinatarioNome = transferencia.Destinatario?.Nome ?? "Usuário não encontrado";

            respostas.Add(resposta);
        }

        return respostas;
    }
}