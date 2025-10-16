namespace PhotoGallery.Bff.Api.Dtos
{
    public class AuthResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
}
