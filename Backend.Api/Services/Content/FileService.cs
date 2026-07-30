using System.Buffers;
using System.Security.Cryptography;
using Backend.Api.Core.Entities.Content;
using Backend.Api.Core.Types;
using Backend.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Api.Services.Content;

public class FileService(
    IDbContextFactory<AppDbContext> dbFactory,
    IOptions<Core.Common.FileOptions> fileOptions
    )
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly string _root = fileOptions.Value.RootPath;

    public async Task<FileAsset> UploadAsync(
        Stream stream,
        string originalFileName,
        string contentType,
        long userId,
        FileCategory category,
        CancellationToken ct = default)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var today = DateTime.UtcNow;
        var relativeDirectory = Path.Combine(
            GetFolder(category),
            today.Year.ToString(),
            today.Month.ToString("00"));
        var directory = Path.Combine(_root, relativeDirectory);

        Directory.CreateDirectory(directory);

        var relativePath = Path.Combine(relativeDirectory, fileName);
        var absolutePath = Path.Combine(_root, relativePath);
        long size = 0;

        string hash;
        await using (var output = File.Create(absolutePath))
        using (var sha = SHA256.Create())
        using (var crypto = new CryptoStream(output, sha, CryptoStreamMode.Write))
        {
            var buffer = ArrayPool<byte>.Shared.Rent(81920); // Default buffer size of CopyToAsync()
            int read;

            try
            {
                while ((read = await stream.ReadAsync(buffer, ct)) > 0)
                {
                    await crypto.WriteAsync(buffer.AsMemory(0, read), ct);
                    size += read;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            await crypto.FlushFinalBlockAsync(ct);
            hash = Convert.ToHexString(sha.Hash!);
        }

        return new FileAsset
        {
            UploaderId = userId,
            OriginalFileName = originalFileName,
            StoragePath = relativePath.Replace('\\', '/'),
            ContentType = contentType,
            SizeInBytes = size,
            Sha256Hash = hash
        };
    }

    public Task<Stream> ReadAsync(string storagePath)
    {
        var path = Path.Combine(_root, storagePath);
        Stream stream = File.OpenRead(path);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath)
    {
        var path = Path.Combine(_root, storagePath);

        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    static private string GetFolder(FileCategory category)
    {
        return category switch
        {
            FileCategory.Avatar => "avatars",

            FileCategory.CourseThumbnail => "course-thumbnails",

            FileCategory.CourseBackground => "course-backgrounds",

            FileCategory.LessonAttachment => "lesson-attachments",

            FileCategory.AssignmentSubmission => "submissions",

            _ => throw new ArgumentOutOfRangeException(nameof(category))
        };
    }
}
