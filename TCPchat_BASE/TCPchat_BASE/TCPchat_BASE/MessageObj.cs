using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace TCPchat_BASE
{
    [Serializable]
    class MessageObj
    {
        private String msg = "";
        public System.String Msg
        {
            get { return msg; }
        }
        public static int MaxBytes
        {
            get { return 1000; }
        }
        private static int frm = 0;
        public int From
        {
            get { return frm; }
            set { frm = value; }
        }

        private MessageObj()
        {

        }
        public static MessageObj WriteMessage(String message)
        {
            MessageObj obj = new MessageObj();
            obj.msg = message;
            return obj;
        }
        public static void Changenm(int name)
        {
            MessageObj.frm = name;
        }

        private IPAddress ip = IPAddress.Parse("127.0.0.1");
        public IPAddress Ip
        {
            get { return ip; }
            set { ip = value; }
        }
        public static class SerializeMessage
        {
            public static byte[] SerializeMSG(MessageObj toSerialize)
            {
                List<byte> resu = new List<byte>();
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, toSerialize);
                    return ms.ToArray();
                }
            }
            public static MessageObj DeSerializeMSG(byte[] toDeSerialize)
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream(toDeSerialize))
                {
                    return (MessageObj)bf.Deserialize(ms);
                }
            }
        }
    }
}
