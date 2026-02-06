using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Aban.AppInfra;
using Aban.AppInfra.EmailHelper;
using Aban.AppInfra.ErrorHandling;
using Aban.AppInfra.Extensions;
using Aban.AppInfra.FileHelper;
using Aban.AppInfra.Transactions;
using Aban.AppInfra.UserHelper;
using Aban.Domain.EntitesSoftSec;
using Aban.Domain.Entities;
using Aban.Domain.Enum;
using Aban.Domain.Pagination;
using Aban.Domain.AbanResponse;
using Aban.Services.InterfacesSecure;
using Aban.Domain.ResponcesSec;

namespace Aban.Services.ImplementSecure;

public class UsuarioService : IUsuarioService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IFileStorage _fileStorage;
    private readonly IUserHelper _userHelper;
    private readonly IEmailHelper _emailHelper;
    private readonly ImgSetting _imgOption;
    private readonly IStringLocalizer _localizer;

    public UsuarioService(DataContext context, IHttpContextAccessor httpContextAccessor, HttpErrorHandler httpErrorHandle,
        ITransactionManager transactionManager, IFileStorage fileStorage, IStringLocalizer localizer,
        IUserHelper userHelper, IEmailHelper emailHelper, IOptions<ImgSetting> ImgOption)
    {
        _context = context;
        _httpErrorHandler = httpErrorHandle;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _fileStorage = fileStorage;
        _userHelper = userHelper;
        _emailHelper = emailHelper;
        _imgOption = ImgOption.Value;
        _localizer = localizer;
    }

    public async Task<ActionResponse<IEnumerable<EnumItemModel>>> ComboCoordinatorAsync(string username)
    {
        try
        {
            User user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<EnumItemModel>>
                {
                    WasSuccess = false,
                    Message = _localizer["Generic_AuthIdFail"]
                };
            }
            var ListModel = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .Where(u => u.Active &&
                            u.CorporationId == user.CorporationId &&
                            u.UsuarioRoles!.Any(ur => ur.UserType.ToString() == "Coordinator"))
                .Select(u => new EnumItemModel
                {
                    Value = u.UsuarioId,
                    Name = $"{u.FirstName} {u.LastName}"
                })
                .ToListAsync();

            // Insertar el elemento neutro al inicio
            var defaultItem = new EnumItemModel
            {
                Value = 0,
                Name = "[Select Coordinator]"
            };
            ListModel.Insert(0, defaultItem);

            return new ActionResponse<IEnumerable<EnumItemModel>>
            {
                WasSuccess = true,
                Result = ListModel
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<EnumItemModel>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IEnumerable<EnumItemModel>>> ComboAsync(string username)
    {
        try
        {
            User user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<EnumItemModel>>
                {
                    WasSuccess = false,
                    Message = "Problemas para Conseguir el Usuario"
                };
            }
            var ListModel = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .Where(u => u.Active &&
                            u.CorporationId == user.CorporationId &&
                            u.UsuarioRoles!.Any(ur => ur.UserType.ToString() == "Investigator"))
                .Select(u => new EnumItemModel
                {
                    Value = u.UsuarioId,
                    Name = $"{u.FirstName} {u.LastName}"
                })
                .ToListAsync();

            // Insertar el elemento neutro al inicio
            var defaultItem = new EnumItemModel
            {
                Value = 0,
                Name = "[Select Investigator]"
            };
            ListModel.Insert(0, defaultItem);

            return new ActionResponse<IEnumerable<EnumItemModel>>
            {
                WasSuccess = true,
                Result = ListModel
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<EnumItemModel>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IEnumerable<Usuario>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            User user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<Usuario>>
                {
                    WasSuccess = false,
                    Message = "Problemas para Conseguir el Usuario"
                };
            }
            var queryable = _context.Usuarios.Include(x => x.UsuarioRoles).Where(x => x.CorporationId == user.CorporationId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(u =>
                    EF.Functions.Like(u.FirstName, $"%{filter}%") ||
                    EF.Functions.Like(u.LastName, $"%{filter}%") ||
                    EF.Functions.Like(u.FirstName + " " + u.LastName, $"%{filter}%")
                );
            }
            //Como debemos organizar entonces haces la paginacion por separado, y mandamos por varibales de contexto http los parametros
            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.OrderBy(x => x.FirstName).Paginate(pagination).ToListAsync();
            //Agregamos a toda la lista las imagenes guardades de los usuarios por medio de Blob SAS URL
            await Task.WhenAll(modelo.Select(async option =>
            {
                if (!string.IsNullOrWhiteSpace(option.Photo))
                {
                    var FileResult = await _fileStorage.GetBlobSasUrlAsync(option.Photo, _imgOption.ImgUsuario, TimeSpan.FromMinutes(3));
                    option.ImageFullPath = FileResult;
                }
            }));

            return new ActionResponse<IEnumerable<Usuario>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Usuario>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Usuario>> GetAsync(int id)
    {
        try
        {
            var modelo = await _context.Usuarios.FindAsync(id);
            if (modelo == null)
            {
                return new ActionResponse<Usuario>
                {
                    WasSuccess = false,
                    Message = _localizer["Generic_IdNotFound"]
                };
            }

            if (!string.IsNullOrWhiteSpace(modelo.Photo))
            {
                var FileResult = await _fileStorage.GetBlobSasUrlAsync(modelo.Photo, _imgOption.ImgUsuario, TimeSpan.FromMinutes(2));
                modelo.ImageFullPath = FileResult;
            }

            return new ActionResponse<Usuario>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Usuario>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Usuario>> UpdateAsync(Usuario modelo, string urlFront)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            if (!string.IsNullOrEmpty(modelo.ImgBase64))
            {
                string guid;
                if (modelo.Photo == null)
                {
                    guid = Guid.NewGuid().ToString() + ".jpg";
                }
                else
                {
                    guid = modelo.Photo;
                }
                var imageId = Convert.FromBase64String(modelo.ImgBase64);
                //modelo.Photo = await _fileStorage.UploadImage(imageId, _imgOption.ImgUsuario, guid);
                modelo.Photo = await _fileStorage.SaveImageAsync(imageId, guid, _imgOption.ImgUsuario);
            }
            _context.Usuarios.Update(modelo);
            await _transactionManager.SaveChangesAsync();

            User UserCurrent = await _userHelper.GetUserByUserNameAsync(modelo.UserName);
            if (UserCurrent != null)
            {
                UserCurrent.FirstName = modelo.FirstName;
                UserCurrent.LastName = modelo.LastName;
                UserCurrent.PhoneNumber = modelo.PhoneNumber;
                UserCurrent.Email = modelo.Email;
                UserCurrent.PhotoUser = modelo.Photo;
                UserCurrent.Active = modelo.Active;
                IdentityResult result = await _userHelper.UpdateUserAsync(UserCurrent);
            }
            else
            {
                if (modelo.Active)
                {
                    Response response = await AcivateUser(modelo, urlFront);
                    if (response.IsSuccess == false)
                    {
                        var guid = modelo.Photo;
                        await _fileStorage.RemoveFileAsync(_imgOption.ImgUsuario, guid!);
                        await _transactionManager.RollbackTransactionAsync();
                        return new ActionResponse<Usuario>
                        {
                            WasSuccess = true,
                            Message = _localizer["Generic_UserCreationFail"]
                        };
                    }
                }
            }

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Usuario>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Usuario>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Usuario>> AddAsync(Usuario modelo, string urlFront, string username)
    {
        User user = await _userHelper.GetUserByUserNameAsync(username);
        if (user == null)
        {
            return new ActionResponse<Usuario>
            {
                WasSuccess = false,
                Message = _localizer["Generic_AuthIdFail"]
            };
        }
        User userCheck = await _userHelper.GetUserByUserNameAsync(modelo.UserName);
        if (userCheck != null)
        {
            return new ActionResponse<Usuario>
            {
                WasSuccess = false,
                Message = _localizer["Generic_EmailAlreadyUsed"]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            modelo.CorporationId = Convert.ToInt32(user.CorporationId);
            if (modelo.ImgBase64 is not null)
            {
                string guid = Guid.NewGuid().ToString() + ".jpg";
                var imageId = Convert.FromBase64String(modelo.ImgBase64);
                //modelo.Photo = await _fileStorage.UploadImage(imageId, _imgOption.ImgUsuario, guid);
                modelo.Photo = await _fileStorage.SaveImageAsync(imageId, guid, _imgOption.ImgUsuario);
            }
            _context.Usuarios.Add(modelo);
            await _transactionManager.SaveChangesAsync();

            //Aseguramos los cambios en la base de datos
            if (modelo.Active)
            {
                Response response = await AcivateUser(modelo, urlFront);
                if (response.IsSuccess == false)
                {
                    var guid = modelo.Photo;
                    await _fileStorage.RemoveFileAsync(_imgOption.ImgUsuario, guid!);
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<Usuario>
                    {
                        WasSuccess = true,
                        Message = _localizer["Generic_UserCreationFail"]
                    };
                }
            }

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Usuario>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Usuario>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(int id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.Usuarios.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer["Generic_IdNotFound"]
                };
            }

            _context.Usuarios.Remove(DataRemove);
            await _transactionManager.SaveChangesAsync();

            if (DataRemove.Photo is not null)
            {
                var response = await _fileStorage.RemoveFileAsync(_imgOption.ImgUsuario, DataRemove.Photo);
                if (!response)
                {
                    return new ActionResponse<bool>
                    {
                        WasSuccess = true,
                        Message = _localizer["Generic_RecordDeletedNoImage"]
                    };
                }
            }
            await _userHelper.DeleteUser(DataRemove.UserName);

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex); // ✅ Manejo de errores automático
        }
    }

    private async Task<Response> AcivateUser(Usuario modelo, string urlFront)
    {
        User user = await _userHelper.AddUserUsuarioSoftAsync(modelo.FirstName, modelo.LastName, modelo.UserName, modelo.Email,
            modelo.PhoneNumber!, modelo.Address!, modelo.CorporationId, modelo.Photo!, "UsuarioSoftware", modelo.Active);
        //Envio de Correo con Token de seguridad para Verificar el correo
        string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
        // Construir la URL sin `Url.Action`
        string tokenLink = $"{urlFront}/api/accounts/ConfirmEmail?userid={user.Id}&token={myToken}";

        string subject = "Activacion de Cuenta";
        string body = ($"De: Hebalmert" +
            $"<h1>Email Confirmation</h1>" +
            $"<p>" +
            $"Su Clave Temporal es: <h2> \"{user.Pass}\"</h2>" +
            $"</p>" +
            $"Para Activar su vuenta, " +
            $"Has Click en el siguiente Link:</br></br><strong><a href = \"{tokenLink}\">Confirmar Correo</a></strong>");

        Response response = await _emailHelper.ConfirmarCuenta(user.Email!, $"{user.FirstName} {user.LastName}", subject, body);
        if (response.IsSuccess == false)
        {
            return response;
        }
        return response;
    }
}