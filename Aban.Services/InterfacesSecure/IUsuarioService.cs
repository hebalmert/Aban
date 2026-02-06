using Aban.Domain.EntitesSoftSec;
using Aban.Domain.Enum;
using Aban.Domain.Pagination;
using Aban.Domain.AbanResponse;

namespace Aban.Services.InterfacesSecure;

public interface IUsuarioService
{
    Task<ActionResponse<IEnumerable<EnumItemModel>>> ComboCoordinatorAsync(string username);

    Task<ActionResponse<IEnumerable<EnumItemModel>>> ComboAsync(string email);

    Task<ActionResponse<IEnumerable<Usuario>>> GetAsync(PaginationDTO pagination, string Email);

    Task<ActionResponse<Usuario>> GetAsync(int id);

    Task<ActionResponse<Usuario>> UpdateAsync(Usuario modelo, string UrlFront);

    Task<ActionResponse<Usuario>> AddAsync(Usuario modelo, string urlFront, string Email);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}