using Aban.Domain.DTOs;
using Aban.Domain.EntitiesSoft;
using Mapster;


namespace Aban.AppInfra.Mappings;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        var config = TypeAdapterConfig.GlobalSettings;

        //sistema de Pruebas para Trabjar Mappers
        //config.NewConfig<QcGeneral, QcGeneral>()
        //    .Ignore(dest => dest.Study!)
        //    .Ignore(dest => dest.Corporation!);

        config.NewConfig<ProductDTO, Product>()
            .Ignore(dest => dest.Corporation!);
        config.NewConfig<Product, ProductDTO>();
    }
}