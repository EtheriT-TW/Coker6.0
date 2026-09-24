using System.Collections.Generic;
using EtheriT.Coker.Application.Dto;

namespace EtheriT.Coker.Application.Dto.Files
{
    public class CanvasImageInspectInputDto
    {
        public List<string> Paths { get; set; } = new();
    }

    public class CanvasImageInspectOutputDto : ResponseMessageDto
    {
        public List<CanvasImageSourceDto> Items { get; set; } = new();
    }

    public class CanvasImageImportInputDto
    {
        public List<string> Paths { get; set; } = new();
    }

    public class CanvasImageSourceDto
    {
        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public long? Size { get; set; }
        public bool IsInternal { get; set; }
        public bool Exists { get; set; }
        public bool CanImport { get; set; }
        public string? Error { get; set; }
    }
}
