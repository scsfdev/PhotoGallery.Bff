using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhotoGallery.Bff.Api.Dtos;
using PhotoGallery.Bff.Api.Services;
using System.Net.WebSockets;

namespace PhotoGallery.Bff.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PGBffController(PhotoGalleryOrchestrator bffService) : ControllerBase
    {
        #region PhotoService related

        [HttpGet("photos")]
        public async Task<ActionResult<IEnumerable<PhotoDto>>> GetAllPhotos()
        {
            var result = await bffService.GetAllPhotosAsync();
           
            return result.Success
                ? Ok(result.Data)
                : StatusCode(result.StatusCode, new { error = result.ErrMsg});
        }

        [HttpGet("photos/{photoGuid:guid}")]
        public async Task<ActionResult<PhotoDto>> GetPhoto(Guid photoGuid)
        {
            var result = await bffService.GetPhotoWithCategoriesAsync(photoGuid);

            return result.Success
                ? Ok(result.Data)
                : StatusCode(result.StatusCode, new { error = result.ErrMsg });
        }

        [HttpPost("upsert")]
        public async Task<ActionResult<PhotoDto>> UpsertPhoto([FromForm] PhotoWriteFormDto dto)
        {
            var result = await bffService.UpsertPhotoAsync(dto);

            return result.Success
                ? StatusCode(result.StatusCode, result.Data)
                : StatusCode(result.StatusCode, new { error = result.ErrMsg });
        }

        #endregion


        #region CategoryService related

        // GET: api/pgbff/categories
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<CategoryMinimalDto>>> GetAllCategories()
        {
            var result = await bffService.GetAllCategoriesAsync();

            return result.Success
                ? Ok(result.Data)
                : StatusCode(result.StatusCode, new { error = result.ErrMsg });
        }

        // GET: api/pgbff/categories/{catGuid}
        [HttpGet("categories/{catGuid:guid}")]
        public async Task<ActionResult<CategoryMinimalDto>> GetCategoryById(Guid catGuid)
        {
            var result = await bffService.GetCategoryByIdAsync(catGuid);

            return result.Success
                ? Ok(result.Data) 
                : StatusCode(result.StatusCode, new { error = result.ErrMsg});
        }

        // POST: api/pgbff/categories
        [HttpPost("categories")]
        public async Task<ActionResult> CreateCategory([FromBody] CategoryWriteDto categoryCreate)
        {
            var result = await bffService.CreateCategoryAsync(categoryCreate);

            return result.Success
                ? StatusCode(result.StatusCode, result.Data)        // 201 + data.
                : StatusCode(result.StatusCode, new {error = result.ErrMsg});
        }

        // POST: api/pgbff/categories/{catGuid}
        [HttpPut("{catGuid:guid}")]
        public async Task<ActionResult> UpdateCategory(Guid catGuid, CategoryWriteDto categoryWrite)
        {
            var result = await bffService.UpdateCategoryAsync(catGuid, categoryWrite);

            return result.Success
                ? StatusCode(result.StatusCode)     // 204
                : StatusCode(result.StatusCode, new { error = result.ErrMsg });
        }


        [HttpDelete("{catGuid:guid}")]
        public async Task<ActionResult> DeleteCategory(Guid catGuid)
        {
            var result = await bffService.DeleteCategoryAsync(catGuid);

            return result.Success
                ? StatusCode(result.StatusCode)     // 204
                : StatusCode(result.StatusCode, new { error = result.ErrMsg});
        }

        #endregion


    }
}
