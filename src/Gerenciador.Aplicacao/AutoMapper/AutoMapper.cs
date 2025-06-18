using AutoMapper;
using Gerenciador.Comunicacao.Responses.Carteira;
using Gerenciador.Comunicacao.Responses.Transferencia;
using Gerenciador.Comunicacao.Responses.Usuario;
using Gerenciador.Dominio.Entidades;

namespace Gerenciador.Aplicacao.AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Usuario
        CreateMap<Usuarios, UsuarioResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.DataCriacao, opt => opt.MapFrom(src => src.DataCriacao));

        // Carteira
        CreateMap<Carteiras, CarteiraResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Saldo, opt => opt.MapFrom(src => src.Saldo))
            .ForMember(dest => dest.UsuarioId, opt => opt.MapFrom(src => src.UsuarioId))
            .ForMember(dest => dest.DataAtualizacao, opt => opt.MapFrom(src => src.DataAtualizacao));

        // Transferencia
        CreateMap<Transferencias, TransferenciaResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Valor, opt => opt.MapFrom(src => src.Valor))
            .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => src.Descricao))
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo))
            .ForMember(dest => dest.DataTransferencia, opt => opt.MapFrom(src => src.DataTransferencia))
            .ForMember(dest => dest.RemetenteId, opt => opt.MapFrom(src => src.RemetenteId))
            .ForMember(dest => dest.DestinatarioId, opt => opt.MapFrom(src => src.DestinatarioId));
    }
}