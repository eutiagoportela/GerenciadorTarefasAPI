namespace Gerenciador.Exceptions;

public class CarteiraException : Exception
{
    public CarteiraException() : base("Erro ao processar operação na carteira")
    {
    }

    public CarteiraException(string message) : base(message)
    {
    }

    public CarteiraException(string message, Exception innerException) : base(message, innerException)
    {
    }
}