namespace Gerenciador.Exceptions;

public class EmailJaExisteException : Exception
{
    public EmailJaExisteException() : base("O email informado já está em uso")
    {
    }

    public EmailJaExisteException(string message) : base(message)
    {
    }

    public EmailJaExisteException(string message, Exception innerException) : base(message, innerException)
    {
    }
}