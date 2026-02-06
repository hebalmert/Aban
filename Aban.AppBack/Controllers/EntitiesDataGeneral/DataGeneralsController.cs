using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Aban.AppInfra.ErrorHandling;
using Aban.UnitOfWork.InterfaceEntities;

namespace Aban.AppBack.Controllers.EntitiesDataGeneral
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/datageneral")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrator, Coordinator")]
    [ApiController]
    public class DataGeneralsController : ControllerBase
    {
        private readonly ICorporationUnitOfWork _unitOfWork;
        private readonly IStringLocalizer _localizer;

        public DataGeneralsController(ICorporationUnitOfWork unitOfWork, IStringLocalizer localizer)
        {
            _unitOfWork = unitOfWork;
            _localizer = localizer;
        }


        [HttpGet("CorpCombo")]
        public async Task<IActionResult> GetComboAsync()
        {
            try
            {
                var response = await _unitOfWork.ComboAsync();
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
