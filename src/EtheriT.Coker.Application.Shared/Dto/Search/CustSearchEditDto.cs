using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Application.Shared.Dto.Search;

public class CustSearchEditDto
{
    public long Id { get; set; }
    [Required, StringLength(60)] public string Title { get; set; } = string.Empty;
    [StringLength(150)] public string? Description { get; set; }
    [StringLength(100)] public string? Placeholder { get; set; }
    public int SerNO { get; set; }
    public bool Visible { get; set; } = true;
    public bool IsDefault { get; set; }
    public bool IsSystem { get; set; }
    public bool IsAvailable { get; set; } = true;
    public List<CustSearchOptionDto> SelectedTags { get; set; } = new();
    public bool SearchAllProd { get; set; }
    public bool SearchAllArticle { get; set; }
    public bool SearchAllMenu { get; set; }
    public List<long> ProductTagIds { get; set; } = new();
    public List<long> ArticleTagIds { get; set; } = new();
    public List<long> MenuIds { get; set; } = new();
}

public class CustSearchScopeDto
{
    public List<long> ProductTagIds { get; set; } = new();
    public List<long> ArticleTagIds { get; set; } = new();
    public List<long> MenuIds { get; set; } = new();
}

public class CustSearchOptionDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class CustSearchOptionsDto
{
    public bool HasProducts { get; set; }
    public List<CustSearchOptionDto> Tags { get; set; } = new();
    public List<CustSearchOptionDto> Menus { get; set; } = new();
}

public class CustSearchDeleteDto
{
    public long Id { get; set; }
}
