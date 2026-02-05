using Aban.Domain.AbanResponse;
using Aban.Domain.Entities;
using Aban.Domain.Pagination;

namespace Aban.Services.InterfaceEntities;

public interface ICityService
{
    Task<ActionResponse<IEnumerable<City>>> ComboAsync(int id);

    Task<ActionResponse<IEnumerable<City>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<City>> GetAsync(int id);

    Task<ActionResponse<City>> UpdateAsync(City modelo);

    Task<ActionResponse<City>> AddAsync(City modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}
