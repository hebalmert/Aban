using Aban.Domain.Entities;
using Aban.Domain.Pagination;
using Aban.Domain.ResponcesSec;
using Aban.Domain.AbanResponse;

namespace Aban.UnitOfWork.InterfaceEntities;

public interface IStateUnitOfWork
{
    Task<ActionResponse<IEnumerable<State>>> ComboAsync(ClaimsDTOs claimsDTO);

    Task<ActionResponse<IEnumerable<State>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<State>> GetAsync(int id);

    Task<ActionResponse<State>> UpdateAsync(State modelo);

    Task<ActionResponse<State>> AddAsync(State modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}