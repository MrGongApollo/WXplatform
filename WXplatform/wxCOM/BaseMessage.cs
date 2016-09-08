using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wxCOM
{
    public abstract class BaseMessage
    {
        /// <summary>
        /// 开发者微信号
        /// </summary>
        public string ToUserName { get; set; }
        /// <summary>
        /// 发送方帐号（一个OpenID）
        /// </summary>
        public string FromUserName { get; set; }
        /// <summary>
        /// 消息创建时间 （整型）
        /// </summary>
        public int CreateTime { get; set; }
        /// <summary>
        /// 消息类型
        /// </summary>
        public MsgType MsgType { get; set; }


        #region 回复空消息
        /// <summary>
        /// 回复空消息
        /// </summary>
        public virtual void ResponseNull(string ToUserName)
        {
            Utils.ResponseWrite(ToUserName,"");
        }
        #endregion
        #region 回复文本消息
        /// <summary>
        /// 回复文本消息
        /// </summary>
        /// <param name="param"></param>
        /// <param name="content"></param>
        public virtual void ResText(string ToUserName, string content,EnterParam param = null)
        {
            Utils.ResponseWrite(ToUserName, content);
        }
        #endregion       
        #region 回复消息(音乐)
        /// <summary>
        /// 回复消息(音乐)
        /// </summary>
        public void ResMusic(EnterParam param, MusicMessage mu)
        {

        }
        #endregion
        #region 回复视频
        /// <summary>
        /// 回复视频
        /// </summary>
        /// <param name="param"></param>
        /// <param name="v"></param>
        public void ResVideo(EnterParam param, VideoMessage v)
        {
        }
        #endregion
        #region 回复消息(图片)
        /// <summary>
        /// 回复消息(图片)
        /// </summary>
        public void ResPicture(EnterParam param, ImgMessage img, string domain)
        {
        }
        #endregion
        #region 回复消息（图文列表）
        /// <summary>
        /// 回复消息（图文列表）
        /// </summary>
        /// <param name="param"></param>
        /// <param name="art"></param>
        public void ResArticles(EnterParam param, List<NewsMessage> art)
        {
        }
        #endregion
        #region 多客服转发
        /// <summary>
        /// 多客服转发
        /// </summary>
        /// <param name="param"></param>
        public void ResDKF(EnterParam param)
        {
        }
        #endregion



        /// <summary>
        /// 多客服转发如果指定的客服没有接入能力(不在线、没有开启自动接入或者自动接入已满)，该用户会一直等待指定客服有接入能力后才会被接入，而不会被其他客服接待。建议在指定客服时，先查询客服的接入能力指定到有能力接入的客服，保证客户能够及时得到服务。
        /// </summary>
        /// <param name="param">用户发送的消息体</param>
        /// <param name="KfAccount">多客服账号</param>
        public void ResDKF(EnterParam param, string KfAccount)
        {
        }
        private void Response(EnterParam param, string data)
        {

        }


    }

    #region 文本实体
    public class TextMessage : BaseMessage
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 消息id，64位整型
        /// </summary>
        public string MsgId { get; set; }


    }
    #endregion

    #region 图片实体
    public class ImgMessage : BaseMessage
    {
       /// <summary>
       /// 图片路径
       /// </summary>
        public string PicUrl { get; set; }
       /// <summary>
        /// 消息id，64位整型
       /// </summary>
        public string MsgId { get; set; }
        /// <summary>
        /// 媒体ID
        /// </summary>
        public string MediaId { get; set; }

     
    }
    #endregion

    #region 语音实体
    public class VoiceMessage : BaseMessage
    {
       /// <summary>
       /// 缩略图ID
       /// </summary>
        public string MsgId { get; set; }
       /// <summary>
        /// 格式
       /// </summary>
        public string Format { get; set; }
        /// <summary>
        /// 媒体ID
        /// </summary>
        public string MediaId { get; set; }
        /// <summary>
        /// 语音识别结果
        /// </summary>
        public string Recognition { get; set; }

    
    }
    #endregion

    #region 视频实体
    public class VideoMessage : BaseMessage
    {
       /// <summary>
       /// 缩略图ID
       /// </summary>
        public string ThumbMediaId { get; set; }
       /// <summary>
        /// 消息id，64位整型
       /// </summary>
        public string MsgId { get; set; }
        /// <summary>
        /// 媒体ID
        /// </summary>
        public string MediaId { get; set; }

    
    }
    #endregion

    #region 音乐实体
    public class MusicMessage : BaseMessage
    {  
        /// <summary>
        /// 音乐标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 音乐描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 音乐链接
        /// </summary>
        public string MusicURL { get; set; }
        /// <summary>
        /// 高质量音乐链接，WIFI环境优先使用该链接播放音乐
        /// </summary>
        public string HQMusicUrl { get; set; }
        /// <summary>
        /// <summary>
        /// 缩略图的媒体id，通过素材管理接口上传多媒体文件，得到的idID
        /// </summary>
        public string ThumbMediaId { get; set; }

    }
    #endregion

    #region 图文消息实体
    public class NewsMessage:BaseMessage
    {
        /// <summary>
        /// 	图文消息个数，限制为10条以内
        /// </summary>
        public int ArticleCount { get; set; }
        /// <summary>
        /// 	多条图文消息信息，默认第一个item为大图,注意，如果图文数超过10，则将会无响应
        /// </summary>
        public string Articles { get; set; }
        /// <summary>
        /// 图文消息标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 	图文消息描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 图片链接，支持JPG、PNG格式，较好的效果为大图360*200，小图200*200
        /// </summary>
        public string PicUrl { get; set; }
        /// <summary>
        /// 点击图文消息跳转链接
        /// </summary>
        public string Url { get; set; }
    }
    #endregion

    #region 链接实体
    public class LinkMessage : BaseMessage
    {
       /// <summary>
       /// 缩略图ID
       /// </summary>
        public string MsgId { get; set; }
       /// <summary>
        /// 标题
       /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 链接地址
        /// </summary>
        public string Url { get; set; }

     
    }
    #endregion

    #region 地理位置消息
    public class LocationMessage : BaseMessage
    {
        /// <summary>
        /// 消息id，64位整型ID
        /// </summary>
        public string MsgId { get; set; }
        /// <summary>
        /// 地理位置维度
        /// </summary>
        public string Location_X { get; set; }
        /// <summary>
        /// 地理位置经度
        /// </summary>
        public string Location_Y { get; set; }
        /// <summary>
        /// 地图缩放大小
        /// </summary>
        public string Scale { get; set; }
        /// <summary>
        /// 地理位置信息
        /// </summary>
        public string Label { get; set; }
    }
    #endregion

    #region 消息类型枚举
    /// <summary>
    /// 消息类型枚举
    /// </summary>
    public enum MsgType
    {
        /// <summary>
        ///文本类型
        /// </summary>
        TEXT,
        /// <summary>
        /// 图片类型
        /// </summary>
        IMAGE,
        /// <summary>
        /// 语音类型
        /// </summary>
        VOICE,
        /// <summary>
        /// 视频类型
        /// </summary>
        VIDEO,
        /// <summary>
        /// 地理位置类型
        /// </summary>
        LOCATION,
        /// <summary>
        /// 链接类型
        /// </summary>
        LINK,
        /// <summary>
        /// 事件类型
        /// </summary>
        EVENT,
        /// <summary>
        /// 图文消息
        /// </summary>
        NEWS
    }
    #endregion

}