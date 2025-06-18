using System.Threading.Tasks;
using AutoMapper;
using Gerenciador.Aplicacao.UseCases.Login.DoLogin;
using Gerenciador.Comunicacao.Requests.Usuario;
using Gerenciador.Comunicacao.Responses.Usuario;
using Gerenciador.Dominio.Entidades;
using Gerenciador.Dominio.Repositorios.Interfaces;
using Gerenciador.Exceptions;
using Gerenciador.Infraestrutura.Security;
using Moq;
using Xunit;

public class DoLoginUseCaseTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private DoLoginUseCase CriarUseCase()
    {
        return new DoLoginUseCase(
            _usuarioRepoMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarLoginResponse_SeCredenciaisValidas()
    {
        // Arrange
        var request = new LoginRequest { Email = "tiago@test.com", Senha = "Senha123!" };
        var usuario = new Usuarios { Id = 1, Nome = "Tiago", Email = request.Email, SenhaHash = "hashed" };

        _usuarioRepoMock.Setup(r => r.ObterPorEmailAsync(request.Email))
            .ReturnsAsync(usuario);

        _passwordHasherMock.Setup(h => h.VerifyPassword(request.Senha, usuario.SenhaHash))
            .Returns(true);

        _jwtTokenGeneratorMock.Setup(j => j.GenerateToken(usuario))
            .Returns("token_fake");

        var usuarioResponse = new UsuarioResponse { Id = 1, Nome = "Tiago", Email = request.Email };
        _mapperMock.Setup(m => m.Map<UsuarioResponse>(usuario))
            .Returns(usuarioResponse);

        var useCase = CriarUseCase();

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("token_fake", result.Token);
        Assert.Equal(usuarioResponse, result.Usuario);
    }

    [Fact]
    public async Task ExecuteAsync_DeveLancarUsuarioNaoEncontradoException_SeUsuarioNaoExistir()
    {
        // Arrange
        var request = new LoginRequest { Email = "naoexiste@test.com", Senha = "Senha123!" };

        _usuarioRepoMock.Setup(r => r.ObterPorEmailAsync(request.Email))
            .ReturnsAsync((Usuarios?)null);

        var useCase = CriarUseCase();

        // Act & Assert
        await Assert.ThrowsAsync<UsuarioNaoEncontradoException>(() => useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_DeveLancarUsuarioNaoEncontradoException_SeSenhaInvalida()
    {
        // Arrange
        var request = new LoginRequest { Email = "tiago@test.com", Senha = "SenhaIncorreta" };
        var usuario = new Usuarios { Id = 1, Nome = "Tiago", Email = request.Email, SenhaHash = "hashed" };

        _usuarioRepoMock.Setup(r => r.ObterPorEmailAsync(request.Email))
            .ReturnsAsync(usuario);

        _passwordHasherMock.Setup(h => h.VerifyPassword(request.Senha, usuario.SenhaHash))
            .Returns(false);

        var useCase = CriarUseCase();

        // Act & Assert
        await Assert.ThrowsAsync<UsuarioNaoEncontradoException>(() => useCase.ExecuteAsync(request));
    }
}
