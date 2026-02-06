using Aban.Domain.EntitesSoftSec;
using Aban.Domain.Enum;
using Aban.Domain.Pagination;
using Aban.Domain.AbanResponse;

namespace Aban.UnitOfWork.InterfacesSecure;

public interface IUsuarioUnitOfWork
{
    Task<ActionResponse<IEnumerable<EnumItemModel>>> ComboCoordinatorAsync(string username);

    Task<ActionResponse<IEnumerable<EnumItemModel>>> ComboAsync(string username);

    Task<ActionResponse<IEnumerable<Usuario>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Usuario>> GetAsync(int id);

    Task<ActionResponse<Usuario>> UpdateAsync(Usuario modelo, string UrlFront);

    Task<ActionResponse<Usuario>> AddAsync(Usuario modelo, string urlFront, string username);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}