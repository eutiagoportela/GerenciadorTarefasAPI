using Gerenciador.Dominio.Enum;

namespace Gerenciador.Comunicacao.Responses.Transferencia;

public class TransferenciaResponse
{
    public int Id { get; set; }
    public decimal Valor { get; set; }
    public string? Descricao { get; set; }
    public TipoTransferencia Tipo { get; set; }
    public string TipoDescricao => Tipo.ToString();
    public DateTime DataTransferencia { get; set; }
    public int RemetenteId { get; set; }
    public string RemetenteNome { get; set; } = string.Empty;
    public int DestinatarioId { get; set; }
    public string DestinatarioNome { get; set; } = string.Empty;
}