using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using wxBIZ;
using System.Web.Security;
using System.IO;
using System.Text;
using System.Net;
using Newtonsoft;
using Newtonsoft.Json;
using System.Xml.Linq;
using wxCOM;
using CommonHelp=WXplatform.Areas.weixin.Help;

namespace WXplatform.Areas.weixin.Controllers
{

    public class wxController : Controller
    {
        

        #region Index
        //
        // GET: /weixin/wx/
        [HttpGet]
        public void Index()
        {
            //GetAccessToken();
            Valid();
        }
        #endregion

        

        #region 微信验证
        #region checkSignature
        //
        // GET: /weixin/wx/
        private bool checkSignature()
        {
            bool ret = false;
            try
            {
                using (wxEntities context = new wxEntities())
                {
                    var _v = context.T_Setting.Where(k => k.SettingKey == "Token" && k.IsDeleted == false).FirstOrDefault();
                    CommonHelp.CommonHelper _help = new CommonHelp.CommonHelper();
                    if (_v != null)
                    {
                        //从微信服务器接收传递过来的数据
                        string signature = Request.QueryString["signature"].ToString(), //微信加密签名
                               timestamp = Request.QueryString["timestamp"].ToString(),//时间戳
                               nonce = Request.QueryString["nonce"].ToString();//随机数
                        string[] ArrTmp = { _v.SettingValue, timestamp, nonce };
                        Array.Sort(ArrTmp);     //字典排序
                        string tmpStr = string.Join("", ArrTmp);//将三个字符串组成一个字符串
                        tmpStr = _help.SHA1(tmpStr).ToLower(); //进行sha1加密  FormsAuthentication.HashPasswordForStoringInConfigFile(tmpStr, "SHA1")
                        //加过密的字符串与微信发送的signature进行比较，一样则通过微信验证，否则失败。
                        ret = tmpStr == signature;
                        T_logs _log = context.T_logs.Create();
                        _log.LogId = Guid.NewGuid().ToString("N");
                        _log.Content = string.Format("验证{0}！微信加密签名：{1}，时间戳{2}，随机数{2}", ret ? "成功" : "失败", signature, timestamp, nonce);
                        _log.CreateTime = DateTime.Now;
                        _log.UserIP = _help.getIp();
                        context.T_logs.Add(_log);
                        context.SaveChanges();
                    }
                }
            }
            catch
            {

            }
            return ret;
        }
        #endregion
        #region Valid
        private void Valid()
        {
            try
            {
                string echoStr = Request.QueryString["echoStr"].ToString();
                if (checkSignature())
                {
                    if (!string.IsNullOrEmpty(echoStr))
                    {
                        //Session["IsRunWx"] = true;
                        Response.Write(echoStr);
                        Response.End();
                    }
                }
            }
            catch
            {

            }
        }
        #endregion
        #endregion


        #region 获取推送事件
        
        #endregion

        #region 返回来的数据
        /// <summary>
        /// 获取post返回来的数据
        /// </summary>
        /// <returns></returns>
        private string PostInput()
        {
            Stream s = System.Web.HttpContext.Current.Request.InputStream;
            byte[] b = new byte[s.Length];
            s.Read(b, 0, (int)s.Length);
            return Encoding.UTF8.GetString(b);
        }
        #endregion

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string postStr = "";
            Valid();
            if (Request.HttpMethod.ToLower() == "post")//当普通微信用户向公众账号发消息时，微信服务器将POST该消息到填写的URL上
            {
                postStr = PostInput();
                if (!string.IsNullOrEmpty(postStr))
                {
                    new CommonHelp.CommonHelper().WriteLogToDB(postStr);//记录日志
                    //ResponseMsg(postStr);
                }
            }
        }

        

        #region 返回的JSON处理字符串
        /// <summary>  
        /// 返回JSon数据  
        /// </summary>  
        /// <param name="JSONData">要处理的JSON数据</param>  
        /// <param name="Url">要提交的URL</param>  
        /// <returns>返回的JSON处理字符串</returns>  
        public string GetResponseData(string JSONData, string Url)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(JSONData);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentLength = bytes.Length;
            request.ContentType = "text/xml";
            Stream reqstream = request.GetRequestStream();
            reqstream.Write(bytes, 0, bytes.Length);

            //声明一个HttpWebRequest请求  
            request.Timeout = 90000;
            //设置连接超时时间  
            request.Headers.Set("Pragma", "no-cache");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream streamReceive = response.GetResponseStream();
            Encoding encoding = Encoding.UTF8;

            StreamReader streamReader = new StreamReader(streamReceive, encoding);
            string strResult = streamReader.ReadToEnd();
            streamReceive.Dispose();
            streamReader.Dispose();

            return strResult;
        }
        #endregion

        
    }
}