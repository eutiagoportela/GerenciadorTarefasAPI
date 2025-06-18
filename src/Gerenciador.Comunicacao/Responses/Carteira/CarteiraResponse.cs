namespace Gerenciador.Comunicacao.Responses.Carteira;

public class CarteiraResponse
{
    public int Id { get; set; }
    public decimal Saldo { get; set; }
    public int UsuarioId { get; set; }
    public DateTime DataAtualizacao { get; set; }
}