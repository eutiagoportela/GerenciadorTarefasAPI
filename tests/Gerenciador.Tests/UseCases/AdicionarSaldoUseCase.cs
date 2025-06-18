
using AutoMapper;
using Gerenciador.Aplicacao.UseCases.Carteira.AdicionarSaldo;
using Gerenciador.Comunicacao.Requests.Carteira;
using Gerenciador.Comunicacao.Responses.Carteira;
using Gerenciador.Dominio.Entidades;
using Gerenciador.Dominio.Repositorios.Interfaces;
using Gerenciador.Exceptions;
using Moq;
using Xunit;

public class AdicionarSaldoUseCaseTests
{
    private readonly Mock<ICarteiraRepository> _carteiraRepoMock = new();
    private readonly Mock<ITransferenciaRepository> _transferenciaRepoMock = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private AdicionarSaldoUseCase CriarUseCase()
    {
        return new AdicionarSaldoUseCase(
            _carteiraRepoMock.Object,
            _transferenciaRepoMock.Object,
            _usuarioRepoMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeveAdicionarSaldoComSucesso()
    {
        // Arrange
        int usuarioId = 1;
        var request = new AdicionarSaldoRequest
        {
            Valor = 100m,
            Descricao = "Depósito teste"
        };

        var usuario = new Usuarios { Id = usuarioId, Nome = "Tiago" };
        var carteira = new Carteiras
        {
            Id = 1,
            UsuarioId = usuarioId,
            Saldo = 200m,
            DataCriacao = DateTime.UtcNow.AddDays(-1),
            DataAtualizacao = DateTime.UtcNow.AddDays(-1)
        };

        // Mock para obter usuário
        _usuarioRepoMock.Setup(r => r.ObterPorIdAsync(usuarioId))
            .ReturnsAsync(usuario);

        // Mock para obter carteira
        _carteiraRepoMock.Setup(r => r.ObterPorUsuarioIdAsync(usuarioId))
            .ReturnsAsync(carteira);

        // Mock para atualizar carteira (simula saldo atualizado)
        _carteiraRepoMock.Setup(r => r.AtualizarAsync(It.IsAny<Carteiras>()))
            .ReturnsAsync((Carteiras c) => c);

        // Mock para criação da transferência
        _transferenciaRepoMock.Setup(r => r.CriarAsync(It.IsAny<Transferencias>()))
            .ReturnsAsync(new Transferencias());

        // Mock para mapeamento
        var carteiraResponse = new CarteiraResponse
        {
            Saldo = carteira.Saldo + request.Valor
        };
        _mapperMock.Setup(m => m.Map<CarteiraResponse>(It.IsAny<Carteiras>()))
            .Returns(carteiraResponse);

        var useCase = CriarUseCase();

        // Act
        var resultado = await useCase.ExecuteAsync(usuarioId, request);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(300m, resultado.Saldo); // 200 + 100
        _usuarioRepoMock.Verify(r => r.ObterPorIdAsync(usuarioId), Times.Once);
        _carteiraRepoMock.Verify(r => r.ObterPorUsuarioIdAsync(usuarioId), Times.Once);
        _carteiraRepoMock.Verify(r => r.AtualizarAsync(It.IsAny<Carteiras>()), Times.Once);
        _transferenciaRepoMock.Verify(r => r.CriarAsync(It.IsAny<Transferencias>()), Times.Once);
        _mapperMock.Verify(m => m.Map<CarteiraResponse>(It.IsAny<Carteiras>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveLancarUsuarioNaoEncontradoException_SeUsuarioNaoExistir()
    {
        // Arrange
        int usuarioId = 1;
        var request = new AdicionarSaldoRequest { Valor = 100m };

        _usuarioRepoMock.Setup(r => r.ObterPorIdAsync(usuarioId))
            .ReturnsAsync((Usuarios?)null);

        var useCase = CriarUseCase();

        // Act & Assert
        await Assert.ThrowsAsync<UsuarioNaoEncontradoException>(() => useCase.ExecuteAsync(usuarioId, request));
    }

    [Fact]
    public async Task ExecuteAsync_DeveLancarCarteiraException_SeCarteiraNaoExistir()
    {
        // Arrange
        int usuarioId = 1;
        var request = new AdicionarSaldoRequest { Valor = 100m };

        var usuario = new Usuarios { Id = usuarioId };
        _usuarioRepoMock.Setup(r => r.ObterPorIdAsync(usuarioId))
            .ReturnsAsync(usuario);

        _carteiraRepoMock.Setup(r => r.ObterPorUsuarioIdAsync(usuarioId))
            .ReturnsAsync((Carteiras?)null);

        var useCase = CriarUseCase();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<CarteiraException>(() => useCase.ExecuteAsync(usuarioId, request));
        Assert.Equal("Carteira não encontrada para o usuário informado", ex.Message);
    }
}
