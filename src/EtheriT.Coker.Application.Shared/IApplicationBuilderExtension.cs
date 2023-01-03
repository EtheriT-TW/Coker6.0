using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application
{
    public static class IApplicationBuilderExtension
    {
        public static void UseVirtualDirectory(this IApplicationBuilder me, string virtualDirectory, string physicalDirectory)
        {
            me.Use(
                async (ctx, next) =>
                {
                    // 比對 Request Path
                    var match = Regex.Match(ctx.Request.Path.Value, $"^/{virtualDirectory}/(.+)", RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        // 用實體目錄路錄建立 FileProvider
                        var fileProvider = new PhysicalFileProvider(physicalDirectory);

                        // 使用相對路徑取得 FileInfo
                        var fileInfo = fileProvider.GetFileInfo(match.Groups[1].Value);

                        if (fileInfo.Exists)
                        {
                            // 取得預設的 ContentType Mappings
                            var contentTypeProvider = new FileExtensionContentTypeProvider();

                            // 依據檔案的副檔名取得 ContentType
                            if (!contentTypeProvider.TryGetContentType(fileInfo.PhysicalPath, out var contentType))
                            {
                                contentType = "application/octet-stream";
                            }

                            ctx.Response.ContentType = contentType;

                            // 回應靜態檔案內容
                            await ctx.Response.SendFileAsync(fileInfo);
                        }
                        else
                        {
                            await next();
                        }
                    }
                    else
                    {
                        await next();
                    }
                });
        }
    }
}
