using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using HypeChatDemo.Model;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HypeChatDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ContactsPage : Page, IContactDelegate
    {
        readonly App chatAplication = (App)Application.Current;

        public ContactsPage()
        {
            this.InitializeComponent();

            chatAplication.SetContactDelegate(this);

            // fill contact list when the page is reloaded
            foreach (Contact item in chatAplication.Contacts)
            {
                ContactView.Items.Add(item);
            }
        }

        async void IContactDelegate.OnAddContact(Contact contact)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                ContactView.Items.Add(contact); // add a new contact to UI ContactsPage 
            });
        }

        async void IContactDelegate.OnRemoveContact(int index)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                ContactView.Items.RemoveAt(index); // remove contact for the ContactPage
            });
        }

        async void IContactDelegate.NotifyNewMessage(int indexOfContact)
        {
           await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
           () =>
           {
               Image newContent = GetNewContentImage(indexOfContact); // new content image on the ListView
               newContent.Visibility = Visibility.Visible; // notify user for the unread messages
           });
        }

        Image GetNewContentImage(int index)
        {
            return FindChildControl<Image>(ContactView.ContainerFromIndex(index), "NewContent") as Image;
        }

        private void ContactView_ItemClick(object sender, ItemClickEventArgs e)
        {
            int index = ContactView.Items.IndexOf(e.ClickedItem); // index of new contente item
            string instanceIdentifier = chatAplication.Stores.ElementAt(index).Key; // Get identifer of the instance 

            Image newContent = GetNewContentImage(index); // get the new content image on the ListView
            newContent.Visibility = Visibility.Collapsed; // close the new content notifier if it´s visible

            //~Open ChatPage
            this.Frame.Navigate(
                 typeof(ChatPage),
                 instanceIdentifier,
                 new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo()
            );
        }

        //Find an element on the ContactsPage.xml
        private DependencyObject FindChildControl<T>(DependencyObject control, string ctrlName)
        {
            int childNumber = VisualTreeHelper.GetChildrenCount(control);

            for (int i = 0; i < childNumber; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(control, i);
                FrameworkElement fe = child as FrameworkElement;

                // Not a framework element or is null
                if (fe == null)
                    return null;

                if (child is T && fe.Name == ctrlName)
                {
                    // Found the control so return
                    return child;
                }

                else
                {
                    // Not found - search children
                    DependencyObject nextLevel = FindChildControl<T>(child, ctrlName);

                    if (nextLevel != null)
                        return nextLevel;
                }
            }

            return null;
        }
    }
}
