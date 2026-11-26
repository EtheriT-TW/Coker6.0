using EtheriT.Coker.Application.Shared.Dto.ArticleImport;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EtheriT.Coker.Application.ArticleImports;

// ArticleImportRow.DownloadedFilesJson 的一筆。Created = true 表示這次匯入才存的檔案（取消時要刪）；
// false 表示沿用之前匯入過的同一個 Drive 檔案（可能已被文章使用，不能刪）。
public sealed record ArticleImportImageRecord(
    string Token,
    string Column,
    string FileName,
    long? FileId,
    string? Path,
    bool Created,
    string? Error);

public static class ArticleImportRowImages
{
    public static List<ArticleImportImageRecord> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            return JsonConvert.DeserializeObject<List<ArticleImportImageRecord>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public static string Serialize(IReadOnlyList<ArticleImportImageRecord> images) => JsonConvert.SerializeObject(images);

    internal static List<ArticleImportImageChoice> Choices(IEnumerable<ArticleImportImageRecord> images)
        => images.Where(i => i.Error == null && !string.IsNullOrEmpty(i.Path))
            .Select(i => new ArticleImportImageChoice(i.Path!, i.Column, i.FileName))
            .ToList();

    public static ArticleImportImageDto ToDto(ArticleImportImageRecord image)
        => new(image.Token, image.Column, image.FileName, image.FileId, image.Path, image.Error);
}

// 把匯入批次中「沒有變成文章」的列所下載的圖片移到資源回收桶。
// 同一張圖如果也被已匯入的列使用、或已經被任何內容引用（FileReferences），就保留。
public sealed class ArticleImportFileCleaner(CokerDbContext db, IArticleImportImageStore images)
{
    public async Task<int> RecycleUnusedAsync(long batchId)
    {
        var rows = await db.ArticleImportRows.AsNoTracking()
            .Where(r => r.FK_ArticleImportBatchId == batchId && !r.IsDeleted)
            .Select(r => new { r.Status, r.DownloadedFilesJson })
            .ToListAsync();

        var kept = rows.Where(r => r.Status == ArticleImportRowStatusEnum.Imported)
            .SelectMany(r => ArticleImportRowImages.Parse(r.DownloadedFilesJson))
            .Select(i => i.FileId)
            .OfType<long>()
            .ToHashSet();
        var candidates = rows.Where(r => r.Status != ArticleImportRowStatusEnum.Imported)
            .SelectMany(r => ArticleImportRowImages.Parse(r.DownloadedFilesJson))
            .Where(i => i.Created && i.FileId is > 0 && !kept.Contains(i.FileId.Value))
            .Select(i => i.FileId!.Value)
            .Distinct()
            .ToList();
        if (candidates.Count == 0) return 0;

        var referenced = await db.FileReferences.AsNoTracking()
            .Where(r => !r.IsDeleted && r.FK_FileUploadId != null && candidates.Contains(r.FK_FileUploadId.Value))
            .Select(r => r.FK_FileUploadId!.Value)
            .Distinct()
            .ToListAsync();

        var recycled = 0;
        foreach (var fileId in candidates.Except(referenced))
        {
            try
            {
                await images.RecycleAsync(fileId);
                recycled++;
            }
            catch (Exception)
            {
                // 單一檔案移除失敗不影響其他檔案；留在上傳目錄，之後可用檔案清理處理。
                db.ChangeTracker.Clear();
            }
        }
        return recycled;
    }
}