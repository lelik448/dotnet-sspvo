using System;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using SsPvo.Client.Enums;
using SsPvo.Client.Messages.Exceptions;
using SsPvo.Client.Messages.Serialization;

namespace SsPvo.Client.Messages
{
    public class RequestData
    {
        public RequestData()
        {
            JHeader = new JObject();
        }

        public JObject JHeader { get; set; }
        public XDocument XPayload { get; set; }
        public object Prepared { get; protected set; }

        public void Prepare(SsPvoMessageType msgType, SsPvoQueueMsgSubType? queueMsgSubType, ICsp csp)
        {
            try
            {
                switch (msgType)
                {
                    case SsPvoMessageType.Cls:
                        Prepared = JHeader;
                        break;
                    case SsPvoMessageType.ServiceQueue:
                    case SsPvoMessageType.EpguQueue:
                        Prepared = queueMsgSubType == SsPvoQueueMsgSubType.SingleMessage
                            ? Utils.GetSignedObject(csp, this, Utils.SerializerSettings)
                            : JHeader;
                        break;
                    case SsPvoMessageType.Cert:
                    case SsPvoMessageType.Action:
                    case SsPvoMessageType.Confirm:
                        Prepared = Utils.GetSignedObject(csp, this, Utils.SerializerSettings);
                        break;
                    default:
                        throw new NotSupportedException("Unsupported message type!");
                }
            }
            catch (Exception e)
            {
                throw new SsPvoMessageRequestPreparationException(e.Message, e);
            }
        }
    }
}
