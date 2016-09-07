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

        #region 操作类型枚举
        /// <summary>
        /// 操作类型枚举
        /// </summary>
        public enum OperateType
        {
            /// <summary>
            /// 查看
            /// </summary>
            view,
            /// <summary>
            /// 登录
            /// </summary>
            login,
            /// <summary>
            /// 新增
            /// </summary>
            add,
            /// <summary>
            /// 修改
            /// </summary>
            modify,
            /// <summary>
            /// 删除
            /// </summary>
            delete
        }
        #endregion

        #region 记录日志
        #region 记录日志到数据库（用户日志表）
        /// <summary>
        /// 记录日志到数据库
        /// </summary>
        /// <param name="strTxt">内容</param>
        public void WriteLogToDB(string strTxt, OperateType Otp, string Ip)
        {
            try
            {
                using (wxEntities context = new wxEntities())
                {
                    T_logs _log = context.T_logs.Create();
                    _log.LogId = Guid.NewGuid().ToString("N");
                    _log.OperateContent = strTxt;
                    _log.CreateTime = DateTime.Now;
                    _log.OperateType = Otp.ToString();
                    _log.UserIP = Ip;
                    context.T_logs.Add(_log);
                    context.SaveChanges();
                }
            }
            catch
            {
            }
        }



        #endregion
        #region 记录日志到数据库（系统日志）
        /// <summary>
        /// 记录系统日志
        /// </summary>
        public void WriteSysLogToDB(string strContent)
        {
            try
            {
                using (wxEntities context = new wxEntities())
                {
                    T_SysLogs _log = context.T_SysLogs.Create();
                    _log.SysLogId = Guid.NewGuid().ToString("N");
                    _log.SysContent = strContent;
                    _log.CreateTime = DateTime.Now;
                    context.T_SysLogs.Add(_log);
                    context.SaveChanges();
                }
            }
            catch
            {
            }
        }
        #endregion
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

        #region 获取设置
        private KeyValuePair<string, string> GetAppConfig() {
            KeyValuePair<string, string> kv=new KeyValuePair<string, string>("","");
            try
            {
                using (wxEntities context = new wxEntities())
                {
                    string appID = context.T_Setting.Where(s => s.IsDeleted == false && s.SettingKey == "AppID").FirstOrDefault().SettingValue,
                        appSecret = context.T_Setting.Where(s => s.IsDeleted == false && s.SettingKey == "AppSecret").FirstOrDefault().SettingValue;

                    kv = new KeyValuePair<string, string>(appID, appSecret);
                }
            }
            catch (Exception ex)
            {
                
            }
            return kv;
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
                            var kv=GetAppConfig();
                            string _SYSID = kv.Key,
                                   _SYSSecret = kv.Value;
                            string GET_URL =
                            string.Format(new wxCOM.WXApiUrl().Dic_WXUrls["GetAccess_token"], _SYSID, _SYSSecret);
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

                                        WriteSysLogToDB(
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