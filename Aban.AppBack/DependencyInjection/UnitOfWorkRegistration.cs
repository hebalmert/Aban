using Aban.Services.ImplementCenter;
using Aban.Services.ImplementDatagen;
using Aban.Services.ImplementEntties;
using Aban.Services.ImplementGen;
using Aban.Services.ImplementPatients;
using Aban.Services.ImplementQc;
using Aban.Services.ImplementSecure;
using Aban.Services.ImplementSetting;
using Aban.Services.ImplementSoft;
using Aban.Services.ImplementStudy;
using Aban.Services.InterfaceDatagen;
using Aban.Services.InterfaceEntities;
using Aban.Services.InterfacePatients;
using Aban.Services.InterfaceQc;
using Aban.Services.InterfaceSetting;
using Aban.Services.InterfacesGen;
using Aban.Services.InterfaceSoft;
using Aban.Services.InterfacesSecure;
using Aban.Services.InterfaceStudy;
using Aban.UnitOfWork.ImplementCenter;
using Aban.UnitOfWork.ImplementDatagen;
using Aban.UnitOfWork.ImplementEntities;
using Aban.UnitOfWork.ImplementGen;
using Aban.UnitOfWork.ImplementPatients;
using Aban.UnitOfWork.ImplementQc;
using Aban.UnitOfWork.ImplementSecure;
using Aban.UnitOfWork.ImplementSetting;
using Aban.UnitOfWork.ImplementSoft;
using Aban.UnitOfWork.ImplementStudy;
using Aban.UnitOfWork.InterfaceDatagen;
using Aban.UnitOfWork.InterfaceEntities;
using Aban.UnitOfWork.InterfacePatients;
using Aban.UnitOfWork.InterfaceQc;
using Aban.UnitOfWork.InterfaceSetting;
using Aban.UnitOfWork.InterfacesGen;
using Aban.UnitOfWork.InterfaceSoft;
using Aban.UnitOfWork.InterfacesSecure;
using Aban.UnitOfWork.InterfaceStudy;
using Aban.Services.ImplementEntitiesGen;

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
            services.AddScoped<ICountryServices, CountryService>();
            services.AddScoped<IStateUnitOfWork, StateUnitOfWork>();
            services.AddScoped<IStateService, StateService>();
            services.AddScoped<ICityUnitOfWork, CityUnitOfWork>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<ISoftPlanUnitOfWork, SoftPlanUnitOfWork>();
            services.AddScoped<ISoftPlanService, SoftPlanService>();
            services.AddScoped<ICorporationUnitOfWork, CorporationUnitOfWork>();
            services.AddScoped<ICorporationService, CorporationService>();
            services.AddScoped<IManagerUnitOfWork, ManagerUnitOfWork>();
            services.AddScoped<IManagerService, ManagerService>();




        }
    }
}