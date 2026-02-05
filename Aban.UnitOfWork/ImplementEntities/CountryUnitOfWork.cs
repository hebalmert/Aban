using Aban.Domain.Entities;
using Aban.Domain.Pagination;
using Aban.Domain.AbanResponse;
using Aban.Services.InterfaceEntities;
using Aban.UnitOfWork.InterfaceEntities;

namespace Aban.UnitOfWork.ImplementEntities;

public class CountryUnitOfWork : ICountryUnitOfWork
{
    private readonly ICountryService _countriesService;

    public CountryUnitOfWork(ICountryService countriesService)
    {
        _countriesService = countriesService;
    }

    public async Task<ActionResponse<IEnumerable<Country>>> ComboAsync() => await _countriesService.ComboAsync();

    public async Task<ActionResponse<IEnumerable<Country>>> GetAsync(PaginationDTO pagination) => await _countriesService.GetAsync(pagination);

    public async Task<ActionResponse<Country>> GetAsync(int id) => await _countriesService.GetAsync(id);

    public async Task<ActionResponse<Country>> UpdateAsync(Country modelo) => await _countriesService.UpdateAsync(modelo);

    public async Task<ActionResponse<Country>> AddAsync(Country modelo) => await _countriesService.AddAsync(modelo);

    public async Task<ActionResponse<bool>> DeleteAsync(int id) => await _countriesService.DeleteAsync(id);
}