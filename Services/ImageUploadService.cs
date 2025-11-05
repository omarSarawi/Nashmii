using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

public class ImageUploadService
{
    private readonly Cloudinary _cloudinary;

    public ImageUploadService()
    {
        var account = new Account(
            "dx8dzzx8e",
            "895447386278921",
            "SYo_dLpe7GdsrhMkYwUlDbHVdvw"
        );

        _cloudinary = new Cloudinary(account);
    }

    public async Task<string?> UploadImageAsync(IFormFile imageFile)
    {
        await using var stream = imageFile.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(imageFile.FileName, stream),
            Folder = "categories"
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.SecureUrl != null)
        {
            return uploadResult.SecureUrl.ToString();
        }

        Console.WriteLine(uploadResult.StatusCode);
        Console.WriteLine(uploadResult.Error?.Message);
        Console.WriteLine(uploadResult.SecureUrl);

        return null;
    }
}
