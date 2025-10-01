using PhotoGallery.Bff.Api.Dtos;
using PhotoGallery.Bff.Api.Shared;
using System.Threading.Tasks;

namespace PhotoGallery.Bff.Api.Clients
{
    public class PhotoServiceClient(HttpClient client)
    {
        public async Task<ServiceResult<IEnumerable<PhotoDto>>> GetAllPhotos()
        {
            // Standardize the return so below code no longer use.
            //var photos = await client.GetFromJsonAsync<IEnumerable<PhotoDto>>($"api/photos");
            //return photos ?? Enumerable.Empty<PhotoDto>();

            var response = await client.GetAsync($"api/photos");

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

            // Add file if includes the photo.
            if (dto.File != null && dto.File.Length > 0) {
                var fileStream = new StreamContent(dto.File.OpenReadStream());
                fileStream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dto.File.ContentType);

                content.Add(fileStream, "File", dto.File.FileName);
            }

            // Add remaining dto data.
            if (dto.PhotoGuid.HasValue)
                content.Add(new StringContent(dto.PhotoGuid.Value.ToString()), "PhotoGuid");

            if(!string.IsNullOrEmpty(dto.Title))
                content.Add(new StringContent(dto.Title), "Title");

            if(!string.IsNullOrEmpty(dto.Description))
                content.Add(new StringContent(dto.Description), "Description");

            if(!string.IsNullOrEmpty(dto.Location))
                content.Add(new StringContent(dto.Location), "Location");

            if (!string.IsNullOrEmpty(dto.Country))
                content.Add(new StringContent(dto.Country), "Country");

            if (dto.DateTaken.HasValue)
                content.Add(new StringContent(dto.DateTaken.Value.ToString("o")), "DateTaken");



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
    }
}
