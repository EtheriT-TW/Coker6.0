namespace EtheriT.Coker.Application.Dto
{
    public class MenuEditorTreeDto : ResponseMessageDto
    {
        public List<MenuEditorTreeItemDto> Maps { get; set; } = new();
    }

    public class MenuEditorTreeItemDto : PowerOptionDto
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? text => Title;
        public string? icon { get; set; }
        public bool Visible { get; set; }
        public int SerNO { get; set; }
        public bool hasContan { get; set; }
        public long? FK_TopNodeId { get; set; }
        public long? FK_RootNodeId { get; set; }
        public bool HasBackstagePermission { get; set; }
        public bool HasFrontPermission { get; set; }
        public List<MenuEditorTreeItemDto>? Children { get; set; }
    }
}
