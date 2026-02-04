using Mapster;
using MapsterMapper;
using Aban.AppInfra.EmailHelper;
using Aban.AppInfra.ErrorHandling;
using Aban.AppInfra.ExcelHelper;
using Aban.AppInfra.FileHelper;
using Aban.AppInfra.Mappings;
using Aban.AppInfra.QRgenerate;
using Aban.AppInfra.Transactions;
using Aban.AppInfra.UserHelper;
using Aban.AppInfra.UtilityTools;

namespace Aban.AppBack.DependencyInjection
{
    public class InfraRegistration
    {
        public static void AddInfraRegistration(IServiceCollection services, IConfiguration config)
        {
            // Manejo de Errores
            services.AddScoped<HttpErrorHandler>();

            //Para generacion del QR para hacer visitas rapidas de Pacientes.
            services.AddSingleton<IQRService, QRService>();

            // Manejo de transacciones por request
            services.AddScoped<ITransactionManager, TransactionManager>();

            // Utilidades para manejo de Imagenes o Archivos
            services.AddScoped<IFileStorage, FileStorage>();

            // Utilidades para autenticación y gestión de usuarios
            services.AddScoped<IUserHelper, UserHelper>();

            // Herramientas generales sin estado
            services.AddTransient<IUtilityTools, UtilityTools>();

            // Servicio de envío de correos
            services.AddTransient<IEmailHelper, EmailHelper>();

            // Configuración y mapeo con Mapster
            MapsterConfig.RegisterMappings();
            services.AddSingleton(TypeAdapterConfig.GlobalSettings);
            services.AddScoped<IMapper, ServiceMapper>();
            services.AddScoped<IMapperService, MapperService>();
        }
    }
}