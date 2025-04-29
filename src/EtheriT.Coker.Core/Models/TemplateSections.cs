using EtheriT.Coker.Application.Shared.Dto.enumType.Template;
using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class TemplateSections: FullAuditedEntity
    {
        public long FK_TemplateID { get; set; }
        public SectionTypeEnum sectionType { get; set; }
        public string ContentConfig { get; set; } = string.Empty;
        public Template template { get; set; }
        public FooterTemplate? footerTemplates { get; set; }
    }
}
