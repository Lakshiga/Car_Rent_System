using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Car_Rent_System.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(string cloudinaryUrl)
        {
            _cloudinary = new Cloudinary(cloudinaryUrl);
            _cloudinary.Api.Secure = true;
        }

        public string UploadImage(string localFilePath)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(localFilePath),
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = true
            };

            var uploadResult = _cloudinary.Upload(uploadParams);
            return uploadResult.SecureUrl.ToString(); 
        }
    }
}
