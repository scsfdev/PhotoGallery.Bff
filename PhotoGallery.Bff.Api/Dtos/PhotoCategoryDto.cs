namespace PhotoGallery.Bff.Api.Dtos
{
    public class PhotoCategoryDto
    {
        public Guid CategoryGuid { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
