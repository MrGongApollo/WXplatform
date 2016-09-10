using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wxCOM
{
    public class WXApiUrl
    {
        public enum Enum_WXUrls
        {
            Aauth2CheckAccessTokenUrl,
            Aauth2Confirm,
            Aauth2GetAccessTokenUrl,
            Aauth2GetWechatUserInfoUrl,
            Aauth2RefreshAccessTokenUrl,
            JsapiTicketUrl,
            UnifiedorderUrl,
            /// <summary>
            /// 创建自定义菜单 Post
            /// </summary>
            CreateWxMenu,
            /// <summary>
            /// 查询自定义菜单 get方法
            /// </summary>
            QueryWxMenu,
            /// <summary>
            /// 删除自定义菜单 GET
            /// </summary>
            DeleteWxMenu,
            /// <summary>
            /// 获取Access_token  GET
            /// </summary>
            GetAccess_token,
        }

        #region 微信api 地址
        /// <summary>
        /// 微信api 调用地址
        /// </summary>
        public Dictionary<Enum_WXUrls, string> Dic_WXUrls = new Dictionary<Enum_WXUrls, string> { 
            {Enum_WXUrls.Aauth2CheckAccessTokenUrl,"https://api.weixin.qq.com/sns/auth?access_token={0}&openid={1}"},
            {Enum_WXUrls.Aauth2Confirm,"https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope={2}&state=STATE#wechat_redirect"},
            {Enum_WXUrls.Aauth2GetAccessTokenUrl,"https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code"},
            {Enum_WXUrls.Aauth2GetWechatUserInfoUrl,"https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}&lang=zh_CN"},
            {Enum_WXUrls.Aauth2RefreshAccessTokenUrl,"https://api.weixin.qq.com/sns/oauth2/refresh_token?appid={0}&grant_type=refresh_token&refresh_token={1}"},
            {Enum_WXUrls.JsapiTicketUrl,"https://api.weixin.qq.com/cgi-bin/ticket/getticket?type=jsapi&access_token={0}"},
            {Enum_WXUrls.UnifiedorderUrl,"https://api.mch.weixin.qq.com/pay/unifiedorder"},
            //创建自定义菜单 Post
            {Enum_WXUrls.CreateWxMenu,"https://api.weixin.qq.com/cgi-bin/menu/create?access_token={0}"},
            //查询自定义菜单 get方法
            {Enum_WXUrls.QueryWxMenu,"https://api.weixin.qq.com/cgi-bin/menu/get?access_token=ACCESS_TOKEN"},
            //删除自定义菜单 GET
            {Enum_WXUrls.DeleteWxMenu,"https://api.weixin.qq.com/cgi-bin/menu/delete?access_token=ACCESS_TOKEN"},
            //获取Access_token  GET
            {Enum_WXUrls.GetAccess_token,"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}"},
        };
        #endregion
    }
}
