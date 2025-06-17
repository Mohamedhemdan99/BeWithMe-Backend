namespace BeWithMe.Services
{
    public class CheckImage
    {
        public  bool IsImage(IFormFile file)
        {
            var allowedTypes = new[] { "image/jpeg", "image/png" };
            return allowedTypes.Contains(file.ContentType);
        }
    }
}
