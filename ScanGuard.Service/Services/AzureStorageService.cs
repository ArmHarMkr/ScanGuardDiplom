using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ScanGuard.BLL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using Image = SixLabors.ImageSharp.Image;
namespace ScanGuard.BLL.Services
{
    public class AzureStorageService : IStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public AzureStorageService(IConfiguration config)
        {
            _containerName = config["AzureStorage:ContainerName"]!;
            _blobServiceClient = new BlobServiceClient(config["AzureStorage:ConnectionString"]);
        }

        public async Task<string> UploadProfilePhotoAsync(IFormFile profilePhoto, string userId)
        {
            if (profilePhoto == null || profilePhoto.Length == 0)
                throw new ArgumentException("Invalid file");

            await using var stream = new MemoryStream();
            await profilePhoto.CopyToAsync(stream);
            stream.Position = 0;

            return await ProcessAndUploadImageAsync(stream, userId);
        }

        public async Task<string> ChangeProfilePhotoAsync(IFormFile newProfilePhoto, string userId)
        {
            if (newProfilePhoto == null || newProfilePhoto.Length == 0)
                throw new ArgumentException("Invalid file");

            // Удаляем старое фото
            await DeleteProfilePhotoAsync(userId);

            // Загружаем новое
            await using var stream = new MemoryStream();
            await newProfilePhoto.CopyToAsync(stream);
            stream.Position = 0;

            return await ProcessAndUploadImageAsync(stream, userId);
        }

        public async Task DeleteProfilePhotoAsync(string userId)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient($"{userId}.jpg");
            await blobClient.DeleteIfExistsAsync();
        }

        private async Task<string> ProcessAndUploadImageAsync(Stream imageStream, string userId)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            using (var image = await Image.LoadAsync(imageStream))
            {
                // Логика обработки изображения
                var (cropSize, finalSize) = CalculateImageSize(image.Width, image.Height);
                var (x, y) = CalculateCropCoordinates(image.Width, image.Height, cropSize);

                image.Mutate(i => i
                    .Crop(new Rectangle(x, y, cropSize, cropSize))
                    .Resize(new ResizeOptions
                    {
                        Size = new Size(finalSize, finalSize),
                        Mode = ResizeMode.Stretch
                    }));

                // Сохранение в Blob Storage
                var blobName = $"{userId}.jpg";
                var blobClient = containerClient.GetBlobClient(blobName);

                using (var outputStream = new MemoryStream())
                {
                    await image.SaveAsync(outputStream, new JpegEncoder());
                    outputStream.Position = 0;
                    await blobClient.UploadAsync(outputStream, overwrite: true);
                }

                return blobClient.Uri.ToString();
            }
        }

        private (int cropSize, int finalSize) CalculateImageSize(int width, int height)
        {
            int cropSize = Math.Min(width, height);
            int finalSize = (width >= 1080 && height >= 1080) ? 1080 : cropSize;
            return (cropSize, finalSize);
        }

        private (int x, int y) CalculateCropCoordinates(int width, int height, int cropSize)
        {
            return ((width - cropSize) / 2, (height - cropSize) / 2);
        }
    }
}
