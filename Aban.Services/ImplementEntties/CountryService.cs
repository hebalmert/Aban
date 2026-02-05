using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Aban.AppInfra;
using Aban.AppInfra.ErrorHandling;
using Aban.AppInfra.Extensions;
using Aban.AppInfra.Transactions;
using Aban.Domain.Entities;
using Aban.Domain.Pagination;
using Aban.Domain.AbanResponse;
using Aban.Services.InterfaceEntities;
using Aban.Domain.Resources;

namespace Aban.Services.ImplementEntties;

public class CountryService : ICountryService
{
    private readonly DataContext _context;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IStringLocalizer _localizer;

    public CountryService(DataContext context, HttpErrorHandler httpErrorHandler,
        IHttpContextAccessor httpContextAccessor, ITransactionManager transactionManager,
        IStringLocalizer localizer)
    {
        _context = context;
        _httpErrorHandler = httpErrorHandler;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _localizer = localizer;
    }

    public async Task<ActionResponse<IEnumerable<Country>>> ComboAsync()
    {
        try
        {
            List<Country> ListModel = await _context.Countries.ToListAsync();
            // Insertar el elemento neutro al inicio
            var defaultItem = new Country
            {
                CountryId = 0,
                Name =$"{_localizer[nameof(Resource.Select_Country)]}"
            };
            ListModel.Insert(0, defaultItem);
            return new ActionResponse<IEnumerable<Country>>
            {
                WasSuccess = true,
                Result = ListModel
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Country>>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<Country>>> GetAsync(PaginationDTO pagination)
    {
        try
        {
            var queryable = _context.Countries.AsQueryable();
            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                //Permite busqueda grandes mateniendo los indices de los campos, solo debemos asegurarnos que el campo a filtrar este indexado en la base de datos
                queryable = queryable.Where(u => EF.Functions.Like(u.Name, $"%{pagination.Filter}%"));
            }

            //En este caso que no tenemos que hacer el Sort por uno o varios campos, podemos usar este metodo simplificado
            var result = await queryable.ApplyFullPaginationAsync(_httpContextAccessor.HttpContext!, pagination);

            return new ActionResponse<IEnumerable<Country>>
            {
                WasSuccess = true,
                Result = result
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Country>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Country>> GetAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return new ActionResponse<Country>
                {
                    WasSuccess = false,
                    Message = _localizer["Generic_InvalidId"]
                };
            }
            var modelo = await _context.Countries
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CountryId == id);
            if (modelo == null)
            {
                return new ActionResponse<Country>
                {
                    WasSuccess = false,
                    Message = _localizer["Generic_IdNotFound"]
                };
            }
            return new ActionResponse<Country>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Country>(ex);
        }
    }

    public async Task<ActionResponse<Country>> UpdateAsync(Country modelo)
    {
        if (modelo == null || modelo.CountryId <= 0)
        {
            return new ActionResponse<Country>
            {
                WasSuccess = false,
                Message = _localizer["Generic_InvalidId"]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            _context.Countries.Update(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Country>
            {
                WasSuccess = true,
                Result = modelo,
                Message = _localizer["Generic_Success"]
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Country>(ex);
        }
    }

    public async Task<ActionResponse<Country>> AddAsync(Country modelo)
    {
        if (modelo == null)
        {
            return new ActionResponse<Country>
            {
                WasSuccess = false,
                Message = _localizer["Generic_InvalidModel"] // 🧠 Clave multilenguaje para modelo nulo
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            _context.Countries.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Country>
            {
                WasSuccess = true,
                Result = modelo,
                Message = _localizer["Generic_Success"] // 🌐 Mensaje localizado de éxito
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Country>(ex); // ✅ Multilenguaje automático en errores
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = _localizer["Generic_InvalidId"] // 🌐 Localizado para ID inválido
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.Countries.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer["Generic_IdNotFound"] // 🌐 Localizado para no encontrado
                };
            }

            _context.Countries.Remove(DataRemove);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = _localizer["Generic_Success"] // 🌐 Localizado para éxito
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }
}