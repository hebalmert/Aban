using Aban.Domain.AbanResponse;
using Aban.Domain.DTOs;
using Aban.Domain.EntitiesSoft;
using Aban.Domain.Pagination;

namespace Aban.UnitOfWork.InterfacesEntitiesGen;

public interface IProductUnitOfWork
{
    Task<ActionResponse<IEnumerable<Product>>> ComboAsync(string username, Guid id);

    Task<ActionResponse<IEnumerable<Product>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Product>> GetAsync(Guid id);

    Task<ActionResponse<Product>> UpdateAsync(Product modelo);

    Task<ActionResponse<Product>> AddAsync(ProductDTO modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}