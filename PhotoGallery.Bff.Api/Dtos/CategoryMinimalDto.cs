namespace PhotoGallery.Bff.Api.Dtos
{
    public class CategoryMinimalDto
    {
        public Guid CategoryGuid { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
