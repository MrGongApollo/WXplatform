using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using wxBIZ;
using wxCOM;

namespace WXplatform.Areas.weixin.Help
{
    public class CommonHelper
    {
        private static readonly object LockTokenObj = new object();

        #region 微信api 地址
        /// <summary>
        /// 微信api 调用地址
        /// </summary>
        public Dictionary<string, string> Dic_WXUrls = new Dictionary<string, string> { 
            {"Aauth2CheckAccessTokenUrl","https://api.weixin.qq.com/sns/auth?access_token={0}&openid={1}"},
            {"Aauth2Confirm","https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope={2}&state=STATE#wechat_redirect"},
            {"Aauth2GetAccessTokenUrl","https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code"},
            {"Aauth2GetWechatUserInfoUrl","https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}&lang=zh_CN"},
            {"Aauth2RefreshAccessTokenUrl","https://api.weixin.qq.com/sns/oauth2/refresh_token?appid={0}&grant_type=refresh_token&refresh_token={1}"},
            {"JsapiTicketUrl","https://api.weixin.qq.com/cgi-bin/ticket/getticket?type=jsapi&access_token={0}"},
            {"UnifiedorderUrl","https://api.mch.weixin.qq.com/pay/unifiedorder"}
        };
        #endregion
        

        #region 记录日志
        /// <summary>
        /// 记录日志到数据库
        /// </summary>
        /// <param name="strTxt">内容</param>
        public void WriteLogToDB(string strTxt)
        {
            try
            {
                using (wxEntities context = new wxEntities())
                {
                    T_logs _log = context.T_logs.Create();
                    _log.LogId = Guid.NewGuid().ToString("N");
                    _log.Content = strTxt;
                    _log.CreateTime = DateTime.Now;
                    _log.UserIP = getIp();
                    context.T_logs.Add(_log);
                    context.SaveChanges();
                }
            }
            catch
            {
            }
        }
        #endregion

        #region 获取IP地址
        /// <summary>
        /// 获取IP地址
        /// </summary>
        /// <returns></returns>
        public string getIp()
        {
            string ip = string.Empty;
            if (!string.IsNullOrEmpty(System.Web.HttpContext.Current.Request.ServerVariables["HTTP_VIA"]))
                ip = Convert.ToString(System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]);
            if (string.IsNullOrEmpty(ip))
                ip = Convert.ToString(System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]);
            return ip;
        }
        #endregion

        #region 发送消息
        /// <summary>
        /// 发送文字消息
        /// </summary>
        /// <param name="Message">消息内容</param>
        /// <param name="fakeid">用户fakeid</param>
        /// <returns></returns>        
        public bool SendMsgText(string Message, string fakeid)
        {
            string _token = GetAccessToken();
            bool result = false;
            string strMsg = System.Web.HttpUtility.UrlEncode(Message);
            string padate = "type=1&content=" + strMsg + "&error=false&tofakeid=" + fakeid + "&token=" + _token + "&ajax=1";
            string url = "https://mp.weixin.qq.com/cgi-bin/singlesend?t=ajax-response&lang=zh_CN";
            byte[] byteArray = Encoding.UTF8.GetBytes(padate); // 转化
            HttpWebRequest webRequest2 = (HttpWebRequest)WebRequest.Create(url);
            webRequest2.Referer = "https://mp.weixin.qq.com/cgi-bin/singlesendpage?t=message/send&action=index&tofakeid=" + fakeid + "&token=" + _token + "&lang=zh_CN";
            webRequest2.Method = "POST";
            webRequest2.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:2.0.1) Gecko/20100101 Firefox/4.0.1";
            webRequest2.ContentType = "application/x-www-form-urlencoded";
            webRequest2.ContentLength = byteArray.Length;
            Stream newStream = webRequest2.GetRequestStream();
            // Send the data. 
            newStream.Write(byteArray, 0, byteArray.Length); //写入参数 
            newStream.Close();
            HttpWebResponse response2 = (HttpWebResponse)webRequest2.GetResponse();
            StreamReader sr2 = new StreamReader(response2.GetResponseStream(), Encoding.Default);
            string text2 = sr2.ReadToEnd();
            if (text2.Contains("ok") || text2.Contains("10706") || text2.Contains("10703"))
            {
                result = true;
            }
            return result;
        }
        #endregion

        #region 获取access_token
        private string GetAccessToken()
        {
            string ret = string.Empty;
            try
            {
                using (wxEntities context = new wxEntities())
                {
                    lock (LockTokenObj)
                    {
                        var _AToken = context.T_Access_Token.Where(g => g.UnValidTime > DateTime.Now).Select(p => p.AccessToken).FirstOrDefault();
                        if (_AToken != null)
                        {
                            ret = _AToken;
                        }
                        else
                        {

                            Sys _sys = Sys.GetSingle();
                            string GET_URL =
                            string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", _sys.AppID, _sys.AppSecret);
                            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(GET_URL);
                            req.Method = "GET";
                            req.ContentType = "application/x-www-form-urlencoded";
                            using (WebResponse wr = req.GetResponse())
                            {
                                using (StreamReader sr = new StreamReader(wr.GetResponseStream()))
                                {
                                    string jsonstr = sr.ReadToEnd();
                                    #region 成功获取token
                                    if (jsonstr.IndexOf("access_token") > 0)
                                    {
                                        AccessToken _atkModel = JsonConvert.DeserializeObject<AccessToken>(jsonstr);
                                        T_Access_Token _atoken = context.T_Access_Token.Create();
                                        _atoken.AccessToken = _atkModel.access_token;
                                        _atoken.CreateTime = DateTime.Now;
                                        _atoken.UnValidTime = DateTime.Now.AddSeconds(_atkModel.expires_in);
                                        context.T_Access_Token.Add(_atoken);
                                        context.SaveChanges();
                                    }
                                    #endregion
                                    #region 返回错误信息
                                    else
                                    {
                                        RetMsg _rmsg = JsonConvert.DeserializeObject<RetMsg>(jsonstr);
                                        WriteLogToDB(
                                            string.Format("获取access_token出错，错误原因：{0}", RetMsg.DicWxRetMsg[_rmsg.errcode])
                                            );
                                    }
                                    #endregion
                                }
                            }


                        }
                    }
                }
            }
            catch
            {

            }
            return ret;
        }

        #endregion

        #region SHA1加密
        /// <summary>
        /// SHA1加密
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
         public string SHA1(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return txt;
            }
            byte[] cleanBytes = Encoding.Default.GetBytes(txt);
            byte[] hashedBytes = System.Security.Cryptography.SHA1.Create().ComputeHash(cleanBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "");
        }
        #endregion
       

    }
        
}