using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Application.Shared.FileManagement;
using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static void FilePathBoundary(BackofficeSandbox box)
    {
        var paths = box.Services.GetRequiredService<IUploadPathResolver>();
        var root = Path.GetFullPath(paths.GetRootPath("diagnostics"));
        var safe = Path.GetFullPath(paths.GetPhysicalPath("diagnostics", "files/probe.txt"));
        Require(safe.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase), "A normal file path escaped the upload root.");
        foreach (var input in new[] { "../outside.txt", "../" + Path.GetFileName(root) + "-other/outside.txt" })
        {
            var rejected = false;
            try { paths.GetPhysicalPath("diagnostics", input); }
            catch { rejected = true; }
            Require(rejected, "A traversal or sibling-prefix path was accepted.");
        }
    }

    private static async Task FileRecycle(BackofficeSandbox box)
    {
        var paths = box.Services.GetRequiredService<IUploadPathResolver>();
        var path = paths.GetPhysicalPath("diagnostics", "diagnostics.txt");
        await File.WriteAllTextAsync(path, "diagnostics-file");
        var file = new FileUpload
        {
            FK_WebsiteId = 999000001, GuidKey = Guid.NewGuid(), ContentType = "text/plain",
            OriginalFileName = "diagnostics.txt", DownloadFileName = "/upload/diagnostics/diagnostics.txt", Size = 16
        };
        box.Db.FileUploads.Add(file); await box.Db.SaveChangesAsync();
        var id = file.Id;
        box.Db.FileCleanupCandidates.Add(new FileCleanupCandidate
        {
            FK_WebsiteId = 999000001, FK_FileUploadId = id, Reason = "檢測未引用檔案",
            FirstDetectedTime = DateTime.Now, LastConfirmedTime = DateTime.Now
        });
        await box.Db.SaveChangesAsync(); box.Db.ChangeTracker.Clear();
        var service = box.Services.GetRequiredService<IFileManagementAppService>();
        await service.MoveToRecycleBinAsync(id); box.Db.ChangeTracker.Clear();
        Require(!File.Exists(path) && (await service.GetRecycleBinFilesAsync()).Any(row => row.Id == id), "File was not moved to the recycle bin.");
        await service.RestoreFromRecycleBinAsync(id); box.Db.ChangeTracker.Clear();
        Require(File.Exists(path) && await File.ReadAllTextAsync(path) == "diagnostics-file", "Restored file contents differ.");
        Require(!(await box.Db.FileUploads.IgnoreQueryFilters().AsNoTracking().SingleAsync(row => row.Id == id)).IsDeleted, "Restored file remains deleted.");
        await service.MoveToRecycleBinAsync(id);
        await service.PermanentlyDeleteAsync(id); box.Db.ChangeTracker.Clear();
        Require(!File.Exists(path) && !(await service.GetRecycleBinFilesAsync()).Any(row => row.Id == id), "File was not permanently deleted.");
    }
}
