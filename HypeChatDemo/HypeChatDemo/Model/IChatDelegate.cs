using HypeLabs;

namespace HypeChatDemo.Model
{
    public interface IChatDelegate
    {
        void OnMessageReceived(Message message);
    }
}
