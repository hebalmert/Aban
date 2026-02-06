using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Aban.AppBack.Helper;
using Aban.AppInfra.ErrorHandling;
using Aban.Domain.EntitesSoftSec;
using Aban.Domain.Pagination;
using Aban.Domain.ResponcesSec;
using Aban.UnitOfWork.InterfacesSecure;

namespace Aban.AppBack.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/usuarios")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Coordinator")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer _localizer;

        public UsuariosController(IUsuarioUnitOfWork usuarioUnitOfWork,
            IConfiguration configuration, IStringLocalizer localizer)
        {
            _unitOfWork = usuarioUnitOfWork;
            _configuration = configuration;
            _localizer = localizer;
        }

        [HttpGet("CoordinatorCombo")] //Usuario con Roles de Coordinator
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Coordinator")]
        public async Task<IActionResult> GetComboCoordinatorAsync()
        {
            try
            {
                ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
                var response = await _unitOfWork.ComboCoordinatorAsync(userClaimsInfo.UserName);
                return ResponseHelper.Format(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message); // Ya está localizado
            }
            catch (Exception ex)
            {
                return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
            }
        }

        [HttpGet("loadCombo")] //Usuario con Roles de Investigator
        public async Task<IActionResult> GetComboAsync()
        {
            try
            {
                ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
                var response = await _unitOfWork.ComboAsync(userClaimsInfo.UserName);
                return ResponseHelper.Format(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message); // Ya está localizado
            }
            catch (Exception ex)
            {
                return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
            }
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<IActionResult> GetAll([FromQuery] PaginationDTO pagination)
        {
            try
            {
                ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
                var response = await _unitOfWork.GetAsync(pagination, userClaimsInfo.UserName);
                return ResponseHelper.Format(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message); // Ya está localizado
            }
            catch (Exception ex)
            {
                return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<IActionResult> GetAsync(int id)
        {
            try
            {
                var response = await _unitOfWork.GetAsync(id);
                return ResponseHelper.Format(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message); // Ya está localizado
            }
            catch (Exception ex)
            {
                return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<IActionResult> PutAsync(Usuario modelo)
        {
            try
            {
                var response = await _unitOfWork.UpdateAsync(modelo, _configuration["UrlFrontend"]!);
                return ResponseHelper.Format(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message); // Ya está localizado
            }
            catch (Exception ex)
            {
                return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<IActionResult> PostAsync(Usuario modelo)
        {
            try
            {
                ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
                var response = await _unitOfWork.AddAsync(modelo, _configuration["UrlFrontend"]!, userClaimsInfo.UserName);
                return ResponseHelper.Format(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message); // Ya está localizado
            }
            catch (Exception ex)
            {
                return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var response = await _unitOfWork.DeleteAsync(id);
                return ResponseHelper.Format(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message); // Ya está localizado
            }
            catch (Exception ex)
            {
                return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
            }
        }
    }
}