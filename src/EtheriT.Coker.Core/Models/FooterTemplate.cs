using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class FooterTemplate: FullAuditedEntity
    {
        public long FK_TemplateSectionsId { get; set; }
        public string? html { get; set; }
        public string? css { get; set; }
        public string? saveHtml { get; set; }
        public string? saveCss { get; set; }
        public TemplateSections templateSections { get; set; }
    }
}
