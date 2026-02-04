using Aban.Domain.AbanResponse;

namespace Aban.AppBack.LoadCountries;

public interface IApiService
{
    Task<Response> GetListAsync<T>(string servicePrefix, string controller);
}