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
        
        #region 消息抛送地址
        public ActionResult Index()
        {
            try
            {
                if (Request.HttpMethod.ToLower() == "post")//当普通微信用户向公众账号发消息时，微信服务器将POST该消息到填写的URL上
                {
                    string postStr = PostInput();
                    if (!string.IsNullOrEmpty(postStr))
                    {
                        new CommonHelp.CommonHelper().WriteSysLogToDB(postStr);//记录日志
                        MessageFactory.CreateMessage(postStr);//分析xml
                        //ResponseMsg(postStr);
                    }
                }
                else if (Request.HttpMethod.ToLower() == "get")
                {
                    Valid();//微信首次验证
                }
            }
            catch (Exception ex)
            {
                new CommonHelp.CommonHelper().WriteSysLogToLocal(ex.Message);
            }
            return ViewBag;
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

                        _help.WriteLogToDB(string.Format("验证{0}！微信加密签名：{1}，时间戳{2}，随机数{2}", ret ? "成功" : "失败", signature, timestamp, nonce),
                          CommonHelp.CommonHelper.OperateType.add,new Utils().getIp());
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
        //protected override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    string postStr = "";
        //    Valid();
        //    if (Request.HttpMethod.ToLower() == "post")//当普通微信用户向公众账号发消息时，微信服务器将POST该消息到填写的URL上
        //    {
        //        postStr = PostInput();
        //        if (!string.IsNullOrEmpty(postStr))
        //        {
        //            new CommonHelp.CommonHelper().WriteSysLogToDB(postStr);//记录日志
        //            //ResponseMsg(postStr);
        //        }
        //    }
        //}
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

        #region 解析消息
        private void AnalysisWxMsg(string xml)
        {
           BaseMessage msg=MessageFactory.CreateMessage(xml);
           switch (msg.MsgType)
           {
               case MsgType.TEXT:
                   break;
               case MsgType.IMAGE:
                   break;
               case MsgType.VOICE:
                   break;
               case MsgType.VIDEO:
                   break;
               case MsgType.LOCATION:
                   break;
               case MsgType.LINK:
                   break;
               case MsgType.EVENT:
                   if (msg is SubEventMessage)//订阅/取消订阅
                   {
                       msg =msg as SubEventMessage;

                   }
                   else if (msg is NormalMenuEventMessage)//点击菜单
                   {
                       NormalMenuEventMessage _msg = msg as NormalMenuEventMessage;
                       switch (_msg.EventKey)
                       {
                           default:
                               break;
                       }
                   }
                   break;
               case MsgType.NEWS:
                   break;
               default:
                   break;
           }
        }
        
        #endregion
    }
}