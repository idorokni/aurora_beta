using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Client.Communication.Services
{
    public class ImageService
    {
        public static async Task<byte[]> ConvertImagePathToBit(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                throw new FileNotFoundException("Image path is invalid or the file does not exist.");

            byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);

            return imageBytes;
        }

        public static byte[] ConvertImageStringToBit(string imageString)
        {
            return Convert.FromBase64String(imageString);
        }
    }
}
