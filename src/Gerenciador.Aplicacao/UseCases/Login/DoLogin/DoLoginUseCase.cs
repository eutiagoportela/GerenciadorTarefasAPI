using AutoMapper;
using Gerenciador.Comunicacao.Requests.Usuario;
using Gerenciador.Comunicacao.Responses.Usuario;
using Gerenciador.Dominio.Repositorios.Interfaces;
using Gerenciador.Exceptions;
using Gerenciador.Infraestrutura.Security;

namespace Gerenciador.Aplicacao.UseCases.Login.DoLogin;

public class DoLoginUseCase : IDoLoginUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IMapper _mapper;

    public DoLoginUseCase(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IMapper mapper)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _mapper = mapper;
    }

    public async Task<LoginResponse> ExecuteAsync(LoginRequest request)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(request.Email);

        if (usuario == null || !_passwordHasher.VerifyPassword(request.Senha, usuario.SenhaHash))
            throw new UsuarioNaoEncontradoException("Email ou senha inválidos.");

        var token = _jwtTokenGenerator.GenerateToken(usuario);
        var expiracaoToken = DateTime.UtcNow.AddHours(24);

        return new LoginResponse
        {
            Token = token,
            ExpiracaoToken = expiracaoToken,
            Usuario = _mapper.Map<UsuarioResponse>(usuario)
        };
    }
}