using Aban.AppBack.Helper;
using Aban.AppInfra.ErrorHandling;
using Aban.Domain.ResponcesSec;
using Aban.UnitOfWork.InterfaceEntities;
using Aban.UnitOfWork.InterfacesEntitiesGen;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Aban.AppBack.Controllers.EntitiesDataGeneral
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/datageneral")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrator, Coordinator")]
    [ApiController]
    public class DataGeneralsController : ControllerBase
    {
        private readonly ICorporationUnitOfWork _unitOfWork;
        private readonly IProductUnitOfWork _productUnitOfWork;
        private readonly IStringLocalizer _localizer;

        public DataGeneralsController(ICorporationUnitOfWork unitOfWork, IProductUnitOfWork productUnitOfWork, IStringLocalizer localizer)
        {
            _unitOfWork = unitOfWork;
            _productUnitOfWork = productUnitOfWork;
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

        [HttpGet("ProductCombo/{id}")]
        public async Task<IActionResult> GetComboAsync(Guid id)
        {
            try
            {
                ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
                var response = await _productUnitOfWork.ComboAsync(userClaimsInfo.UserName, id);
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
