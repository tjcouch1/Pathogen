using UnityEngine.Networking;

namespace VoiceChat.Networking
{
    class VoiceChatMsgType
    {
        public const short Base = MsgType.Highest;

        public const short Packet       = Base + 1;
    }
}
