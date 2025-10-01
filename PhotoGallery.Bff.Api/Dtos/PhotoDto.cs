namespace PhotoGallery.Bff.Api.Dtos
{
    public class PhotoDto
    {
        public Guid PhotoGuid { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int LikesCount { get; set; }
        public List<PhotoCategoryDto> PhotoCategories { get; set; } = new();
    }
}
