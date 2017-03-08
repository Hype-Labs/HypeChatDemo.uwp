namespace HypeChatDemo.Model
{
    public interface IContactDelegate
    {
        void OnAddContact(Contact contact);
        void OnRemoveContact(int indexOfContact);
        void NotifyNewMessage(int indexOfContact);
    }
}
