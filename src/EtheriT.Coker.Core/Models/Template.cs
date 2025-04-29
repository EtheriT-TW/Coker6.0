using EtheriT.Coker.Application.Shared.Dto.enumType.Template;
using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class Template: FullAuditedEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long FK_WebsiteID {  get; set; }
        public LayoutTypeEnum LayoutType { get; set; }
        public HeadTypeEnum HeadType { get; set; }
        public TemplateTypeEnum templateTypeEnum { get; set; }
        public long? FK_ThemeId { get; set; }
        public string LayoutConfig { get; set; } = string.Empty;
        public string Css { get; set; } = string.Empty;
        public bool Enable { get; set; }
        public Website Website { get; set; }
        public List<TemplateSections> templateSections { get; set; }
    }
}
