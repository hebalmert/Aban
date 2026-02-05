using Aban.Domain.Entities;
using Aban.Domain.Pagination;
using Aban.Domain.AbanResponse;

namespace Aban.UnitOfWork.InterfaceEntities;

public interface ICorporationUnitOfWork
{
    Task<ActionResponse<IEnumerable<Corporation>>> ComboAsync();

    Task<ActionResponse<IEnumerable<Corporation>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<Corporation>> GetAsync(int id);

    Task<ActionResponse<Corporation>> UpdateAsync(Corporation modelo);

    Task<ActionResponse<Corporation>> AddAsync(Corporation modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}