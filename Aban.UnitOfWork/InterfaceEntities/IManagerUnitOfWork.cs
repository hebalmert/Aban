using Aban.Domain.Entities;
using Aban.Domain.Pagination;
using Aban.Domain.AbanResponse;

namespace Aban.UnitOfWork.InterfaceEntities;

public interface IManagerUnitOfWork
{
    Task<ActionResponse<IEnumerable<Manager>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<Manager>> GetAsync(int id);

    Task<ActionResponse<Manager>> UpdateAsync(Manager modelo, string frontUrl);

    Task<ActionResponse<Manager>> AddAsync(Manager modelo, string frontUrl);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}