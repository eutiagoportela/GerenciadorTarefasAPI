namespace Gerenciador.Exceptions;

public class UsuarioNaoEncontradoException : Exception
{
    public UsuarioNaoEncontradoException() : base("Usuário não encontrado")
    {
    }

    public UsuarioNaoEncontradoException(string message) : base(message)
    {
    }

    public UsuarioNaoEncontradoException(string message, Exception innerException) : base(message, innerException)
    {
    }
}