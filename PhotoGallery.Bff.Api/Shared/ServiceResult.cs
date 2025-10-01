namespace PhotoGallery.Bff.Api.Shared
{
    // Bubble up HTTP status + validation errors (To propagate validation errors and statuses to frontend).
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrMsg { get; set; }
        public int StatusCode { get; set; }

        public static ServiceResult<T> Ok(T data, int statusCode = 200) => 
            new ServiceResult<T> { Success = true, Data = data, StatusCode = statusCode };  // Can use full declaration new ServiceResult.

        public static ServiceResult<T> Fail(string error, int statusCode) => 
            new() { Success = false, ErrMsg = error, StatusCode = statusCode }; // Or new() simplified.
    }
}
