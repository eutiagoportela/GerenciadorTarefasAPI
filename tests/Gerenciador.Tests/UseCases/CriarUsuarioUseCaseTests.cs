
using AutoMapper;
using Gerenciador.Aplicacao.UseCases.Usuario.CriarUsuario;
using Gerenciador.Comunicacao.Requests.Usuario;
using Gerenciador.Comunicacao.Responses.Usuario;
using Gerenciador.Dominio.Entidades;
using Gerenciador.Dominio.Repositorios.Interfaces;
using Gerenciador.Exceptions;
using Gerenciador.Infraestrutura.Security;
using Moq;
using Xunit;

public class CriarUsuarioUseCaseTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock = new();
    private readonly Mock<ICarteiraRepository> _carteiraRepoMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private CriarUsuarioUseCase CriarUseCase()
    {
        return new CriarUsuarioUseCase(
            _usuarioRepoMock.Object,
            _carteiraRepoMock.Object,
            _passwordHasherMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeveCriarUsuarioComSucesso()
    {
        // Arrange
        var request = new CriarUsuarioRequest
        {
            Nome = "Tiago",
            Email = "tiago@test.com",
            Senha = "Senha123!",
            ConfirmarSenha = "Senha123!"
        };

        _usuarioRepoMock.Setup(r => r.EmailExisteAsync(request.Email))
            .ReturnsAsync(false);

        _passwordHasherMock.Setup(h => h.HashPassword(request.Senha))
            .Returns("hashed_senha");

        var usuarioCriado = new Usuarios { Id = 1, Nome = request.Nome, Email = request.Email };
        _usuarioRepoMock.Setup(r => r.CriarAsync(It.IsAny<Usuarios>()))
            .ReturnsAsync(usuarioCriado);

        _carteiraRepoMock.Setup(r => r.CriarAsync(It.IsAny<Carteiras>()))
            .ReturnsAsync(new Carteiras());

        var usuarioResponse = new UsuarioResponse { Id = 1, Nome = request.Nome, Email = request.Email };
        _mapperMock.Setup(m => m.Map<UsuarioResponse>(usuarioCriado))
            .Returns(usuarioResponse);

        var useCase = CriarUseCase();

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(usuarioResponse.Id, result.Id);
        Assert.Equal(usuarioResponse.Nome, result.Nome);
        Assert.Equal(usuarioResponse.Email, result.Email);
        _usuarioRepoMock.Verify(r => r.EmailExisteAsync(request.Email), Times.Once);
        _usuarioRepoMock.Verify(r => r.CriarAsync(It.IsAny<Usuarios>()), Times.Once);
        _carteiraRepoMock.Verify(r => r.CriarAsync(It.IsAny<Carteiras>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveLancarEmailJaExisteException_SeEmailExistir()
    {
        // Arrange
        var request = new CriarUsuarioRequest { Email = "existe@test.com" };
        _usuarioRepoMock.Setup(r => r.EmailExisteAsync(request.Email))
            .ReturnsAsync(true);

        var useCase = CriarUseCase();

        // Act & Assert
        await Assert.ThrowsAsync<EmailJaExisteException>(() => useCase.ExecuteAsync(request));
        _usuarioRepoMock.Verify(r => r.EmailExisteAsync(request.Email), Times.Once);
        _usuarioRepoMock.Verify(r => r.CriarAsync(It.IsAny<Usuarios>()), Times.Never);
    }
}
