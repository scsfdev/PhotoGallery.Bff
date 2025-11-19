using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhotoGallery.Bff.Api.Dtos;
using PhotoGallery.Bff.Api.Services;
using System.Net.WebSockets;
using System.Security.Claims;

namespace PhotoGallery.Bff.Api.Controllers
{
    // TODO:
    // 1. [FINISHED] Add authorization
    // 2. Check how to handle & return error (Or use ProblemDetails)
    //      Eg: Results.Problem(type:"", title: "", detail: "", statusCode: StatusCodes.Status400BadRequest);
    // 3. Proper Log error in each microservice (or check how Microservice log error).
    // 4. Standardize return type ??? (Or Task<Results<NotFound, OK<Object>>>).
    // 5. API Versioning, API Documentation
    // 6. Check EF Core select in batch (SQL DB has parameter limit, eg: Select * from photos where Id in (..up to 1000).
    // 7. Check how to Fine tune data pulling with Filtering, Sorting, Pagination (eg: .Orderby .Skip .Take, etc).
    // 8. Check how to use Caching in pulling data. Consider IEnumerable Vs IQueryable. Validate User Input.
    // 9. Check how to Add compression on return payloads (Eg: BrotliCompressionProvider or GzipCompressionProvider in Program.cs or DI).

    [Route("api/[controller]")]
    [ApiController]
    public class PGBffController(PhotoGalleryOrchestrator bffService) : ControllerBase
    {

        #region User Authorization/Authentication related

        // POST: api/pgbff/user/register
        [HttpPost("user/register")]
        public async Task<ActionResult> RegisterUser([FromBody] RegisterRequestDto registerDto)
        {
            var result = await bffService.RegisterUserAsync(registerDto);
            if (!result.Success)
                return BadRequest("Registration failed!");

            // Optional: auto-login after register
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,                 // In local Docker, for ease of use, no https. For production need to use HTTPS
                SameSite = SameSiteMode.None,   // In case React frontend is hosted on a different domain/downstream service.
                Expires = result.Data!.Expiration
            };

            // If use Cookie, enable below.
            //Response.Cookies.Append("pgbff_auth_token", result.Data!.Token, cookieOptions);

            return Ok(new 
            { 
                id = result.Data!.Id, 
                expiration = result.Data!.Expiration,
                token = result.Data!.Token
            });
        }

        // POST: api/pgbff/user/login
        [HttpPost("user/login")]
        public async Task<ActionResult<AuthResponseDto>> LoginUser([FromBody] LoginRequestDto loginDto)
        {
            var result = await bffService.LoginUserAsync(loginDto);

            if (!result.Success)
                return Unauthorized("Invalid credentials!");

            // If use cookie, enable below.
            //// Set the JWT token in an HttpOnly cookie.
            //var cookieOptions = new CookieOptions
            //{
            //    HttpOnly = true,
            //    Secure = false,                 // In local Docker, for ease of use, no https. For production need to use HTTPS
            //    SameSite = SameSiteMode.None,   // In case React frontend is hosted on a different domain/downstream service.
            //    Expires = result.Data!.Expiration
            //};

            //Response.Cookies.Append("pgbff_auth_token", result.Data!.Token, cookieOptions);


            // Store JWT token in HttpContext for downstream call
            HttpContext.Items["jwt_token"] = result.Data!.Token;
            
            // If result ok, need to pull User Display name from UserService by userId.
            var userProfileResult = await bffService.GetUserProfileAsync(result.Data!.Id);

            // If no profile, something wrong, just return bad request.
            if (!userProfileResult.Success)
                 return BadRequest("Cannot retrieve user profile!");

            // If no profile , just return empty display name.
            var authResponse = new AuthResponseDto
            {
                Id = result.Data!.Id,
                DisplayName = userProfileResult.Data!.DisplayName,
                Expiration = result.Data!.Expiration,
                Token = result.Data!.Token
            };

            return Ok(authResponse);
        }

        // POST: api/pgbff/user/change-password
        [Authorize]
        [HttpPost("user/change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePwdRequestDto changePwdDto)
        {
            var result = await bffService.ChangePasswordAsync(changePwdDto);
            return result.Success
                ? StatusCode(result.StatusCode, result.Data)        // 200 + data.
                : StatusCode(result.StatusCode, new { error = result.ErrMsg });
        }

        // PUT: api/pgbff/user/deactivate/{userGuid}
        [Authorize(Roles = "Admin")]
        [HttpPut("user/deactivate/{userGuid:guid}")]
        public async Task<ActionResult> DeactivateUser(Guid userGuid)
        {
            var result = await bffService.DeactivateUserAsync(userGuid);
            return result.Success
                ? StatusCode(result.StatusCode, result.Data)        // 200 + data.
                : StatusCode(result.StatusCode, new { error = result.ErrMsg });
        }

        // Optional: If use Cookie, enable below Logout method.
        //// POST: api/pgbff/user/logout
        //[Authorize]
        //[HttpPost("user/logout")]
        //public IActionResult LogoutUser()
        //{
        //    // Remove the authentication cookie by setting its expiration to a past date.
        //    if (Request.Cookies.ContainsKey("pgbff_auth_token"))
        //    {
        //        var cookieOptions = new CookieOptions
        //        {
        //            HttpOnly = true,
        //            Secure = false,                 // In local Docker, for ease of use, no https. For production need to use HTTPS
        //            SameSite = SameSiteMode.None,   // In case React frontend is hosted on a different domain/downstream service.
        //            Expires = DateTimeOffset.UtcNow.AddDays(-1) // Expire the cookie immediately
        //        };
        //        Response.Cookies.Append("pgbff_auth_token", "", cookieOptions);

        //        // call to delete for extra safety.
        //        Response.Cookies.Delete("pgbff_auth_token");
        //    }

        //    return Ok("Logged out successfully.");
        //}

        #endregion


        #region UserService related

        [Authorize(Roles = "Admin")]
        [HttpGet("user/get-all-profiles")]
        public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetAllProfiles()
        {
            var result = await bffService.GetAllUserProfilesAsync();
            return result.Success
                ? Ok(result.Data)
                : StatusCode(result.StatusCode, new { error = result.ErrMsg });
        }

        [Authorize]
        [HttpGet("user/profile")]
        public async Task<ActionResult<UserProfileDto>> GetUserProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //// If we want admin to pull out specific profile, we can get role info and check == Admin or not. and pull by pass in ID (maybe from body), not from jwt.
            //var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("Invalid User ID.");

            var result = await bffService.GetUserProfileAsync(userGuid.ToString());
            return result.Success
                ? Ok(result.Data)
                : StatusCode(result.StatusCode, new { error = result.ErrMsg });
        }

        [Authorize]
        [HttpPut("user/profile")]
        public async Task<ActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDto profileUpdateDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // If no id in jwt, reject it.
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("Invalid User ID.");

            //// If log in ID and pass in ID are not the same, reject it.
            //if (userGuid != profileUpdateDto.Id)
            //    return Unauthorized("You are not authorized for the action!");

            // ID will be passed in from jwt, not from body.
            var result = await bffService.UpdateUserProfileAsync(new UserProfileDto { Id = userGuid, DisplayName = profileUpdateDto.DisplayName, Email = profileUpdateDto.Email });
            return result.Success
                ? StatusCode(result.StatusCode)     // 204
                : StatusCode(result.StatusCode, new { error = result.ErrMsg });
        }


        #endregion


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

        [Authorize(Roles = "Admin")]
        [HttpPost("photos/upsert")]
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
        [Authorize(Roles = "Admin")]
        [HttpPost("categories")]
        public async Task<ActionResult> CreateCategory([FromBody] CategoryWriteDto categoryCreate)
        {
            var result = await bffService.CreateCategoryAsync(categoryCreate);

            return result.Success
                ? StatusCode(result.StatusCode, result.Data)        // 201 + data.
                : StatusCode(result.StatusCode, new {error = result.ErrMsg});
        }

        // POST: api/pgbff/categories/{catGuid}
        [Authorize(Roles = "Admin")]
        [HttpPut("categories/{catGuid:guid}")]
        public async Task<ActionResult> UpdateCategory(Guid catGuid, CategoryWriteDto categoryWrite)
        {
            var result = await bffService.UpdateCategoryAsync(catGuid, categoryWrite);

            return result.Success
                ? StatusCode(result.StatusCode)     // 204
                : StatusCode(result.StatusCode, new { error = result.ErrMsg });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("categories/{catGuid:guid}")]
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
