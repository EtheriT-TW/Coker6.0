using EtheriT.Coker.Application.Shared.Dto.Tag;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Article
{
    public class ArticleDto
    {
        public long? Id { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Description { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public bool Visible { get; set; }
        public int SerNO { get; set; } = 500;
        public bool PopularVisible { get; set; }
        public bool RemovedFromShelves { get; set; }
        public DateTime? NodeDate { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool permanent { get; set; }
        public List<TagSelectedDto> TagSelected { get; set; }
    }
}
