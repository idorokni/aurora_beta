using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Communication.Services
{
    public class ImageStorageService
    {
        private static async Task<string> GenerateImageIdentifierAsync(string username)
        {
            return await Task.Run(() =>
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                var guid = Guid.NewGuid().ToString();
                return $"{timestamp}_{guid}";
            });
        }

        public static async Task ModifyImageDirectoryNameForNewUser(string oldUsername, string newUsername)
        {
            try
            {
                var baseDirectory = "Images";
                var oldUserDirectory = Path.Combine(baseDirectory, oldUsername);
                var newUserDirectory = Path.Combine(baseDirectory, newUsername);

                if (Directory.Exists(oldUserDirectory))
                {
                    Directory.Move(oldUserDirectory, newUserDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error renaming directory: {ex.Message}");
            }
        }


        public static async Task<string> SaveProfileImage(string username, byte[] imageBytes)
        {
            try
            {
                var profileIdentifier = $"{username}_ProfileImage";
                var baseDirectory = "Images";
                if (!Directory.Exists(baseDirectory))
                {
                    Directory.CreateDirectory(baseDirectory);
                }
                var userDirectory = Path.Combine(baseDirectory, username);
                if (!Directory.Exists(userDirectory))
                {
                    Directory.CreateDirectory(userDirectory);
                }

                var profileImagePath = Path.Combine(userDirectory, $"{profileIdentifier}.txt");
                await File.WriteAllTextAsync(profileImagePath, Convert.ToBase64String(imageBytes));
                return profileImagePath;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        public static async Task<string> SaveImageAsync(string userID, byte[] imageBytes)
        {
            try
            {
                var imageIdentifier = await GenerateImageIdentifierAsync(userID);
                var baseDirectory = "Images";
                if (!Directory.Exists(baseDirectory))
                {
                    Directory.CreateDirectory(baseDirectory);
                }
                var userDirectory = Path.Combine(baseDirectory, userID);
                if (!Directory.Exists(userDirectory))
                {
                    Directory.CreateDirectory(userDirectory);
                }
                var postPath = Path.Combine(userDirectory, $"{imageIdentifier}.txt");
                await File.WriteAllTextAsync(postPath, Convert.ToBase64String(imageBytes));
                return postPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        public static async Task<string> GetImageAsync(string path)
        {
            try
            {
                return await File.ReadAllTextAsync(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

    }
}
