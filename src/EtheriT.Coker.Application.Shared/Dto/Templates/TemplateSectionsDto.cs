using EtheriT.Coker.Application.Shared.Dto.enumType.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Templates
{
    public class TemplateSectionsDto: ResponseObject
    {
        public long Id { get; set; }
        public long FK_TemplateID { get; set; }
        public SectionTypeEnum sectionType { get; set; }
        public string ContentConfig { get; set; } = string.Empty;
        private FooterTemplateDto? _footer;
        public FooterTemplateDto? footerTemplateDto
        {
            get => sectionType == SectionTypeEnum.頁尾 ? _footer : null;
            set => _footer = value;
        }
    }
}
