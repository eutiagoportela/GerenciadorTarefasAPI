using AutoMapper;
using Gerenciador.Comunicacao.Requests.Usuario;
using Gerenciador.Comunicacao.Responses.Usuario;
using Gerenciador.Dominio.Entidades;
using Gerenciador.Dominio.Repositorios.Interfaces;
using Gerenciador.Exceptions;
using Gerenciador.Infraestrutura.Security;

namespace Gerenciador.Aplicacao.UseCases.Usuario.CriarUsuario;

public class CriarUsuarioUseCase : ICriarUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ICarteiraRepository _carteiraRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public CriarUsuarioUseCase(
        IUsuarioRepository usuarioRepository,
        ICarteiraRepository carteiraRepository,
        IPasswordHasher passwordHasher,
        IMapper mapper)
    {
        _usuarioRepository = usuarioRepository;
        _carteiraRepository = carteiraRepository;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<UsuarioResponse> ExecuteAsync(CriarUsuarioRequest request)
    {
        // Verificar se o email já existe
        var emailExiste = await _usuarioRepository.EmailExisteAsync(request.Email);
        if (emailExiste)
            throw new EmailJaExisteException();

        // Criar o usuário
        var usuario = new Usuarios
        {
            Nome = request.Nome,
            Email = request.Email,
            SenhaHash = _passwordHasher.HashPassword(request.Senha),
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };

        var usuarioCriado = await _usuarioRepository.CriarAsync(usuario);

        // Criar carteira para o usuário
        var carteira = new Carteiras
        {
            UsuarioId = usuarioCriado.Id,
            Saldo = 0,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };

        await _carteiraRepository.CriarAsync(carteira);

        return _mapper.Map<UsuarioResponse>(usuarioCriado);
    }
}