using PhotoGallery.Bff.Api.Dtos;
using PhotoGallery.Bff.Api.Shared;
using System.Net.Http;

namespace PhotoGallery.Bff.Api.Clients
{
    public class CategoryServiceClient(HttpClient client)
    {
        public async Task<ServiceResult<IEnumerable<CategoryMinimalDto>>> GetAllCategoriesAsync()
        {
            var response = await client.GetAsync($"api/categories");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return ServiceResult<IEnumerable<CategoryMinimalDto>>.Fail("No category exist!", (int)response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return ServiceResult<IEnumerable<CategoryMinimalDto>>.Fail(error, (int)response.StatusCode);
            }

            var categories = await response.Content.ReadFromJsonAsync<IEnumerable<CategoryMinimalDto>>();
            
            return ServiceResult<IEnumerable<CategoryMinimalDto>>.Ok(categories!, (int)response.StatusCode);
        }

        public async Task<ServiceResult<CategoryMinimalDto>> GetCategoryDetailsByIdAsync(Guid id)
        {
            var response = await client.GetAsync($"api/categories/{id}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CategoryMinimalDto>();
                return ServiceResult<CategoryMinimalDto>.Ok(result!, (int)response.StatusCode);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return ServiceResult<CategoryMinimalDto>.Fail("Category not found!", (int)response.StatusCode);

            var error = await response.Content.ReadAsStringAsync();
            return ServiceResult<CategoryMinimalDto>.Fail(error, (int)response.StatusCode);
        }

        public async Task<Dictionary<Guid, string>> GetCategoriesByIdsAsync(IEnumerable<Guid> ids)
        {
            var query = string.Join("&", ids.Select(id => $"ids={id}"));
            var categories = await client.GetFromJsonAsync<List<CategoryMinimalDto>>($"api/categories/byIds?{query}");

            return categories?.ToDictionary(c => c.CategoryGuid, c => c.Title)
                   ?? [];
        }


        public async Task<ServiceResult<CategoryMinimalDto>> CreateCategoryAsync(CategoryWriteDto categoryDto)
        {
            var response = await client.PostAsJsonAsync("api/categories", categoryDto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CategoryMinimalDto>();
                return ServiceResult<CategoryMinimalDto>.Ok(result!, (int)response.StatusCode);
            }

            var errors = await response.Content.ReadAsStringAsync();
            return ServiceResult<CategoryMinimalDto>.Fail(errors, (int)response.StatusCode);
        }

        public async Task<ServiceResult<bool>> UpdateCategoryAsync(Guid id, CategoryWriteDto categoryDto)
        {
            var response = await client.PutAsJsonAsync($"api/categories/{id}", categoryDto);
            if (response.IsSuccessStatusCode)
            {
                // Update success - returns 204 NoContent.
                return ServiceResult<bool>.Ok(true,(int)response.StatusCode);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return ServiceResult<bool>.Fail("Category not found!", (int)response.StatusCode);

            var errors = await response.Content.ReadAsStringAsync();

            // For all other errors,
            return ServiceResult<bool>.Fail(errors, (int)response.StatusCode);
        }

        public async Task<ServiceResult<bool>> DeleteCategoryAsync(Guid id)
        {
            var response = await client.DeleteAsync($"api/categories/{id}");
            if (response.IsSuccessStatusCode)
            {
                // Success - Returns 204 NoContent.
                return ServiceResult<bool>.Ok(true, (int)response.StatusCode);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return ServiceResult<bool>.Fail("Category not found!", (int)response.StatusCode);

            var errors = await response.Content.ReadAsStringAsync();

            // For all other errors,
            return ServiceResult<bool>.Fail(errors, (int)response.StatusCode);

        }
    }
}
