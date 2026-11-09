using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Advertise;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Directory;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Freight;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Dto.UserHabits;
using EtheriT.Coker.Application.Shared.Freight;
using EtheriT.Coker.Application.Shared.Member;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.UserHabits;
using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task Crud<TEntity, TInput>(BackofficeSandbox box, TInput input,
        Func<TInput, Task<ResponseMessageDto>> save, Func<long, Task<ResponseMessageDto>> delete,
        Action<TInput, long> mutate, Func<TEntity, string?> name) where TEntity : FullAuditedEntity
    {
        var before = (await box.Db.Set<TEntity>().IgnoreQueryFilters().Select(row => row.Id).ToListAsync()).ToHashSet();
        Success(await save(input)); box.Db.ChangeTracker.Clear();
        var created = await box.Db.Set<TEntity>().IgnoreQueryFilters().AsNoTracking().Where(row => !before.Contains(row.Id)).ToListAsync();
        Require(created.Count == 1, "Create must persist exactly one fixture record.");
        var id = created[0].Id;
        mutate(input, id);
        Success(await save(input)); box.Db.ChangeTracker.Clear();
        var updated = await box.Db.Set<TEntity>().IgnoreQueryFilters().AsNoTracking().SingleAsync(row => row.Id == id);
        Require(name(updated) == "更新檢測資料", "Update did not persist expected data.");
        Success(await delete(id)); box.Db.ChangeTracker.Clear();
        var removed = await box.Db.Set<TEntity>().IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(row => row.Id == id);
        Require(removed == null || removed.IsDeleted, "Delete did not remove or soft-delete the fixture.");
    }

    private static Task CertificateCrud(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<ITechnicalCertificateAppService>();
        return Crud<TechnicalCertificate, TechCertDto>(box,
            new() { Title = "檢測證照", Description = "檢測說明", Img = "", Permanent = true, Disp_opt = true },
            service.AddUp, service.Delete, (input, id) => { input.Id = id; input.Title = "更新檢測資料"; }, row => row.Title);
    }
    private static Task DirectoryCrud(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<IDirectoryAppService>();
        return Crud<EtheriT.Coker.Core.Models.Directory, DirectoryAddUpDto>(box,
            new() { Id = 0, Title = "檢測目錄", Description = "檢測說明", Type = 1, Visible = true },
            service.AddUp, service.Delete, (input, id) => { input.Id = id; input.Title = "更新檢測資料"; }, row => row.Title);
    }
    private static Task ArticleCrud(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<IArticleAppService>();
        return Crud<Article, ArticleDto>(box,
            new() { Title = "檢測文章", Description = "檢測說明", Visible = true, permanent = true, TagSelected = new() },
            service.AddUp, service.Delete, (input, id) => { input.Id = id; input.Title = "更新檢測資料"; }, row => row.Title);
    }
    private static Task AdvertiseCrud(BackofficeSandbox box, int type)
    {
        var service = box.Services.GetRequiredService<IAdvertiseAppService>();
        return Crud<Advertise, AdvertiseDto>(box,
            new() { Title = "檢測廣告", Describe = "檢測說明", Type = type, Visible = true, permanent = true, TagSelected = new() },
            service.AddUp, service.Delete, (input, id) => { input.Id = id; input.Title = "更新檢測資料"; }, row => row.Title);
    }
    private static async Task TagCrud(BackofficeSandbox box, bool group)
    {
        var service = box.Services.GetRequiredService<ITagAppService>();
        var input = new DevExpressDto { Values = JsonConvert.SerializeObject(new { Title = "檢測標籤", Disp_Opt = true, FK_Tid = new long[0] }) };
        void Mutate(DevExpressDto data, long id)
        { data.Key = checked((int)id); data.Values = JsonConvert.SerializeObject(new { Title = "更新檢測資料", Disp_Opt = true, FK_Tid = new long[0] }); }
        if (group) await Crud<Tag_Group, DevExpressDto>(box, input, service.TagGroupAddUp, service.TagGroupDelete, Mutate, row => row.Title);
        else await Crud<Tag, DevExpressDto>(box, input, service.TagAddUp, service.TagDelete, Mutate, row => row.Title);
    }
    private static Task UserGroupCrud(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<IUserHabitsAppService>();
        return Crud<UserGrouping, UserGroupAddUpInputDto>(box,
            new() { Title = "檢測分群", Description = "檢測說明", Enable = true }, service.AddUpUserGroup,
            service.DeleteUserGroup, (input, id) => { input.Id = id; input.Title = "更新檢測資料"; }, row => row.Title);
    }
    private static async Task MemberRoleCrud(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<IMemberAppService>();
        // The first role is a default role and must not be used as the deletion fixture.
        Success(await service.RoleAddUp(new DevExpressDto { Values = "{\"Name\":\"固定預設角色\"}" }));
        await Crud<Role, DevExpressDto>(box, new() { Values = "{\"Name\":\"檢測會員角色\"}" },
            service.RoleAddUp, service.RoleDelete, (input, id) => { input.Key = checked((int)id); input.Values = "{\"Name\":\"更新檢測資料\"}"; }, row => row.Name);
    }
    private static Task BoxCrud(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<IFreightAppService>();
        return Crud<LogisticsBox, GetLogisticsBoxAllListInputDto>(box,
            new() { Name = "檢測箱型", CapacityPoint = 10, IsActive = true, Sort = 1 }, service.LogisticsBoxAddUp,
            service.LogisticsBoxDelete, (input, id) => { input.Id = id; input.Name = "更新檢測資料"; }, row => row.Name);
    }
}
