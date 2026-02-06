using Aban.Domain.AbanResponse;
using Aban.Domain.EntitesSoftSec;
using Aban.Domain.Enum;
using Aban.Domain.Pagination;
using Aban.Services.InterfacesSecure;
using Aban.UnitOfWork.InterfacesSecure;

namespace Aban.UnitOfWork.ImplementSecure;

public class UsuarioUnitOfWork : IUsuarioUnitOfWork
{
    private readonly IUsuarioService _usuarioService;

    public UsuarioUnitOfWork(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    public async Task<ActionResponse<IEnumerable<EnumItemModel>>> ComboCoordinatorAsync(string username) => await _usuarioService.ComboCoordinatorAsync(username);

    public async Task<ActionResponse<IEnumerable<EnumItemModel>>> ComboAsync(string username) => await _usuarioService.ComboAsync(username);

    public async Task<ActionResponse<IEnumerable<Usuario>>> GetAsync(PaginationDTO pagination, string username) => await _usuarioService.GetAsync(pagination, username);

    public async Task<ActionResponse<Usuario>> GetAsync(int id) => await _usuarioService.GetAsync(id);

    public async Task<ActionResponse<Usuario>> UpdateAsync(Usuario modelo, string urlFront) => await _usuarioService.UpdateAsync(modelo, urlFront);

    public async Task<ActionResponse<Usuario>> AddAsync(Usuario modelo, string urlFront, string username) => await _usuarioService.AddAsync(modelo, urlFront, username);

    public async Task<ActionResponse<bool>> DeleteAsync(int id) => await _usuarioService.DeleteAsync(id);
}