using AutoMapper;
using System.Linq;

namespace Postamat.Models.Mapping
{
    /// <summary>
    /// Маппинг данных.
    /// </summary>
    public class MappingProfiler : Profile
    {
        public MappingProfiler()
        {
            CreateMap<Order, OrderInfoDto>()
                .ForMember(info => info.ID,
                    opt => opt.MapFrom(order => order.ID))
                .ForMember(info => info.Status,
                    opt => opt.MapFrom(order => StatusName(order.Status)))
                .ForMember(info => info.Products,
                    opt => opt.MapFrom(order => order.Lines
                        .SelectMany(l => Enumerable.Range(0, l.Quantity).Select(i => l.Product.Name))
                        .OrderBy(p => p)
                        .ToList()))
                .ForMember(info => info.Price,
                    opt => opt.MapFrom(order => order.Price))
                .ForMember(info => info.PostamatNumber,
                    opt => opt.MapFrom(order => order.Postamat.Number))
                .ForMember(info => info.CustomerName,
                    opt => opt.MapFrom(order => order.Name))
                .ForMember(info => info.PhoneNumber,
                    opt => opt.MapFrom(order => order.PhoneNumber));

            CreateMap<Order, OrderAdminInfoDto>()
               .ForMember(info => info.ID,
                   opt => opt.MapFrom(order => order.ID))
               .ForMember(info => info.Status,
                   opt => opt.MapFrom(order => StatusName(order.Status)))
               .ForMember(info => info.Price,
                   opt => opt.MapFrom(order => order.Price))
               .ForMember(info => info.PostamatNumber,
                   opt => opt.MapFrom(order => order.Postamat.Number))
               .ForMember(info => info.CustomerName,
                   opt => opt.MapFrom(order => order.Name))
               .ForMember(info => info.PhoneNumber,
                   opt => opt.MapFrom(order => order.PhoneNumber))
               .ForMember(info => info.CustomerID,
                    opt => opt.MapFrom(order => order.Customer.ID));

            CreateMap<Postamat, PostamatInfoDto>()
                .ForMember(info => info.Number,
                    opt => opt.MapFrom(postamat => postamat.Number))
                .ForMember(info => info.Address,
                    opt => opt.MapFrom(postamat => postamat.Address))
                .ForMember(info => info.IsWorking,
                    opt => opt.MapFrom(postamat => postamat.IsWorking ? "Открыт" : "Закрыт"));

            CreateMap<Product, ProductInfoDto>()
                .ForMember(info => info.Name,
                    opt => opt.MapFrom(product => product.Name))
                .ForMember(info => info.Price,
                    opt => opt.MapFrom(product => product.Price));
        }
        string StatusName(int status) => status switch
        {
            1 => "Зарегистрирован",
            2 => "Принят на складе",
            3 => "Выдан курьеру",
            4 => "Доставлен в постамат",
            5 => "Доставлен получателю",
            6 => "Отменён",
            _ => "Удалён",
        };
    }
}