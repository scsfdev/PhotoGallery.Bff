using PhotoGallery.Bff.Api.Dtos;
using PhotoGallery.Bff.Api.Shared;
using System.Threading.Tasks;

namespace PhotoGallery.Bff.Api.Clients
{
    public class PhotoServiceClient(HttpClient client)
    {
        public async Task<ServiceResult<IEnumerable<PhotoDto>>> GetAllPhotos(Guid? categoryGuid)
        {
            // Standardize the return so below code no longer use.
            //var photos = await client.GetFromJsonAsync<IEnumerable<PhotoDto>>($"api/photos");
            //return photos ?? Enumerable.Empty<PhotoDto>();

            string url = categoryGuid.HasValue ? $"api/photos?categoryGuid={categoryGuid}" : "api/photos";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var photos = await response.Content.ReadFromJsonAsync<IEnumerable<PhotoDto>>();
                return ServiceResult<IEnumerable<PhotoDto>>.Ok(photos!,(int)response.StatusCode);
            }

            if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return ServiceResult<IEnumerable<PhotoDto>>.Fail("No photo exist!", (int)response.StatusCode);

            var error = await response.Content.ReadAsStringAsync();
            return ServiceResult<IEnumerable<PhotoDto>>.Fail(error, (int)response.StatusCode);
        }

        public async Task<ServiceResult<PhotoDto>> GetPhotoByGuidAsync(Guid photoGuid)
        {
            var response = await client.GetAsync($"api/photos/{photoGuid}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PhotoDto>();
                return ServiceResult<PhotoDto>.Ok(result!, (int)response.StatusCode);
            }

            if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return ServiceResult<PhotoDto>.Fail("Photo not found!", (int) response.StatusCode);

            var error = await response.Content.ReadAsStringAsync();
            return ServiceResult<PhotoDto>.Fail(error, (int)response.StatusCode);
        }

        public async Task<ServiceResult<PhotoDto>> UpsertPhotoAsync(PhotoWriteFormDto dto)
        {
            using var content = new MultipartFormDataContent();

            

            // Add dto data.
            if (dto.PhotoGuid.HasValue)
                content.Add(new StringContent(dto.PhotoGuid.Value.ToString()), nameof(dto.PhotoGuid));

            if(!string.IsNullOrWhiteSpace(dto.Title))
                content.Add(new StringContent(dto.Title), nameof(dto.Title));

            if(!string.IsNullOrWhiteSpace(dto.Description))
                content.Add(new StringContent(dto.Description), nameof(dto.Description));

            if(!string.IsNullOrWhiteSpace(dto.Location))
                content.Add(new StringContent(dto.Location), nameof(dto.Location));

            if (!string.IsNullOrWhiteSpace(dto.Country))
                content.Add(new StringContent(dto.Country), nameof(dto.Country));

            if (dto.DateTaken.HasValue)
                content.Add(new StringContent(dto.DateTaken.Value.ToString("o")), nameof(dto.DateTaken));

            // Add category guids if it is not empty.
            if (dto.CategoryGuids != null && dto.CategoryGuids.Any())
            {
                foreach (var id in dto.CategoryGuids)
                {
                    content.Add(new StringContent(id.ToString()), nameof(dto.CategoryGuids));
                }
            }
                
            // Add file if it is not empty.
            if (dto.File != null && dto.File.Length > 0)
            {
                var fileStream = new StreamContent(dto.File.OpenReadStream());
                fileStream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dto.File.ContentType);

                content.Add(fileStream, nameof(dto.File), dto.File.FileName);
            }

            // Send out.
            var response = await client.PostAsync($"api/photos/upsert", content);

            if (response.IsSuccessStatusCode)
            {
                var photo = await response.Content.ReadFromJsonAsync<PhotoDto>();
                return ServiceResult<PhotoDto>.Ok(photo!, (int)response.StatusCode);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return ServiceResult<PhotoDto>.Fail("Photo not found!", (int)response.StatusCode);

            var error = await response.Content.ReadAsStringAsync();
            return ServiceResult<PhotoDto>.Fail(error, (int)response.StatusCode);
        }

        public async Task<ServiceResult<bool>> DeletePhotoAsync(Guid photoGuid)
        {
            var response = await client.DeleteAsync($"api/photos/{photoGuid}");

            if (response.IsSuccessStatusCode)
                return ServiceResult<bool>.Ok(true, (int)response.StatusCode);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return ServiceResult<bool>.Fail("No photo exist!", (int)response.StatusCode);

            var error = await response.Content.ReadAsStringAsync();
            return ServiceResult<bool>.Fail(error, (int)response.StatusCode);
        }
    }
}
