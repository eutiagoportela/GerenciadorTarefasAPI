
namespace Gerenciador.Exceptions;

public class ValidacaoException : Exception
{
    public List<string> Erros { get; }

    public ValidacaoException(string mensagem) : base(mensagem)
    {
        Erros = new List<string> { mensagem };
    }

    public ValidacaoException(List<string> erros) : base("Erro de validação")
    {
        Erros = erros;
    }

    public ValidacaoException(string mensagem, Exception innerException) : base(mensagem, innerException)
    {
        Erros = new List<string> { mensagem };
    }
}
