using Aban.AppInfra;
using Aban.AppInfra.ErrorHandling;
using Aban.AppInfra.Extensions;
using Aban.AppInfra.Mappings;
using Aban.AppInfra.Transactions;
using Aban.AppInfra.UserHelper;
using Aban.AppInfra.Validations;
using Aban.Domain.AbanResponse;
using Aban.Domain.DTOs;
using Aban.Domain.Entities;
using Aban.Domain.EntitiesSoft;
using Aban.Domain.Pagination;
using Aban.Domain.Resources;
using Aban.Services.InterfacesEntitiesGen;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Aban.Services.ImplementEntitiesSoft;

public class ProductService : IProductService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IMapperService _mapperService;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public ProductService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IUserHelper userHelper, IMapperService mapperService,
        HttpErrorHandler httpErrorHandler, IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _mapperService = mapperService;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    public async Task<ActionResponse<IEnumerable<Product>>> ComboAsync(string username, Guid id)
    {
        try
        {
            User user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<Product>>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }
            var ListModel = await _context.Products
                .Where(x => x.Active && x.CorporationId == user.CorporationId)
                .ToListAsync();
            var defaultItem = new Product
            {
                ProductId = Guid.Empty,
                ProductName = $"[{_localizer[nameof(Resource.Product)]}]"
            };
            ListModel.Insert(0, defaultItem);

            return new ActionResponse<IEnumerable<Product>>
            {
                WasSuccess = true,
                Result = ListModel
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Product>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IEnumerable<Product>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            User user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<Product>>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            var queryable = _context.Products
                .Where(x => x.CorporationId == pagination.Id).AsQueryable(); //Temporalmente no sera user.CorporationId porque no estoy validado con otro Role solo Admin

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.ProductName!.ToLower().Contains(pagination.Filter.ToLower()));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.OrderBy(x => x.ProductName).Paginate(pagination).ToListAsync();

            return new ActionResponse<IEnumerable<Product>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Product>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Product>> GetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return new ActionResponse<Product>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }
        try
        {
            var modelo = await _context.Products
                .FirstOrDefaultAsync(x => x.ProductId == id);
            if (modelo == null)
            {
                return new ActionResponse<Product>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            return new ActionResponse<Product>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Product>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Product>> UpdateAsync(Product modelo)
    {
        if (modelo == null || modelo.ProductId == Guid.Empty)
        {
            return new ActionResponse<Product>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        await _transactionManager.BeginTransactionAsync();

        try
        {

            _context.Products.Update(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Product>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Product>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Product>> AddAsync(ProductDTO modelo, string username)
    {
        if (modelo == null)
        {
            return new ActionResponse<Product>
            {
                WasSuccess = false,
                Message = _localizer["Generic_InvalidModel"]
            };
        }

        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<Product>
            {
                WasSuccess = false,
                Message = _localizer["Generic_InvalidModel"]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            User user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<Product>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }
            Product NewModelo = _mapperService.Map<ProductDTO, Product>(modelo);
            NewModelo.CorporationId = 1; //temporalmente sera este porque no estoy probando Swagger sin Registrar con otros Roles
            _context.Products.Add(NewModelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Product>
            {
                WasSuccess = true,
                Result = NewModelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Product>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.Products.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.Products.Remove(DataRemove);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex); // ✅ Manejo de errores automático
        }
    }
}
