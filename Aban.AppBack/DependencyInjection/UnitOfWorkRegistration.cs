

using Aban.Services.ImplementEntitiesSoft;
using Aban.Services.ImplementEntties;
using Aban.Services.ImplementSecure;
using Aban.Services.InterfaceEntities;
using Aban.Services.InterfacesEntitiesGen;
using Aban.Services.InterfacesSecure;
using Aban.UnitOfWork.ImplementEntities;
using Aban.UnitOfWork.ImplementEntitiesSoft;
using Aban.UnitOfWork.ImplementSecure;
using Aban.UnitOfWork.InterfaceEntities;
using Aban.UnitOfWork.InterfacesEntitiesGen;
using Aban.UnitOfWork.InterfacesSecure;

namespace Aban.AppBack.DependencyInjection
{
    public class UnitOfWorkRegistration
    {
        public static void AddUnitOfWorkRegistration(IServiceCollection services)
        {
            //EntitiesSecurities Software
            services.AddScoped<IAccountUnitOfWork, AccountUnitOfWork>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUsuarioUnitOfWork, UsuarioUnitOfWork>();
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IUsuarioRoleUnitOfWork, UsuarioRoleUnitOfWork>();
            services.AddScoped<IUsuarioRoleService, UsuarioRoleService>();

            //Entities
            services.AddScoped<ICountryUnitOfWork, CountryUnitOfWork>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IStateUnitOfWork, StateUnitOfWork>();
            services.AddScoped<IStateService, StateService>();
            services.AddScoped<ICityUnitOfWork, CityUnitOfWork>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<ICorporationUnitOfWork, CorporationUnitOfWork>();
            services.AddScoped<ICorporationService, CorporationService>();
            services.AddScoped<IManagerUnitOfWork, ManagerUnitOfWork>();
            services.AddScoped<IManagerService, ManagerService>();

            //EntitiesSoft
            services.AddScoped<IProductUnitOfWork, ProductUnitOfWork>();
            services.AddScoped<IProductService, ProductService>();

        }
    }
}