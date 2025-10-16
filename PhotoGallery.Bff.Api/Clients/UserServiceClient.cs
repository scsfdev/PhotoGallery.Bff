using PhotoGallery.Bff.Api.Dtos;
using PhotoGallery.Bff.Api.Shared;

namespace PhotoGallery.Bff.Api.Clients
{
    public class UserServiceClient(HttpClient client)
    {
        public async Task<ServiceResult<UserProfileDto>> GetUserDisplayNameAsync(string userId)
        {
            var response = await client.GetAsync("api/userprofiles/getbyid/");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return ServiceResult<UserProfileDto>.Fail(error, (int)response.StatusCode);
            }

            var result = await response.Content.ReadFromJsonAsync<UserProfileDto>();
            return ServiceResult<UserProfileDto>.Ok(result!, (int)response.StatusCode);
        }

        public async Task<ServiceResult<IEnumerable<UserProfileDto>>> GetAllProfilesAsync()
        {
            var response = await client.GetAsync($"api/userprofiles/getall");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return ServiceResult<IEnumerable<UserProfileDto>>.Fail("No User exist!", (int)response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return ServiceResult<IEnumerable<UserProfileDto>>.Fail(error, (int)response.StatusCode);
            }

            var categories = await response.Content.ReadFromJsonAsync<IEnumerable<UserProfileDto>>();

            return ServiceResult<IEnumerable<UserProfileDto>>.Ok(categories!, (int)response.StatusCode);
        }

        public async Task<ServiceResult<bool>> UpdateUserProfileAsync(UserProfileDto profileUpdateDto)
        {
            var response = await client.PutAsJsonAsync("api/userprofiles/updateprofile", profileUpdateDto);
            if (response.IsSuccessStatusCode)
            {
                // Update success - returns 204 NoContent.
                return ServiceResult<bool>.Ok(true, (int)response.StatusCode);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return ServiceResult<bool>.Fail("User Profile not found!", (int)response.StatusCode);

            var errors = await response.Content.ReadAsStringAsync();

            // For all other errors,
            return ServiceResult<bool>.Fail(errors, (int)response.StatusCode);
        }
    }
}
