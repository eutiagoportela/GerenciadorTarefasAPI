namespace Gerenciador.Comunicacao.DTOs;

public class RespostaPadrao<T>
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public T? Dados { get; set; }
    public List<string> Erros { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static RespostaPadrao<T> ComSucesso(T? dados, string mensagem = "Operação realizada com sucesso")
    {
        return new RespostaPadrao<T>
        {
            Sucesso = true,
            Mensagem = mensagem,
            Dados = dados
        };
    }

    public static RespostaPadrao<T> ComErro(string mensagem, List<string>? erros = null)
    {
        return new RespostaPadrao<T>
        {
            Sucesso = false,
            Mensagem = mensagem,
            Erros = erros ?? new List<string>()
        };
    }

    public static RespostaPadrao<T> ComErro(string mensagem, string erro)
    {
        return new RespostaPadrao<T>
        {
            Sucesso = false,
            Mensagem = mensagem,
            Erros = new List<string> { erro }
        };
    }
}