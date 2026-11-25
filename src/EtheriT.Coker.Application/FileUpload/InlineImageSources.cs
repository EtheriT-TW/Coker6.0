using System.Security.Cryptography;
using HtmlAgilityPack;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Dto.Files;

namespace EtheriT.Coker.Application;

public partial class FileUploadAppService
{
    private const int InlineImageMaxBytes = 10 * 1024 * 1024;
    private const int InlineImagesMaxBytes = 25 * 1024 * 1024;
    private const int InlineImageMaxCount = 50;

    // Called only after the owning content service has checked write permission.
    // No remote URL or browser-local blob is downloaded by this conversion.
    public async Task<string> NormalizeInlineImageSourcesAsync(string html)
    {
        if (string.IsNullOrEmpty(html)
            || (!html.Contains("data:", StringComparison.OrdinalIgnoreCase)
                && !html.Contains("blob:", StringComparison.OrdinalIgnoreCase))) return html;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var replacements = new List<(HtmlAttribute Attribute, string Key)>();
        var images = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        var decodedSources = new Dictionary<string, string>(StringComparer.Ordinal);
        long totalBytes = 0;
        foreach (var node in doc.DocumentNode.Descendants())
        {
            foreach (var attribute in node.Attributes)
            {
                if (!attribute.Name.Equals("src", StringComparison.OrdinalIgnoreCase)
                    && !attribute.Name.Equals("srcset", StringComparison.OrdinalIgnoreCase)) continue;
                var source = HtmlEntity.DeEntitize(attribute.Value).Trim();
                if (attribute.Name.Equals("srcset", StringComparison.OrdinalIgnoreCase))
                {
                    if (source.Contains("data:", StringComparison.OrdinalIgnoreCase)
                        || source.Contains("blob:", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("響應式圖片不可使用內嵌或暫存資料，請先上傳圖片。");
                    continue;
                }
                if (source.StartsWith("blob:", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("圖片使用瀏覽器暫存網址，請先上傳圖片再儲存。");
                if (!source.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) continue;
                if (replacements.Count >= InlineImageMaxCount)
                    throw new InvalidOperationException("單次最多可轉存 50 個內嵌圖片，請分批上傳。");
                if (!decodedSources.TryGetValue(source, out var key))
                {
                    var bytes = DecodeInlineRasterImage(source);
                    key = Convert.ToHexString(SHA256.HashData(bytes));
                    if (!images.ContainsKey(key))
                    {
                        totalBytes += bytes.Length;
                        if (totalBytes > InlineImagesMaxBytes)
                            throw new InvalidOperationException("單次內嵌圖片合計不可超過 25 MB，請先上傳圖片。");
                        images.Add(key, bytes);
                    }
                    decodedSources.Add(source, key);
                }
                replacements.Add((attribute, key));
            }
        }
        if (replacements.Count == 0) return html;

        // Validate ALL inputs before creating files; identical image bytes share one
        // site-scoped content name, including across retries and repeated saves.
        var websiteId = await loginUserData.GetWebsiteId();
        var orgName = await loginUserData.GetWebsiteOrgName();
        var paths = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in images)
        {
            var sourcePath = "inline-image:" + entry.Key;
            var sourceKey = CreateCanvasImageSourceKey(websiteId, sourcePath);
            var gate = AtomicFileLocks.GetOrAdd($"canvas-image-import:{websiteId}:{sourceKey:N}", _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync();
            try
            {
                var fileStem = "inline-image-" + entry.Key.ToLowerInvariant();
                var saved = await FindInlineImageByContentAsync(websiteId, orgName, fileStem, sourcePath)
                    ?? await FindExistingCanvasImportAsync(websiteId, orgName, sourcePath, sourceKey);
                if (saved == null)
                {
                    using var stream = new MemoryStream(entry.Value, writable: false);
                    // A recycled image may be reinserted later. Give a new upload its own
                    // identity; deduplication uses the content name, not a reused GuidKey.
                    saved = await SaveCanvasImageAsync(stream, fileStem, sourcePath, sourceKey, usePerUploadSourceKey: true);
                }
                paths.Add(entry.Key, saved.Path!);
            }
            finally { gate.Release(); }
        }
        foreach (var replacement in replacements)
            replacement.Attribute.Value = paths[replacement.Key];
        return doc.DocumentNode.OuterHtml;
    }

    private async Task<FileItemDto?> FindInlineImageByContentAsync(long websiteId, string orgName, string fileStem, string sourcePath)
    {
        var prefix = fileStem + ".";
        var files = await db.FileUploads.AsNoTracking()
            .Where(file => file.FK_WebsiteId == websiteId && !file.IsDeleted
                && file.ContentType.StartsWith("image/") && file.OriginalFileName.StartsWith(prefix))
            .OrderByDescending(file => file.Id).ToListAsync();
        foreach (var file in files)
        {
            // A user-supplied filename is not proof that its bytes match this digest.
            if (file.GuidKey != CreateCanvasImageSourceKey(websiteId, sourcePath + "#upload:" + file.Id)) continue;
            if (string.IsNullOrWhiteSpace(file.DownloadFileName)) continue;
            var physical = uploadPathResolver.GetPhysicalPathFromDownloadFileName(orgName, file.DownloadFileName);
            if (File.Exists(physical))
                return new FileItemDto { Id = file.Id, Guid = file.GuidKey,
                    Name = file.OriginalFileName, Path = ApplyOrgToUploadPath(file.DownloadFileName, orgName) };
        }
        return null;
    }

    private static byte[] DecodeInlineRasterImage(string source)
    {
        var comma = source.IndexOf(',');
        if (comma < 0 || comma > 128)
            throw new InvalidOperationException("內嵌圖片格式不完整，請重新上傳圖片。");
        var header = source[5..comma].Split(';');
        var mime = header[0].Trim().ToLowerInvariant();
        if (mime is not ("image/png" or "image/jpeg" or "image/jpg" or "image/gif" or "image/webp" or "image/bmp"))
            throw new InvalidOperationException("內嵌圖片只支援 PNG、JPEG、GIF、WebP 或 BMP；其他格式請先轉成圖檔上傳。");
        var encoded = source[(comma + 1)..];
        // Percent escapes can expand each Base64 byte to three characters.
        if (encoded.Length > ((InlineImageMaxBytes + 2L) / 3 * 4 + 1024) * 3)
            throw new InvalidOperationException("單張內嵌圖片不可超過 10 MB，請先上傳圖片。");
        byte[] bytes;
        try
        {
            if (header.Skip(1).Any(value => value.Equals("base64", StringComparison.OrdinalIgnoreCase)))
            {
                var base64 = Uri.UnescapeDataString(encoded);
                if (base64.Length > (InlineImageMaxBytes + 2L) / 3 * 4 + 1024)
                    throw new InvalidOperationException("單張內嵌圖片不可超過 10 MB，請先上傳圖片。");
                bytes = Convert.FromBase64String(base64);
            }
            else
            {
                // Decode data:image/...,%89PNG... as raw octets, never UTF-8 text.
                using var decoded = new MemoryStream();
                for (var i = 0; i < encoded.Length; i++)
                {
                    if (encoded[i] == '%')
                    {
                        if (i + 2 >= encoded.Length) throw new FormatException();
                        decoded.WriteByte(Convert.ToByte(encoded.Substring(i + 1, 2), 16));
                        i += 2;
                    }
                    else if (encoded[i] <= 127) decoded.WriteByte((byte)encoded[i]);
                    else throw new FormatException();
                    if (decoded.Length > InlineImageMaxBytes)
                        throw new InvalidOperationException("單張內嵌圖片不可超過 10 MB，請先上傳圖片。");
                }
                bytes = decoded.ToArray();
            }
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            throw new InvalidOperationException("內嵌圖片編碼無效，請重新上傳圖片。");
        }
        if (bytes.Length == 0 || bytes.Length > InlineImageMaxBytes)
            throw new InvalidOperationException("內嵌圖片為空或超過單張 10 MB 限制。");

        // Check raster signatures before ImageMagick sees the bytes, preventing
        // SVG/XML or other document formats from invoking a different decoder.
        var actualMime = bytes.Length >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) ? "image/png"
            : bytes.Length >= 3 && bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255 ? "image/jpeg"
            : bytes.Length >= 6 && (System.Text.Encoding.ASCII.GetString(bytes, 0, 6) is "GIF87a" or "GIF89a") ? "image/gif"
            : bytes.Length >= 12 && System.Text.Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP" ? "image/webp"
            : bytes.Length >= 2 && bytes[0] == 66 && bytes[1] == 77 ? "image/bmp" : "";
        if (actualMime != (mime == "image/jpg" ? "image/jpeg" : mime))
            throw new InvalidOperationException("內嵌圖片內容與宣告格式不符，請重新上傳圖片。");
        try
        {
            var info = new MagickImageInfo(bytes);
            if (info.Width == 0 || info.Height == 0 || (ulong)info.Width * info.Height > 40000000)
                throw new InvalidOperationException("內嵌圖片尺寸過大，請縮小後重新上傳。");
        }
        catch (MagickException)
        {
            throw new InvalidOperationException("內嵌圖片內容損壞，請重新上傳圖片。");
        }
        return bytes;
    }
}
