namespace Gerenciador.Exceptions;

public class AcessoNegadoException : Exception
{
    public AcessoNegadoException() : base("Acesso negado ao recurso solicitado")
    {
    }

    public AcessoNegadoException(string message) : base(message)
    {
    }

    public AcessoNegadoException(string message, Exception innerException) : base(message, innerException)
    {
    }
}