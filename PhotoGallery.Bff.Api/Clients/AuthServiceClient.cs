
using PhotoGallery.Bff.Api.Dtos;
using PhotoGallery.Bff.Api.Shared;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PhotoGallery.Bff.Api.Clients
{
    public class AuthServiceClient(HttpClient client)
    {
        public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto reuqest)
        {
            var response = await client.PostAsJsonAsync("api/auth/login", reuqest);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return ServiceResult<AuthResponseDto>.Fail(error, (int)response.StatusCode);
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            return ServiceResult<AuthResponseDto>.Ok(tokenResponse!, (int)response.StatusCode);
        }

        public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            var response = await client.PostAsJsonAsync("api/auth/register", request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return ServiceResult<AuthResponseDto>.Fail(error, (int)response.StatusCode);
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            return ServiceResult<AuthResponseDto>.Ok(tokenResponse!, (int)response.StatusCode);
        }

        public async Task<ServiceResult<string>> ChangePasswordAsync(ChangePwdRequestDto request)
        {
            var response = await client.PostAsJsonAsync("api/auth/change-password", request);
            if(!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return ServiceResult<string>.Fail(error, (int)response.StatusCode);
            }

            return ServiceResult<string>.Ok("Password changed successfully.", (int)response.StatusCode);
        }

        public async Task<ServiceResult<string>> DeactivateAsync(Guid userGuid)
        {
            var response = await client.PutAsync($"api/auth/deactivate/{userGuid}",null);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return ServiceResult<string>.Fail(error, (int)response.StatusCode);
            }

            return ServiceResult<string>.Ok("User deactivated successfully.", (int)response.StatusCode);
        }
    }
}
