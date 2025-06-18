using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Gerenciador.Comunicacao.Requests.Transferencia;
using Gerenciador.Comunicacao.Responses.Transferencia;
using Gerenciador.Dominio.Entidades;
using Gerenciador.Dominio.Enum;
using Gerenciador.Dominio.Repositorios.Interfaces;
using Gerenciador.Exceptions;
using Gerenciador.Infraestrutura;

namespace Gerenciador.Aplicacao.UseCases.Transferencia.CriarTransferencia;

public class CriarTransferenciaUseCase : ICriarTransferenciaUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ICarteiraRepository _carteiraRepository;
    private readonly ITransferenciaRepository _transferenciaRepository;
    private readonly PostgreSqlDbContext _context; 
    private readonly IMapper _mapper;

    public CriarTransferenciaUseCase(
        IUsuarioRepository usuarioRepository,
        ICarteiraRepository carteiraRepository,
        ITransferenciaRepository transferenciaRepository,
        PostgreSqlDbContext context,
        IMapper mapper)
    {
        _usuarioRepository = usuarioRepository;
        _carteiraRepository = carteiraRepository;
        _transferenciaRepository = transferenciaRepository;
        _context = context;
        _mapper = mapper;
    }

    public async Task<TransferenciaResponse> ExecuteAsync(int remetenteId, CriarTransferenciaRequest request)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validar remetente
                var remetente = await _usuarioRepository.ObterPorIdAsync(remetenteId);
                if (remetente == null)
                    throw new UsuarioNaoEncontradoException("Remetente não encontrado");

                // Validar destinatário
                var destinatario = await _usuarioRepository.ObterPorIdAsync(request.DestinatarioId);
                if (destinatario == null)
                    throw new UsuarioNaoEncontradoException("Destinatário não encontrado");

                // Validar que remetente e destinatário são diferentes
                if (remetenteId == request.DestinatarioId)
                    throw new ArgumentException("Não é possível transferir para si mesmo");

                // Obter carteira do remetente com lock para evitar concorrência
                var carteiraRemetente = await _context.Carteiras
                    .Where(c => c.UsuarioId == remetenteId)
                    .FirstOrDefaultAsync();

                if (carteiraRemetente == null)
                    throw new CarteiraException("Carteira do remetente não encontrada");

                // Obter carteira do destinatário
                var carteiraDestinatario = await _context.Carteiras
                    .Where(c => c.UsuarioId == request.DestinatarioId)
                    .FirstOrDefaultAsync();

                if (carteiraDestinatario == null)
                    throw new CarteiraException("Carteira do destinatário não encontrada");

                // Verificar saldo suficiente
                if (carteiraRemetente.Saldo < request.Valor)
                    throw new SaldoInsuficienteException();

                // 1. Debitar da carteira do remetente
                carteiraRemetente.DebitarSaldo(request.Valor);
                _context.Carteiras.Update(carteiraRemetente);

                // 2. Creditar na carteira do destinatário
                carteiraDestinatario.AdicionarSaldo(request.Valor);
                _context.Carteiras.Update(carteiraDestinatario);

                // 3. Registrar a transferência
                var transferencia = new Transferencias
                {
                    Valor = request.Valor,
                    Descricao = request.Descricao,
                    Tipo = TipoTransferencia.Transferencia,
                    RemetenteId = remetenteId,
                    DestinatarioId = request.DestinatarioId,
                    DataTransferencia = DateTime.UtcNow
                };

                _context.Transferencias.Add(transferencia);

                // SALVAR TODAS AS MUDANÇAS EM UMA ÚNICA OPERAÇÃO
                await _context.SaveChangesAsync();

                // COMMIT DA TRANSAÇÃO
                await transaction.CommitAsync();

                var response = _mapper.Map<TransferenciaResponse>(transferencia);
                response.RemetenteNome = remetente.Nome;
                response.DestinatarioNome = destinatario.Nome;

                return response;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}
