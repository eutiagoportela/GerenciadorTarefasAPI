namespace Gerenciador.Comunicacao.DTOs;

public class RespostaPadrao
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public List<string> Erros { get; set; } = new();

    public static RespostaPadrao ComSucesso(string mensagem = "Operação realizada com sucesso")
    {
        return new RespostaPadrao
        {
            Sucesso = true,
            Mensagem = mensagem
        };
    }

    public static RespostaPadrao ComErro(string mensagem, List<string>? erros = null)
    {
        return new RespostaPadrao
        {
            Sucesso = false,
            Mensagem = mensagem,
            Erros = erros ?? new List<string>()
        };
    }
}