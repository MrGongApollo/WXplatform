using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using wxBIZ;

namespace wxCOM
{
    public class MessageFactory
    {
        private static List<BaseMsg> _queue;

        #region 原先
        //public static BaseMessage CreateMessage(string xml)
        //{
        //    if (_queue == null)
        //    {
        //        _queue = new List<BaseMsg>();
        //    }
        //    else if (_queue.Count >= 50)
        //    {
        //        _queue = _queue.Where(q => { return q.CreateTime.AddSeconds(20) > DateTime.Now; }).ToList();//保留20秒内未响应的消息
        //    }
        //    XElement xdoc = XElement.Parse(xml);
        //    var msgtype = xdoc.Element("MsgType").Value.ToUpper();
        //    var FromUserName = xdoc.Element("FromUserName").Value;
        //    var MsgId = xdoc.Element("MsgId").Value;
        //    var CreateTime = xdoc.Element("CreateTime").Value;
        //    MsgType type = (MsgType)Enum.Parse(typeof(MsgType), msgtype);
        //    if (type != MsgType.EVENT)
        //    {
        //        if (_queue.FirstOrDefault(m => { return m.MsgFlag == MsgId; }) == null)
        //        {
        //            _queue.Add(new BaseMsg
        //            {
        //                CreateTime = DateTime.Now,
        //                FromUser = FromUserName,
        //                MsgFlag = MsgId
        //            });
        //        }
        //        else
        //        {
        //            return null;
        //        }

        //    }
        //    else
        //    {
        //        if (_queue.FirstOrDefault(m => { return m.MsgFlag == CreateTime; }) == null)
        //        {
        //            _queue.Add(new BaseMsg
        //            {
        //                CreateTime = DateTime.Now,
        //                FromUser = FromUserName,
        //                MsgFlag = CreateTime
        //            });
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //    switch (type)
        //    {
        //        case MsgType.TEXT: return Utils.ConvertObj<TextMessage>(xml);
        //        case MsgType.IMAGE: return Utils.ConvertObj<ImgMessage>(xml);
        //        case MsgType.VIDEO: return Utils.ConvertObj<VideoMessage>(xml);
        //        case MsgType.VOICE: return Utils.ConvertObj<VoiceMessage>(xml);
        //        case MsgType.LINK:
        //            return Utils.ConvertObj<LinkMessage>(xml);
        //        case MsgType.LOCATION:
        //            return Utils.ConvertObj<LocationMessage>(xml);
        //        case MsgType.EVENT://事件类型
        //            {
        //                var eventtype = (EventType)Enum.Parse(typeof(EventType), xdoc.Element("Event").Value.ToUpper());
        //                switch (eventtype)
        //                {
        //                    case EventType.CLICK:
        //                        return Utils.ConvertObj<NormalMenuEventMessage>(xml);
        //                    case EventType.VIEW: return Utils.ConvertObj<NormalMenuEventMessage>(xml);
        //                    case EventType.LOCATION: return Utils.ConvertObj<LocationEventMessage>(xml);
        //                    //case EventType.LOCATION_SELECT: return Utils.ConvertObj<LocationMenuEventMessage>(xml);
        //                    case EventType.SCAN: return Utils.ConvertObj<ScanEventMessage>(xml);
        //                    case EventType.SUBSCRIBE: return Utils.ConvertObj<SubEventMessage>(xml);
        //                    case EventType.UNSUBSCRIBE: return Utils.ConvertObj<SubEventMessage>(xml);
        //                    case EventType.SCANCODE_WAITMSG: return Utils.ConvertObj<ScanMenuEventMessage>(xml);
        //                    default:
        //                        return Utils.ConvertObj<EventMessage>(xml);
        //                }
        //            } break;
        //        default:
        //            return Utils.ConvertObj<BaseMessage>(xml);
        //    }
        //}
        #endregion

        #region 修改
        /// <summary>
        /// 解析xml
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static BaseMessage CreateMessage(string xml)
        {
            if (_queue == null)
            {
                _queue = new List<BaseMsg>();
            }
            else if (_queue.Count >= 50)
            {
                _queue = _queue.Where(q => { return q.CreateTime.AddSeconds(20) > DateTime.Now; }).ToList();//保留20秒内未响应的消息
            }
            XElement xdoc = XElement.Parse(xml);
            var msgtype = xdoc.Element("MsgType").Value.ToUpper();
            var FromUserName = xdoc.Element("FromUserName").Value;
            var MsgId = xdoc.Element("MsgId").Value;
            var CreateTime = xdoc.Element("CreateTime").Value;
            MsgType type = (MsgType)Enum.Parse(typeof(MsgType), msgtype);
            if (type != MsgType.EVENT)
            {
                if (_queue.FirstOrDefault(m => { return m.MsgFlag == MsgId; }) == null)
                {
                    _queue.Add(new BaseMsg
                    {
                        CreateTime = DateTime.Now,
                        FromUser = FromUserName,
                        MsgFlag = MsgId
                    });
                }
                else
                {
                    return null;
                }

            }
            else
            {
                if (_queue.FirstOrDefault(m => { return m.MsgFlag == CreateTime; }) == null)
                {
                    _queue.Add(new BaseMsg
                    {
                        CreateTime = DateTime.Now,
                        FromUser = FromUserName,
                        MsgFlag = CreateTime
                    });
                }
                else
                {
                    return null;
                }
            }

            switch (type)
            {
                #region 文字消息
                case MsgType.TEXT:
                    //自动回复消息
                    TextMessage receiveMsg = Utils.ConvertObj<TextMessage>(xml);
                    TextMessage _retmsg = new TextMessage()
                    {
                        FromUserName = receiveMsg.ToUserName,
                        ToUserName = receiveMsg.FromUserName,
                        Content = string.Format("你刚才居然对我说了：{0}", receiveMsg.Content)
                    };
                    _retmsg.ResText(_retmsg);
                    #region 保存到数据库
                     try
                    {
                        using (wxEntities _db=new wxEntities())
                        {
                            _db.T_TextMessage.Add(new T_TextMessage
                            {
                                MsgId=receiveMsg.MsgId,
                                FromUserName = receiveMsg.FromUserName,
                                ToUserName=receiveMsg.ToUserName,
                                MsgType = receiveMsg.MsgType.ToString(),
                                CreateTime=receiveMsg.CreateTime,
                                Content=receiveMsg.Content,
                                SysCreateTime=DateTime.Now
                            });
                            _db.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {
                        
                    }

                    #endregion
                    return receiveMsg;
                #endregion
                #region 图片消息
                case MsgType.IMAGE:
                    ImgMessage receiveImg = Utils.ConvertObj<ImgMessage>(xml);
                    System.Reflection.PropertyInfo[] Props = receiveImg.GetType().GetProperties();
                   
                    #region 记录到数据库
                    try
                    {
                        using (wxEntities _db=new wxEntities())
                        {
                            T_ImgMessage Img = _db.T_ImgMessage.Create();
                            Img.SysCreateTime = DateTime.Now;

                            ImgMessage postImg = new ImgMessage();
                            foreach (System.Reflection.PropertyInfo Prop in Props)
                            {
                                var _val = receiveImg.GetType().GetProperty(Prop.Name).GetValue(receiveImg);
                                postImg.GetType().GetProperty(Prop.Name).SetValue(postImg, _val);
                                Img.GetType().GetProperty(Prop.Name).SetValue(Img, _val);
                            }
                            #region 回复图片消息
                            postImg.FromUserName = receiveImg.ToUserName;
                            postImg.ToUserName = receiveImg.FromUserName;
                            new ImgMessage().ResPicture(null, postImg, null);
                            #endregion
                        }
                    }
                    catch (Exception)
                    {
                        
                    }
                    #endregion

                    return receiveImg;
                #endregion
                #region 视频消息
                case MsgType.VIDEO: return Utils.ConvertObj<VideoMessage>(xml);
                #endregion
                #region 语言消息
                case MsgType.VOICE:
                    VoiceMessage postVoice = Utils.ConvertObj<VoiceMessage>(xml);
                    TextMessage retmsg = new TextMessage() {
                        FromUserName = postVoice.ToUserName,
                        ToUserName = postVoice.FromUserName,
                        Content=string.Format("已经智能辨别您的语言消息：{0}", postVoice.Recognition)
                    };
                    retmsg.ResText(retmsg);
                    return postVoice;
                #endregion
                #region 链接消息
                case MsgType.LINK:
                    return Utils.ConvertObj<LinkMessage>(xml);
                #endregion
                #region 地理消息
                case MsgType.LOCATION:
                    return Utils.ConvertObj<LocationMessage>(xml);
                #endregion
                #region 事件消息
                case MsgType.EVENT://事件类型
                    {
                        var eventtype = (EventType)Enum.Parse(typeof(EventType), xdoc.Element("Event").Value.ToUpper());
                        switch (eventtype)
                        {
                            case EventType.CLICK:

                                return Utils.ConvertObj<NormalMenuEventMessage>(xml);
                            case EventType.VIEW: return Utils.ConvertObj<NormalMenuEventMessage>(xml);
                            case EventType.LOCATION: return Utils.ConvertObj<LocationEventMessage>(xml);
                            //case EventType.LOCATION_SELECT: return Utils.ConvertObj<LocationMenuEventMessage>(xml);
                            case EventType.SCAN: return Utils.ConvertObj<ScanEventMessage>(xml);
                            case EventType.SUBSCRIBE:
                                SubEventMessage receivesubmgs=Utils.ConvertObj<SubEventMessage>(xml);
                                //关注时自动回复消息
                                TextMessage submsg = new TextMessage
                                {
                                    FromUserName = receivesubmgs.ToUserName,
                                    ToUserName = receivesubmgs.FromUserName,
                                    Content = "欢迎关注测试平台！"
                                };
                                submsg.ResText(submsg);

                                return receivesubmgs;
                            case EventType.UNSUBSCRIBE: return Utils.ConvertObj<SubEventMessage>(xml);
                            case EventType.SCANCODE_WAITMSG: return Utils.ConvertObj<ScanMenuEventMessage>(xml);
                            default:
                                return Utils.ConvertObj<EventMessage>(xml);
                        }
                    } break;
                #endregion
                #region 其他消息
                default:
                    return Utils.ConvertObj<BaseMessage>(xml);
                #endregion
            }
        }
        #endregion

    }
}