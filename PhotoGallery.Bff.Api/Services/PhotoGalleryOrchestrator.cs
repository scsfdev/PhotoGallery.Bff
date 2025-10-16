using PhotoGallery.Bff.Api.Clients;
using PhotoGallery.Bff.Api.Dtos;
using PhotoGallery.Bff.Api.Shared;

namespace PhotoGallery.Bff.Api.Services
{
    public class PhotoGalleryOrchestrator(AuthServiceClient authClient, UserServiceClient userClient, PhotoServiceClient photoClient, CategoryServiceClient categoryClient) 
    {
        #region User Authorization/Authentication related

        public async Task<ServiceResult<AuthResponseDto>> RegisterUserAsync(RegisterRequestDto registerDto)
        {
            return await authClient.RegisterAsync(registerDto);
        }

        public async Task<ServiceResult<AuthResponseDto>> LoginUserAsync(LoginRequestDto loginDto)
        {
            return await authClient.LoginAsync(loginDto);
        }

        public async Task<ServiceResult<string>> ChangePasswordAsync(ChangePwdRequestDto changePwdDto)
        {
            return await authClient.ChangePasswordAsync(changePwdDto);
        }
        
        public async Task<ServiceResult<string>> DeactivateUserAsync(Guid userGuid)
        {
            return await authClient.DeactivateAsync(userGuid);
        }

        #endregion


        #region UserProfile related

        public async Task<ServiceResult<IEnumerable<UserProfileDto>>> GetAllUserProfilesAsync()
        {
            return await userClient.GetAllProfilesAsync();
        }

        public async Task<ServiceResult<UserProfileDto>> GetUserProfileAsync(string userId)
        {
            return await userClient.GetUserDisplayNameAsync(userId);
        }

        public async Task<ServiceResult<bool>> UpdateUserProfileAsync(UserProfileDto profileUpdateDto)
        {
            return await userClient.UpdateUserProfileAsync(profileUpdateDto);
        }

        #endregion

        #region PhotoService related

        public async Task<ServiceResult<IEnumerable<PhotoDto>>> GetAllPhotosAsync()
        {
            var response = await photoClient.GetAllPhotos();
            if (!response.Success) return response;

            var photos = response.Data!;
            foreach(var photo in photos)
            {
                var categoryIds = photo.PhotoCategories.Select(pc => pc.CategoryGuid).Distinct().ToList();
                if (categoryIds.Count <= 0)
                    continue;

                var categoryMap = await categoryClient.GetCategoriesByIdsAsync(categoryIds);
                foreach (var pc in photo.PhotoCategories)
                {
                    pc.CategoryName = categoryMap.GetValueOrDefault(pc.CategoryGuid, "Unknown");
                }
            }

            return ServiceResult<IEnumerable<PhotoDto>>.Ok(photos, 200);
        }

        public async Task<ServiceResult<PhotoDto>> GetPhotoWithCategoriesAsync(Guid photoGuid)
        {
            var response = await photoClient.GetPhotoByGuidAsync(photoGuid);
            if (!response.Success) return response;

            var photo = response.Data!;

            var categoryIds = photo.PhotoCategories.Select(pc => pc.CategoryGuid).Distinct().ToList();
            if(categoryIds.Count <= 0)
                return ServiceResult<PhotoDto>.Ok(photo, 200);

            var categoryMap = await categoryClient.GetCategoriesByIdsAsync(categoryIds);

            foreach (var pc in photo.PhotoCategories)
            {
                pc.CategoryName = categoryMap.GetValueOrDefault(pc.CategoryGuid, "Unknown");
            }

            return ServiceResult<PhotoDto>.Ok(photo, 200);
        }


        public async Task<ServiceResult<PhotoDto>> UpsertPhotoAsync(PhotoWriteFormDto dto)
        {
            return await photoClient.UpsertPhotoAsync(dto);
        }

        #endregion


        #region CategoryService related

        public async Task<ServiceResult<IEnumerable<CategoryMinimalDto>>> GetAllCategoriesAsync()
        {
            return await categoryClient.GetAllCategoriesAsync();
        }

        public async Task<ServiceResult<CategoryMinimalDto>> GetCategoryByIdAsync(Guid id)
        {
            return await categoryClient.GetCategoryDetailsByIdAsync(id);
        }

        public async Task<ServiceResult<CategoryMinimalDto>> CreateCategoryAsync(CategoryWriteDto categoryWriteDto)
        {
            return await categoryClient.CreateCategoryAsync(categoryWriteDto);
        }

        public async Task<ServiceResult<bool>> UpdateCategoryAsync(Guid id, CategoryWriteDto categoryWriteDto)
        {
            return await categoryClient.UpdateCategoryAsync(id, categoryWriteDto);
        }

        public async Task<ServiceResult<bool>> DeleteCategoryAsync(Guid id)
        {
            return await categoryClient.DeleteCategoryAsync(id);
        }

        #endregion


    }
}
