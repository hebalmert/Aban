using Mapster;


namespace Aban.AppInfra.Mappings;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        var config = TypeAdapterConfig.GlobalSettings;

        //sistema de Pruebas
        //config.NewConfig<QcGeneral, QcGeneral>()
        //    .Ignore(dest => dest.Study!)
        //    .Ignore(dest => dest.Corporation!);


    }
}