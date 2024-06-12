
namespace EtheriT.Coker.Web.Public.Middlewares
{
    public class NotEqual : IRouteConstraint
    {
        private List<string> _match;
        public NotEqual(List<string> match)
        {
            _match = match;
        }

        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            return !_match.Exists(e => e == values[routeKey].ToString());
        }
    }
}
