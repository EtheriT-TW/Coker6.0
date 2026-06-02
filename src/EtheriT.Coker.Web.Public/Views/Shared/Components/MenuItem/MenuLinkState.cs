namespace EtheriT.Coker.Web.Public.Views.Shared.Components.MenuItem
{
    public class MenuLinkState
    {
        private readonly string? _link;
        private readonly List<MenuItemModel>? _children;

        public MenuLinkState(string? link, List<MenuItemModel>? children)
        {
            _link = link;
            _children = children;
        }
        public string Target(bool? target)
        {
            return HasLink && target == true ? "_blank" : "_self";
        }

        public string? Rel(bool? target)
        {
            return HasLink && target == true ? "noopener noreferrer" : null;
        }

        public bool HasChildren => _children != null && _children.Any();

        public bool HasLink => !string.IsNullOrWhiteSpace(_link) && _link != "#";

        public bool IsNoLink => !HasLink;

        public string Href => HasLink ? _link! : "#";

        public string CssClass => IsNoLink ? "is-no-link" : "";

        public string? Role => IsNoLink && HasChildren ? "button" : null;

        public string? AriaHasPopup => IsNoLink && HasChildren ? "true" : null;

        public string? AriaDisabled => IsNoLink && !HasChildren ? "true" : null;

        public string? TabIndex => IsNoLink && !HasChildren ? "-1" : null;
    }
}
