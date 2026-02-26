using AutoMapper;
using ApiDemoShop.Model;
using LibDemoShop;

namespace ApiDemoShop.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // BasketItem mappings
            CreateMap<BasketItem, BasketItemDTO>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductPrice,
                    opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.ProductImage,
                    opt => opt.MapFrom(src => src.Product.ProductImages.FirstOrDefault().Image));
            CreateMap<CreateBasketItemDTO, BasketItem>();
            CreateMap<UpdateBasketItemDTO, BasketItem>();

            // Order mappings
            CreateMap<Order, OrderDTO>()
                .ForMember(dest => dest.StatusTitle,
                    opt => opt.MapFrom(src => src.Status.Title))
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User.Username));
            CreateMap<CreateOrderDTO, Order>()
                .ForMember(dest => dest.CreateDate,
                    opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.StatusId,
                    opt => opt.MapFrom(src => 1)); // Статус по умолчанию

            // OrderItem mappings
            CreateMap<OrderItem, OrderItemDTO>()
                .ForMember(dest => dest.OrderId,
                    opt => opt.MapFrom(src => src.OrdeId))
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductPrice,
                    opt => opt.MapFrom(src => src.Product.Price));
            CreateMap<CreateOrderItemDTO, OrderItem>();

            // OrderStatus mappings
            CreateMap<OrderStatus, OrderStatusDTO>();
            CreateMap<CreateOrderStatusDTO, OrderStatus>();

            // Product mappings
            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.Images,
                    opt => opt.MapFrom(src => src.ProductImages.Select(pi => pi.Image).ToList()))
                .ForMember(dest => dest.Tags,
                    opt => opt.MapFrom(src => src.ProductTags.Select(pt => pt.Tag).ToList()))
                .ForMember(dest => dest.MainImage,
                    opt => opt.MapFrom(src => src.ProductImages.FirstOrDefault().Image));
            CreateMap<CreateProductDTO, Product>();
            CreateMap<UpdateProductDTO, Product>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ProductImage mappings
            CreateMap<ProductImage, ProductImageDTO>();
            CreateMap<CreateProductImageDTO, ProductImage>();

            // ProductTag mappings
            CreateMap<ProductTag, ProductTagDTO>()
                .ForMember(dest => dest.TagTitle,
                    opt => opt.MapFrom(src => src.Tag.Title));
            CreateMap<CreateProductTagDTO, ProductTag>();

            // ProductType mappings
            CreateMap<ProductType, ProductTypeDTO>();
            CreateMap<CreateProductTypeDTO, ProductType>();

            // SavedProduct mappings
            CreateMap<SavedProduct, SavedProductDTO>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductPrice,
                    opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.ProductImage,
                    opt => opt.MapFrom(src => src.Product.ProductImages.FirstOrDefault().Image));
            CreateMap<CreateSavedProductDTO, SavedProduct>();

            // Tag mappings
            CreateMap<Tag, TagDTO>()
                .ForMember(dest => dest.TypeTitle,
                    opt => opt.MapFrom(src => src.Type.Title));
            CreateMap<CreateTagDTO, Tag>();
            CreateMap<UpdateTagDTO, Tag>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // User mappings
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.RoleTitle,
                    opt => opt.MapFrom(src => src.Role.Title));
            CreateMap<CreateUserDTO, User>();
            CreateMap<UpdateUserDTO, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // UserRole mappings
            CreateMap<UserRole, UserRoleDTO>();
            CreateMap<CreateUserRoleDTO, UserRole>();
        }
    }
}
