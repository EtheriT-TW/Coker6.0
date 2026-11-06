using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Contact;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Common;
using EtheriT.Coker.Application.Shared.Dto.Contact;
using EtheriT.Coker.Application.Shared.Dto.Mail;
using EtheriT.Coker.Application.Shared.Dto.Templates;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Templates;
using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task TemplateGlobal(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<ITemplatesApplicationService>();
        foreach (var show in new[] { false, true })
        {
            Success(await service.saveGlobalSettings(new GlobalSettingsDto { Visibility = new()
                { ShowMarquee = show, ShowPagePath = show, ShowPopular = show, ShowPublishDate = show } }));
            box.Db.ChangeTracker.Clear();
            var result = await service.getGlobalSettings(); Success(result);
            var value = JToken.FromObject(result.Object!).ToObject<GlobalSettingsDto>()!;
            Require(value.Visibility.ShowMarquee == show && value.Visibility.ShowPagePath == show &&
                value.Visibility.ShowPopular == show && value.Visibility.ShowPublishDate == show, "Global visibility save mismatch.");
        }
    }
    private static async Task TemplateHeader(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<ITemplatesApplicationService>();
        var input = new HeaderTemplateDto { ContentConfig = new() { ShowPagePath = false, ShowMarquee = false } };
        Success(await service.saveDefaultHeader(input)); box.Db.ChangeTracker.Clear();
        var result = await service.getDefaultHeader(); Success(result);
        var value = JToken.FromObject(result.Object!).ToObject<HeaderTemplateDto>()!;
        Require(!value.ContentConfig.ShowPagePath && !value.ContentConfig.ShowMarquee, "Header save mismatch.");
    }
    private static async Task TemplateFooter(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<ITemplatesApplicationService>();
        Success(await service.saveDefaultFooter(new MenuSaveContenDto { SaveHtml = "<p>diagnostics-footer</p>", SaveCss = "p{color:red}" }));
        box.Db.ChangeTracker.Clear();
        Require(await box.Db.FooterTemplates.AsNoTracking().AnyAsync(row => row.saveHtml != null && row.saveHtml.Contains("diagnostics-footer")), "Footer draft was not saved.");
    }
    private static async Task<List<Contact>> ContactFixtures(BackofficeSandbox box)
    {
        var menu = new WebMenu { FK_WebsiteId = 999000001, Title = "檢測表單", RouterName = "diagnostics-form" };
        box.Db.WebMenus.Add(menu); await box.Db.SaveChangesAsync();
        var contacts = Enumerable.Range(1, 3).Select(number => new Contact
        {
            FK_WebMenuId = menu.Id, Html = "<p>檢測詢問</p>", Name = "檢測表單", UserName = "檢測人員",
            Email = "recipient" + number + "@example.invalid", TargetEmail = "sender@example.invalid", Status = ContactStatusEnum.未處理
        }).ToList();
        box.Db.Contacts.AddRange(contacts); await box.Db.SaveChangesAsync(); box.Db.ChangeTracker.Clear();
        return contacts;
    }
    private static async Task ContactStatus(BackofficeSandbox box)
    {
        var contacts = await ContactFixtures(box);
        var service = box.Services.GetRequiredService<IContactAppService>();
        Require(!(await service.BatchUpdateStatus(new() { Ids = new(), Status = ContactStatusEnum.處理中 })).Success, "Empty batch selection was accepted.");
        Success(await service.BatchUpdateStatus(new() { Ids = contacts.Take(2).Select(row => row.Id).ToList(), Status = ContactStatusEnum.處理中 }));
        box.Db.ChangeTracker.Clear();
        var selected = contacts.Take(2).Select(row => row.Id).ToList();
        Require(await box.Db.Contacts.AsNoTracking().Where(row => selected.Contains(row.Id)).AllAsync(row => row.Status == ContactStatusEnum.處理中), "Batch status mismatch.");
        Require((await box.Db.Contacts.AsNoTracking().SingleAsync(row => row.Id == contacts[2].Id)).Status == ContactStatusEnum.未處理, "Unselected contact was changed.");
    }
    private static async Task ContactReply(BackofficeSandbox box)
    {
        var contacts = await ContactFixtures(box); var service = box.Services.GetRequiredService<IContactAppService>();
        var capture = DiagnosticsSandboxScope.Current!; var before = capture.Mail.Count;
        Success(await service.ReplyContact(new() { Id = contacts[0].Id, Status = ContactStatusEnum.已完成, Reply = "diagnostics-reply" }));
        box.Db.ChangeTracker.Clear();
        var saved = await box.Db.Contacts.AsNoTracking().SingleAsync(row => row.Id == contacts[0].Id);
        Require(saved.Reply == "diagnostics-reply" && saved.ReplyTime.HasValue && capture.Mail.Count == before + 1, "Reply persistence or mail generation failed.");
        Require(capture.Mail.Last().Recipients.SequenceEqual(new[] { contacts[0].Email }), "Reply recipient mismatch.");
        Require(!(await service.ReplyContact(new() { Id = contacts[0].Id, Status = ContactStatusEnum.處理中, Reply = "不應修改" })).Success, "Completed contact could be modified.");
    }
    private static async Task MailEnvelope(BackofficeSandbox box)
    {
        var capture = DiagnosticsSandboxScope.Current!;
        var before = capture.Mail.Count;
        var input = new SenderDto { Subject = "diagnostics-mail", TextBody = "測試文字", Body = "<p>測試內文</p>",
            Sender = new MailUserDataDto { Email = "sender@example.invalid", Name = "檢測寄件人" },
            Recipients = new() { new MailUserDataDto { Email = "recipient@example.invalid", Name = "檢測收件人" } } };
        Success(await box.Services.GetRequiredService<MailAppService>().sendSystemMail(input));
        Require(capture.Mail.Count == before + 1 && capture.Mail.Last().Subject == input.Subject &&
            capture.Mail.Last().Recipients.SequenceEqual(new[] { "recipient@example.invalid" }), "Mail envelope mismatch.");
    }
}
