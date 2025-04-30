using EtheriT.Coker.Application.Shared.Dto.enumType.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Templates
{
    public class TemplateSectionsDto
    {
        public long FK_TemplateID { get; set; }
        public SectionTypeEnum sectionType { get; set; }
        public string ContentConfig { get; set; } = string.Empty;
    }
}
