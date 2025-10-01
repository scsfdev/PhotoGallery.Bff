namespace PhotoGallery.Bff.Api.Dtos
{
    public class PhotoWriteFormDto
    {
        public Guid? PhotoGuid { get; set; }        // null = Insert (Create), not null = Update.

        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? Country { get; set; }

        public DateTime? DateTaken { get; set; }

        // File for upload
        public IFormFile? File { get; set; }    // null = No upload action, not null = Upload to GCS.
    }
}
