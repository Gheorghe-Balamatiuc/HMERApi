namespace HMERApi.Services;

/// <summary>
/// Interface for file operations such as saving and deleting files
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Saves a file to the server's file system
    /// </summary>
    /// <param name="imageFile">The file to save</param>
    /// <param name="allowedFileExtensions">List of allowed file extensions</param>
    /// <returns>The generated filename with extension</returns>
    Task<string> SaveFileAsync(IFormFile imageFile, string[] allowedFileExtensions);
    
    /// <summary>
    /// Deletes a file from the server's file system
    /// </summary>
    /// <param name="fileNameWithExtension">The name of the file to delete</param>
    void DeleteFile(string fileNameWithExtension);
}

/// <summary>
/// Service that handles file operations for the application
/// </summary>
public class FileService(IWebHostEnvironment environment) : IFileService
{
    /// <summary>
    /// Saves a file to the Uploads directory with validation for file extensions
    /// </summary>
    /// <param name="imageFile">The file to save</param>
    /// <param name="allowedFileExtensions">Array of allowed file extensions (e.g., [".jpg", ".png"])</param>
    /// <returns>The generated filename with extension</returns>
    /// <exception cref="ArgumentNullException">Thrown when imageFile is null</exception>
    /// <exception cref="ArgumentException">Thrown when file extension is not allowed</exception>
    public async Task<string> SaveFileAsync(IFormFile imageFile, string[] allowedFileExtensions)
    {
        ArgumentNullException.ThrowIfNull(imageFile);

        var contentPath = environment.ContentRootPath;
        var path = Path.Combine(contentPath, "Uploads");
        // path = "c://projects/ImageManipulation.Ap/uploads" ,not exactly, but something like that

        // Create uploads directory if it doesn't exist
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        // Check the allowed extenstions
        var ext = Path.GetExtension(imageFile.FileName);
        if (!allowedFileExtensions.Contains(ext))
        {
            throw new ArgumentException($"Only {string.Join(",", allowedFileExtensions)} are allowed.");
        }

        // generate a unique filename using GUID to prevent collisions
        var fileName = $"{Guid.NewGuid()}{ext}";
        var fileNameWithPath = Path.Combine(path, fileName);
        using var stream = new FileStream(fileNameWithPath, FileMode.Create);
        await imageFile.CopyToAsync(stream);
        return fileName;
    }

    /// <summary>
    /// Deletes a file from the Uploads directory
    /// </summary>
    /// <param name="fileNameWithExtension">The name of the file to delete (with extension)</param>
    /// <exception cref="ArgumentNullException">Thrown when fileNameWithExtension is null or empty</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist</exception>
    public void DeleteFile(string fileNameWithExtension)
    {
        if (string.IsNullOrEmpty(fileNameWithExtension))
        {
            throw new ArgumentNullException(nameof(fileNameWithExtension));
        }
        var contentPath = environment.ContentRootPath;
        var path = Path.Combine(contentPath, $"Uploads", fileNameWithExtension);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Invalid file path");
        }
        File.Delete(path);
    }
}