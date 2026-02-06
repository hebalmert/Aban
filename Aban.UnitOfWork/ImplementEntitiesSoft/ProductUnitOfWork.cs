using Aban.Domain.AbanResponse;
using Aban.Domain.DTOs;
using Aban.Domain.EntitiesSoft;
using Aban.Domain.Pagination;
using Aban.Services.InterfacesEntitiesGen;
using Aban.UnitOfWork.InterfacesEntitiesGen;

namespace Aban.UnitOfWork.ImplementEntitiesSoft;

public class ProductUnitOfWork : IProductUnitOfWork
{
    private readonly IProductService _productService;

    public ProductUnitOfWork(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<ActionResponse<IEnumerable<Product>>> ComboAsync(string username, Guid id) => await _productService.ComboAsync(username, id);

    public async Task<ActionResponse<IEnumerable<Product>>> GetAsync(PaginationDTO pagination, string username) => await _productService.GetAsync(pagination, username);

    public async Task<ActionResponse<Product>> GetAsync(Guid id) => await _productService.GetAsync(id);

    public async Task<ActionResponse<Product>> UpdateAsync(Product modelo) => await _productService.UpdateAsync(modelo);

    public async Task<ActionResponse<Product>> AddAsync(ProductDTO modelo, string username) => await _productService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _productService.DeleteAsync(id);
}
