namespace PhotoGallery.Bff.Api.Dtos
{
    public class ChangePwdRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
