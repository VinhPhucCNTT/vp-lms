using System.Buffers;
using System.Security.Cryptography;
using Backend.Api.Core.Entities.Content;
using Backend.Api.Core.Types;
using Backend.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Api.Services.Content;

public class FileService(
    IOptions<Core.Common.FileOptions> fileOptions,
    IDbContextFactory<AppDbContext> dbFactory)
{
    private readonly string _root = fileOptions.Value.RootPath;
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

    public async Task<long> UploadAsync(
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

        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var file = new FileAsset
        {
            UserId = userId,
            OriginalFileName = originalFileName,
            StoragePath = relativePath.Replace('\\', '/'),
            ContentType = contentType,
            SizeInBytes = size,
            Sha256Hash = hash
        };
        db.FileAssets.Add(file);
        await db.SaveChangesAsync(ct);

        return file.Id;
    }

    public Task<Stream> ReadAsync(string storagePath)
    {
        var path = Path.Combine(_root, storagePath);
        Stream stream = File.OpenRead(path);
        return Task.FromResult(stream);
    }

    public async Task<IResult?> GetFileAsync(long fileId, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var file = await db.FileAssets
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == fileId, ct);
        if (file is null) return null;

        var path = Path.Combine(_root, file.StoragePath);
        Stream stream = File.OpenRead(path);

        return Results.File(
            stream,
            file.ContentType,
            enableRangeProcessing: true);
    }

    public async Task DeleteAsync(long fileId, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var file = await db.FileAssets
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == fileId, ct);
        if (file is null) return;

        var path = Path.Combine(_root, file.StoragePath);

        if (File.Exists(path))
            File.Delete(path);

        db.FileAssets.Remove(file);
        await db.SaveChangesAsync(ct);
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
