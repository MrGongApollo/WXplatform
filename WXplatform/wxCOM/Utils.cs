using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Web;
using wxBIZ;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace wxCOM
{
   public class Utils
    {
       private static readonly object LockTokenObj = new object();

        #region 微信xml回复消息格式
        public Dictionary<string, string> Dic_XML_retMsg = new Dictionary<string, string> { 
         ///返回图文消息项
        {"Message_News_Item",@"<item><Title><![CDATA[{0}]]></Title><Description><![CDATA[{1}]]></Description><PicUrl><![CDATA[{2}]]></PicUrl><Url><![CDATA[{3}]]></Url></item>"}
        /// 返回图文消息主体
        ,{"Message_News_Main",@"<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>{2}</CreateTime><MsgType><![CDATA[news]]></MsgType><ArticleCount>{3}</ArticleCount><Articles>{4}</Articles></xml> "}
        };
        #endregion

        #region 解析xml消息类型
        public static T ConvertObj<T>(string xmlstr)
        {
            XElement xdoc = XElement.Parse(xmlstr);
            var type = typeof(T);
            var t = Activator.CreateInstance<T>();
            foreach (XElement element in xdoc.Elements())
            {
                var pr = type.GetProperty(element.Name.ToString());
                if (element.HasElements)
                {//这里主要是兼容微信新添加的菜单类型。nnd，竟然有子属性，所以这里就做了个子属性的处理
                    foreach (var ele in element.Elements())
                    {
                        pr = type.GetProperty(ele.Name.ToString());
                        pr.SetValue(t, Convert.ChangeType(ele.Value, pr.PropertyType), null);
                    }
                    continue;
                }
                if (pr.PropertyType.Name == "MsgType")//获取消息模型
                {
                    pr.SetValue(t, (MsgType)Enum.Parse(typeof(MsgType), element.Value.ToUpper()), null);
                    continue;
                }
                if (pr.PropertyType.Name == "Event")//获取事件类型。
                {
                    pr.SetValue(t, (EventType)Enum.Parse(typeof(EventType), element.Value.ToUpper()), null);
                    continue;
                }
                pr.SetValue(t, Convert.ChangeType(element.Value, pr.PropertyType), null);
            }
            return t;
        }
        #endregion

        #region 回复消息
        /// <summary>
        /// 回复消息
        /// </summary>
        /// <param name="msg"></param>
        public static void ResponseWrite(string ToUserName, string strContent)
        {
            string responseContent = FormatTextXML(ToUserName, strContent);
            HttpContext.Current.Response.ContentType = "text/xml";
            HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
            HttpContext.Current.Response.Write(responseContent);
            HttpContext.Current.Response.End();
        }
        #endregion

        #region 返回格式化的Xml格式内容
        /// <summary>
        /// 返回格式化的Xml格式内容
        /// </summary>
        /// <param name="ToUserName">公众号</param>
        /// <param name="FromUserName">用户号</param>
        /// <param name="Content">回复内容</param>
        /// <returns></returns>
        private static string FormatTextXML(string ToUserName, string Content)
        {
            return "<xml><ToUserName><![CDATA[" + ToUserName + "]]></ToUserName><FromUserName><![CDATA[" + GetOpenID() + "]]></FromUserName><CreateTime>" + DateTime.Now.Subtract(new DateTime(1970, 1, 1, 8, 0, 0)).TotalSeconds.ToString() + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[" + Content + "]]></Content><FuncFlag>1</FuncFlag></xml>";
        }
        #endregion

        #region 获取OpenID
        /// <summary>
        /// OpenID
        /// </summary>
        /// <returns></returns>
        private static string GetOpenID()
        {
            string openId = string.Empty;
            try
            {
                using (wxEntities context = new wxEntities())
                {
                    openId = context.T_Setting.Where(s => s.IsDeleted == false && s.SettingKey == "OpenID").FirstOrDefault().SettingValue;
                }
            }
            catch (Exception)
            {

            }
            return openId;
        }
        #endregion

        #region 发送post请求
        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="url">Url地址</param>
        /// <param name="method">方法（post或get）</param>
        /// <param name="method">数据类型</param>
        /// <param name="requestData">数据</param>
        public static string SendPostHttpRequest(string url, string requestData, string contentType = "application/x-www-form-urlencoded")
        {
            WebRequest request = (WebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            byte[] postBytes = null;
            request.ContentType = contentType;
            postBytes = Encoding.UTF8.GetBytes(requestData);
            request.ContentLength = postBytes.Length;
            using (Stream outstream = request.GetRequestStream())
            {
                outstream.Write(postBytes, 0, postBytes.Length);
            }
            string result = string.Empty;
            using (WebResponse response = request.GetResponse())
            {
                if (response != null)
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            result = reader.ReadToEnd();
                        }
                    }

                }
            }
            return result;
        }
        #endregion

        #region 发送get请求
        /// <summary>
        /// 发送get请求
        /// </summary>
        /// <param name="url">Url地址</param>
        /// <param name="method">方法（post或get）</param>
        /// <param name="method">数据类型</param>
        /// <param name="requestData">数据</param>
        public static string SendGetHttpRequest(string url, string contentType = "application/x-www-form-urlencoded")
        {
            WebRequest request = (WebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = contentType;
            string result = string.Empty;
            using (WebResponse response = request.GetResponse())
            {
                if (response != null)
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
            }
            return result;
        }
        #endregion

        #region 获取设置
        public KeyValuePair<string, string> GetAppConfig()
        {
            KeyValuePair<string, string> kv = new KeyValuePair<string, string>("", "");
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

        #region 获取access_token
        public string GetAccessToken()
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
                            var kv = GetAppConfig();
                            string _SYSID = kv.Key,
                                   _SYSSecret = kv.Value;
                            string GET_URL =
                            string.Format(new wxCOM.WXApiUrl().Dic_WXUrls[wxCOM.WXApiUrl.Enum_WXUrls.GetAccess_token], _SYSID, _SYSSecret);
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

        #region 记录日志
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
    }
}


