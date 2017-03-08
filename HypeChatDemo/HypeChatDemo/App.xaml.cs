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
using HypeComponent;
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
            RequestHypeToStart();
        }

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
            // Adding itself as an Hype state observer makes sure that the application gets
            // notifications for lifecycle events being triggered by the Hype framework. These
            // events include starting and stopping, as well as some error handling.
            Hype.Instance.AddStateObserver(this);

            // Network observer notifications include other devices entering and leaving the
            // network. When a device is found all observers get a OnInstanceFound notification,
            // and when they leave OnInstanceLost is triggered instead.
            Hype.Instance.AddNetworkObserver(this);

            // I/O notifications indicate when messages are sent, delivered, or fail to be sent.
            // Notice that a message being sent does not imply that it has been delivered, only
            // that it has been queued for output. This is especially important when using mesh
            // networking, as the destination device might not be connect in a direct link.
            Hype.Instance.AddMessageObserver(this);

            // Requesting Hype to start is equivalent to requesting the device to publish
            // itself on the network and start browsing for other devices in proximity. If
            // everything goes well, the OnStart(Hype) observer method gets called, indicating
            // that the device is actively participating on the network. The 00000000 realm is
            // reserved for test apps, so it's not recommended that apps are shipped with it.
            // For generating a realm go to https://hypelabs.io, login, access the dashboard
            // under the Apps section and click "Create New App". The resulting app should
            // display a realm number. Copy and paste that here.
            Hype.Instance.Start(new Dictionary<string, Object>
            {
                { Hype.OptionRealmKey, "00000000" },
            });
        }

        public void RequestHypeToStop()
        {
            // Stopping the Hype framework does not break existing connections. When the framework
            // stops, all active connections are kept and found devices are not lost. Stopping means
            // that no new devices will be found, as the framework won't be looking for them anymore
            // and that this device is not advertising itself either.
            Hype.Instance.Stop();
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

        void IStateObserver.OnStart(Hype hype)
        {
            // At this point, the device is actively participating on the network. Other devices
            // (instances) can be found at any time and the domestic (this) device can be found
            // by others. When that happens, the two devices should be ready to communicate.
            Debug.WriteLine(String.Format("{0}: Hype started", this.GetType().Name));
        }

        void IStateObserver.OnStop(Hype hype, Error error)
        {
            String description = "";

            if (error != null)
            {

                // The error parameter will usually be null if the framework stopped because
                // it was requested to stop. This might not always happen, as even if requested
                // to stop the framework might do so with an error.
                description = String.Format("[{0}]", error.GetDescription());
            }

            // The framework has stopped working for some reason. If it was asked to do so (by
            // calling stop) the error parameter is null. If, on the other hand, it was forced
            // by some external means, the error parameter indicates the cause. Common causes
            // include the user turning the adapters off. When the later happens, you shouldn't
            // attempt to start the Hype services again. Instead, the framework triggers a 
            // OnReady delegate method call if recovery from the failure becomes possible.
            Debug.WriteLine(String.Format("{0}: Hype stopped [{1}]", this.GetType().Name, description));
        }

        void IStateObserver.OnFailedStarting(Hype hype, Error error)
        {
            // Hype couldn't start its services. Usually this means that all adapters (Wi-Fi
            // and Bluetooth) are not on, and as such the device is incapable of participating
            // on the network. The error parameter indicates the cause for the failure. Attempting
            // to restart the services is futile at this point. Instead, the implementation should
            // wait for the framework to trigger a OnReady notification, indicating that recovery
            // is possible, and start the services then.
            Debug.WriteLine(String.Format("{0}: Hype failed starting [{1}]", this.GetType().Name, error.GetDescription()));
        }

        void IStateObserver.OnReady(Hype hype)
        {
            Debug.WriteLine(String.Format("{0}: Hype ready!", this.GetType().Name));

            // This Hype delegate event indicates that the framework believes that it's capable
            // of recovering from a previous start failure. This event is only triggered once.
            // It's not guaranteed that starting the services will result in success, but it's
            // known to be highly likely. If the services are not needed at this point it's
            // possible to delay the execution for later, but it's not guaranteed that the
            // recovery conditions will still hold by then.
            RequestHypeToStart();
        }

        void IStateObserver.OnStateChange(Hype hype)
        {
            // State change updates are triggered before their corresponding, specific, observer
            // call. For instance, when Hype starts, it transits to the State.Running state,
            // triggering a call to this method, and only then is OnStart(Hype) called. Every
            // such event has a corresponding observer method, so state change notifications
            // are mostly for convenience. This method is often not used.
        }

        void INetworkObserver.OnInstanceFound(Hype hype, Instance instance)
        {
            // Hype instances that are participating on the network are identified by a full
            // UUID, composed by the vendor's realm followed by a unique identifier generated
            // for each instance.
            Debug.WriteLine(String.Format("{0}: Found instance: {1}", this.GetType().Name, instance.GetStringIdentifier()));

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

        void INetworkObserver.OnInstanceLost(Hype hype, Instance instance, Error error)
        {
            // An instance being lost means that communicating with it is no longer possible.
            // This usually happens by the link being broken. This can happen if the connection
            // times out or the device goes out of range. Another possibility is the user turning
            // the adapters off, in which case not only are all instances lost but the framework
            // also stops with an error.

            Debug.WriteLine(String.Format("{0}: Lost instance: {1}", this.GetType().Name, instance.GetStringIdentifier()));
          
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

        void IMessageObserver.OnMessageReceived(Hype hype, Message message, Instance instance)
        {
            Debug.WriteLine(String.Format("{0}: Got a message from: {1}", this.GetType().Name, instance.GetStringIdentifier()));

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

        void IMessageObserver.OnMessageFailedSending(Hype hype, MessageInfo messageInfo, Instance instance, Error error)
        {
            // Sending messages can fail for a lot of reasons, such as the adapters
            // (Wi-Fi) being turned off by the user while the process of sending the 
            // data is still ongoing. The error parameter describes the cause for 
            // the failure.
            Debug.WriteLine(String.Format("{0}: Failed to send message: {1} ", this.GetType().Name, messageInfo.GetIdentifier()));
        }

        void IMessageObserver.OnMessageSent(Hype hype, MessageInfo messageInfo, Instance instance, float progress, bool done)
        {
            // A message being "sent" indicates that it has been written to the output
            // streams. However, the content could still be buffered for output, so it
            // has not necessarily left the device. This is useful to indicate when a
            // message is being processed, but it does not indicate delivery by the
            // destination device.
            Debug.WriteLine(String.Format("{0}: Message being sent: {1}", this.GetType().Name, progress));
        }

        void IMessageObserver.OnMessageDelivered(Hype hype, MessageInfo messageInfo, Instance instance, float progress, bool done)
        {
            // A message being delivered indicates that the destination device has
            // acknowledge reception. If the "done" argument is true, then the message
            // has been fully delivered and the content is available on the destination
            // device. This method is useful for implementing progress bars.
            Debug.WriteLine(String.Format("{0}: Message being delivered: {1}", this.GetType().Name, progress));
        }
    }
}
