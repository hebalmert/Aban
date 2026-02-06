using Aban.Domain.EntitesSoftSec;
using Aban.Domain.Enum;
using Aban.Domain.Pagination;
using Aban.Domain.AbanResponse;

namespace Aban.Services.InterfacesSecure;

public interface IUsuarioRoleService
{
    Task<ActionResponse<IEnumerable<EnumItemModel>>> ComboAsync();

    Task<ActionResponse<IEnumerable<UsuarioRole>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<UsuarioRole>> GetAsync(int id);

    Task<ActionResponse<UsuarioRole>> AddAsync(UsuarioRole modelo, string Email);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}