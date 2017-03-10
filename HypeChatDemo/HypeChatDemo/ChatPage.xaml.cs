using HypeChatDemo.Model;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using HypeLabs;
using System.Text;
using Windows.ApplicationModel.Core;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace HypeChatDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatPage : Page , IChatDelegate
    {
        readonly App chatAplication = (App)Application.Current;
      
        public ChatPage()
        {
            this.InitializeComponent();
            chatAplication.SetChatDelegate(this);
        }

        public string InstanceID { get; set; }

        private Message SendMessage(String text, Instance instance)
        {
            // When sending content there must be some sort of protocol that both parties
            // understand. In this case, we simply send the text encoded in UTF-8. The data
            // must be decoded when received, using the same encoding.
            byte[] data = Encoding.UTF8.GetBytes(text);

            // Sends the data and returns the message that has been generated for it. Messages have
            // identifiers that are useful for keeping track of the message's deliverability state.
            // In order to track message delivery set the last parameter to true. Notice that this
            // is not recommend, as it incurs extra overhead on the network. Use this feature only
            // if progress tracking is really necessary.
            return Hype.Instance.Send(data.AsBuffer(), instance, true);
        }

        public Store GetStore()
        {
            if (chatAplication.Stores.ContainsKey(InstanceID))
            {
                return chatAplication.Stores[InstanceID];
            }

            return null;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Instance id selected by User on the ContactPage
            InstanceID = (string)e.Parameter;

           foreach (string item in GetStore().Messages)
           {
                MessageView.Items.Add(item);
           }

            Frame rootFrame = Window.Current.Content as Frame;

            // If we have pages in our in-app backstack 
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }
        
        private void SendButtonClick(object sender, RoutedEventArgs e)
        {
            if (MessageTextBox.Text.Length > 0)
            {
              string text = MessageTextBox.Text;
              MessageView.Items.Add(text);
              GetStore().Messages.Add(text);
              Message message = SendMessage(text, GetStore().Instance);
            }
            
            MessageTextBox.Text = "";
        }

        async void IChatDelegate.OnMessageReceived(Message message)
        {
           await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
           () =>
           {
               string text = System.Text.Encoding.UTF8.GetString(message.GetData().ToArray());
               MessageView.Items.Add(text); // add message received to the chatPage
           });
        }
    }
}
