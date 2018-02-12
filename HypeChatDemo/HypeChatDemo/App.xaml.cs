using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using HypeLabs;
using HypeChatDemo.Model;

namespace HypeChatDemo
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application, IStateObserver, INetworkObserver, IMessageObserver
    {
        // The _stores object keeps track of message storage associated with each instance (peer)
        private IDictionary<string, Store> _stores;
        private List<Contact> _contacts;
        private WeakReference<IContactDelegate> _contactDelegate;
        private WeakReference<IChatDelegate> _chatDelegate;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Construct();
            this.Suspending += new SuspendingEventHandler(OnSuspending);
            this.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);
        }

        partial void Construct();

        public void SetContactDelegate(IContactDelegate contactDelegate)
        {
            this._contactDelegate = new WeakReference<IContactDelegate>(contactDelegate);
        }

        public void SetChatDelegate(IChatDelegate chatDelegate)
        {
            this._chatDelegate = new WeakReference<IChatDelegate>(chatDelegate);
        }

        public IContactDelegate GetContactDelegate()
        {
            IContactDelegate contactDelegateTemp;

            if (_contactDelegate != null)
            {
                _contactDelegate.TryGetTarget(out contactDelegateTemp);
                return contactDelegateTemp;
            }

            return null;
        }

        public int GetContactIndex(string identifier)
        {
            // returns the index of one especific contact on the list. 
            return Contacts.FindIndex(contact => contact.Identifier == identifier);
        }
        
        public IChatDelegate GetChatDelegate()
        {
            IChatDelegate chatDelegateTemp;

            if (_chatDelegate != null)
            {
                _chatDelegate.TryGetTarget(out chatDelegateTemp);
                return chatDelegateTemp;
            }

            return null;
        }

        public IDictionary<string, Store> Stores
        {
            get
            {
                if (_stores == null)
                {
                    _stores = new Dictionary<string, Store>();
                }

                return _stores;
            }
        }
        public List<Contact> Contacts
        {
            get
            {
                if (_contacts == null)
                {
                    _contacts = new List<Contact>();
                }

                return _contacts;
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = false;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(ContactsPage), e.Arguments);

            }

            // Register a global back event handler. This can be registered on a per-page-bases if you only have a subset of your pages
            // that needs to handle back or if you want to do page-specific logic before deciding to navigate back on those pages.
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;

            // Ensure the current window is active
            Window.Current.Activate();

            // Start the Hype framework
            RequestHypeToStart();        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();

            // Windows does not perform proper cleanup when the services go offline. This
            // may cause service bloat on the network, which degrades performance. For this
            // reason, it's usually recommend that services are removed from the network
            // when the app is closed. This means that, at this point, Hype does not support
            // background execution on Windows when the app is closed, only when it's kept
            // minimzed. This method is called when the app is closed and causes the Hype
            // services to be removed from the network.
            RequestHypeToStop();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            // When an error occurs at runtime the Hype SDK may not get a change to perform
            // proper cleanup This may cause service bloat on the network, which degrades
            // performance. When an exception occurs, it's recommended that Hype be requested
            // to stop.
            RequestHypeToStop();
        }

        public void RequestHypeToStart()
        {
            // Add this as an Hype observer
            Hype.AddStateObserver(this);
            Hype.AddNetworkObserver(this);
            Hype.AddMessageObserver(this);

            // Generate an app identifier in the HypeLabs dashboard (https://hypelabs.io/apps/),
            // by creating a new app. Copy the given identifier here.
            Hype.SetAppIdentifier("{{app_identifier}}");

            Hype.Start();
        }

        public void RequestHypeToStop()
        {
            Hype.Stop();
        }

        /// <summary>
        /// Invoked when a user issues a global back on the device.
        /// If the app has no in-app back stack left for the current view/frame the user may be navigated away
        /// back to the previous app in the system's app back stack or to the start screen.
        /// In windowed mode on desktop there is no system app back stack and the user will stay in the app even when the in-app back stack is depleted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            SetChatDelegate(null);

            if (rootFrame == null)
                return;

            // If we can go back and the event has not already been handled, do so.
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>

        void IStateObserver.OnHypeStart()
        {
            Debug.WriteLine("Hype started!");
        }

        void IStateObserver.OnHypeStop(Error error)
        {
            String description = "";

            if (error != null)
            {
                description = String.Format("[{0}]", error.GetDescription());
            }

            Debug.WriteLine(String.Format("Hype stopped [{0}]", description));
        }

        void IStateObserver.OnHypeFailedStarting(Error error)
        {
            Debug.WriteLine(String.Format("Hype failed starting [{0}]", error.GetDescription()));
        }

        void IStateObserver.OnHypeReady()
        {
            Debug.WriteLine("Hype ready!");

            RequestHypeToStart();
        }

        void IStateObserver.OnHypeStateChange()
        {
            Debug.WriteLine(String.Format("Hype changed state to [{0}] (Idle=0, Starting=1, Running=2, Stopping=3)", (int)Hype.GetState()));
        }

        bool ShouldResolveInstance(Instance instance)
        {
            // This method can be used to decide whether an instance is interesting
            return true;
        }

        string IStateObserver.OnHypeRequestAccessToken(int userIdentifier)
        {
            // Access the app settings (https://hypelabs.io/apps/) to find an access token to use here.
            return "{{access_token}}";
        }

        void INetworkObserver.OnHypeInstanceFound(Instance instance)
        {
            Debug.WriteLine(String.Format("Hype found instance: {0}", instance.GetStringIdentifier()));

            if(ShouldResolveInstance(instance))
            {
                Hype.Resolve(instance);
            }

        }

        void INetworkObserver.OnHypeInstanceLost(Instance instance, Error error)
        {
            Debug.WriteLine(String.Format("Hype lost instance: {0} [{1}]", instance.GetStringIdentifier(), error.GetDescription()));
          
            // Cleaning up is always a good idea. It's not possible to communicate with instances
            // that were previously lost.
            Stores.Remove(instance.GetStringIdentifier());

            // Update the contact list by dropping the lost contact
            int index = GetContactIndex(instance.GetStringIdentifier());
            
            Contacts.RemoveAt(index);

            // Notifying the contact delegate, which may cause the contacts page to reload.
            IContactDelegate contactDelegate = GetContactDelegate();

            if (contactDelegate != null)
            {
                contactDelegate.OnRemoveContact(index);
            }
        }

        void INetworkObserver.OnHypeInstanceResolved(Instance instance)
        {
            Debug.WriteLine(String.Format("Hype instance resolved: {0}", instance.GetStringIdentifier()));

            // Creates a new contact to add to the contacts list
            Contact contact = new Contact() { Identifier = instance.GetStringIdentifier(), Name = "Description not available" };

            IContactDelegate contactDelegate = GetContactDelegate();

            // Instances should be strongly kept by some data structure. Their identifiers
            // are useful for keeping track of which instances are ready to communicate.
            Stores.Add(instance.GetStringIdentifier(), new Store(instance));

            // Save the contact before reloading the contacts page (ContactPage)
            Contacts.Add(contact);

            // Notify the contacts delegate (if any) that a new contact is available, which may
            // cause the contact page to reload.
            if (contactDelegate != null)
            {
                contactDelegate.OnAddContact(contact);
            }
        }

        void INetworkObserver.OnHypeInstanceFailedResolving(Instance instance, Error error)
        {
            Debug.WriteLine(String.Format("Hype failed resolving instance: {0} [{1}]", instance.GetStringIdentifier(), error.GetDescription()));
        }

        void IMessageObserver.OnHypeMessageReceived(Message message, Instance instance)
        {
            Debug.WriteLine(String.Format("Hype Got a message from: {0}", instance.GetStringIdentifier()));

            IChatDelegate chatDelegate = GetChatDelegate();
            
            Store store = Stores[instance.GetStringIdentifier()];
            
            string text = System.Text.Encoding.UTF8.GetString(message.GetData().ToArray());
            
            store.Add(text);

            // Notify the chat delegate, causing the the conversation to be updated
            if (chatDelegate != null)
            {
                chatDelegate.OnMessageReceived(message);
            }

            else
            {
                // Notify the contacts delegate so that an indicator of new contact is shown
                // at the front of the sender's contact. This is shown as a green circle.
                IContactDelegate contactDelegate = GetContactDelegate();
             
                if (contactDelegate != null)
                {
                    contactDelegate.NotifyNewMessage(GetContactIndex(instance.GetStringIdentifier()));
                }
            }
        }

        void IMessageObserver.OnHypeMessageFailedSending(MessageInfo messageInfo, Instance instance, Error error)
        {
            Debug.WriteLine(String.Format("Hype failed to send message: {0} [{1}] ", messageInfo.GetIdentifier(), error.GetDescription()));
        }

        void IMessageObserver.OnHypeMessageSent(MessageInfo messageInfo, Instance instance, float progress, bool done)
        {
            // A message being "sent" indicates that it has been written to the output
            // streams. However, the content could still be buffered for output, so it
            // has not necessarily left the device. This is useful to indicate when a
            // message is being processed, but it does not indicate delivery by the
            // destination device.
            Debug.WriteLine(String.Format("Hype is sending a message: {0}",  progress));
        }

        void IMessageObserver.OnHypeMessageDelivered(MessageInfo messageInfo, Instance instance, float progress, bool done)
        {
            // A message being delivered indicates that the destination device has
            // acknowledge reception. If the "done" argument is true, then the message
            // has been fully delivered and the content is available on the destination
            // device. This method is useful for implementing progress bars.
            Debug.WriteLine(String.Format("Hype delivered a message: {0}", progress));
        }

    }
}
