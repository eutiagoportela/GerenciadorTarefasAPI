
namespace Gerenciador.Exceptions;

public class ConcorrenciaException : Exception
{
    public ConcorrenciaException() : base("Erro de concorrência. Tente novamente.")
    {
    }

    public ConcorrenciaException(string message) : base(message)
    {
    }

    public ConcorrenciaException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
