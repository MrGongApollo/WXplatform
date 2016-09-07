using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wxCOM
{
    public class AccessToken
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
    }
}