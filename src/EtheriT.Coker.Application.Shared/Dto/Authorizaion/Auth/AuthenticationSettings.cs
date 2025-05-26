using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Authorizaion.Auth
{
    public class AuthenticationSettings
    {
        public LineConfig Line { get; set; }
        public ProviderConfig Google { get; set; }
        public FacebookConfig Facebook { get; set; }
        public AppleConfig Apple { get; set; }
    }
}
