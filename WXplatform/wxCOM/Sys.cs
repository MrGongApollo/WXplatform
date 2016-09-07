using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wxCOM
{
    public class Sys
    {
        /// <summary>
        /// 初始化
        /// </summary>
        private Sys(string _appid, string secret)
        {
            this.AppID = _appid;
            this.AppSecret = secret;
        }
        private static Sys sys;
        public static Sys GetSingle()
        {
            return sys??new Sys("wxfa07619bf87bc096", "d4624c36b6795d1d99dcf0547af5443d");
        }

        public string AppID { get; set; }
        public string AppSecret { get; set; }
    }
}