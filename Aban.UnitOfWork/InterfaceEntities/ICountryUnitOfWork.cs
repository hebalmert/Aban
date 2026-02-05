using Aban.Domain.Entities;
using Aban.Domain.Pagination;
using Aban.Domain.AbanResponse;

namespace Aban.UnitOfWork.InterfaceEntities;

public interface ICountryUnitOfWork
{
    Task<ActionResponse<IEnumerable<Country>>> ComboAsync();

    Task<ActionResponse<IEnumerable<Country>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<Country>> GetAsync(int id);

    Task<ActionResponse<Country>> UpdateAsync(Country modelo);

    Task<ActionResponse<Country>> AddAsync(Country modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}