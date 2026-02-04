using Aban.Domain.AbanResponse;
using Aban.Domain.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Aban.AppInfra.ErrorHandling;

public class HttpErrorHandler
{
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<HttpErrorHandler> _logger;

    public HttpErrorHandler(IStringLocalizer localizer, ILogger<HttpErrorHandler> logger)
    {
        _localizer = localizer;
        _logger = logger;
    }

    public Task<ActionResponse<T>> HandleErrorAsync<T>(Exception exception)
    {
        string errorMessage = _localizer[nameof(Resource.Generic_UnexpectedError)];

        if (exception is null)
        {
            errorMessage = _localizer[nameof(Resource.Generic_NullException)];

            _logger.LogError("Null exception received in HttpErrorHandler");

            return Task.FromResult(new ActionResponse<T>
            {
                WasSuccess = false,
                Message = errorMessage,
                Result = default
            });
        }

        // Log general info BEFORE procesar
        _logger.LogError(
            exception,
            "Error capturado en HttpErrorHandler. Tipo: {ExceptionType}, Mensaje: {ExceptionMessage}",
            exception.GetType().Name,
            exception.Message
        );

        if (exception is HttpRequestException httpEx)
        {
            errorMessage = $"{_localizer[nameof(Resource.Generic_Http_BadRequest)]}: {httpEx.Message}";
        }
        else if (exception is DbUpdateException dbEx)
        {
            var innerMsg = dbEx.InnerException?.Message?.ToLower() ?? "";

            if (innerMsg.Contains("duplicate key") || innerMsg.Contains("unique constraint"))
            {
                errorMessage = _localizer[nameof(Resource.Db_Duplicate)];
            }
            else if (innerMsg.Contains("foreign key") || innerMsg.Contains("reference"))
            {
                errorMessage = _localizer[nameof(Resource.Db_Reference)];
            }
            else
            {
                errorMessage = $"{_localizer[nameof(Resource.Db_Error)]}: {dbEx.Message}";
            }
        }
        else if (exception is DbUpdateConcurrencyException)
        {
            errorMessage = _localizer[nameof(Resource.Db_Concurrency)];
        }
        else
        {
            errorMessage = $"{_localizer[nameof(Resource.Generic_Exception)]}: {exception.Message}";
        }

        // Log final con el mensaje traducido
        _logger.LogError(
            exception,
            "Mensaje final enviado al cliente: {ErrorMessage}",
            errorMessage
        );

        return Task.FromResult(new ActionResponse<T>
        {
            WasSuccess = false,
            Message = errorMessage,
            Result = default
        });
    }
}
