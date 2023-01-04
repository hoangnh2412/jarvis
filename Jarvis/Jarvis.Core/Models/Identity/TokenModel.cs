using System;
using System.Collections.Generic;

namespace Jarvis.Models.Identity.Models.Identity
{
    public class TokenModel
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpireAt { get; set; }
        // public double ExpireIn { get; set; }
        public double Timezone { get; set; }
        public List<string> Roles { get; set; }
    }
}
