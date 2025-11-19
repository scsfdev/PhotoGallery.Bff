namespace PhotoGallery.Bff.Api.Dtos
{
    public class PhotoCategoryDto
    {
        public Guid CategoryGuid { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
