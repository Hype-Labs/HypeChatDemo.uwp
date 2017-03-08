
![alt tag](https://hypelabs.io/static/img/NQMAnSZ.jpg)
![alt tag](https://hypelabs.io/static/img/logo200x.png)

[Hype](http://hypelabs.io/?r=13) is an SDK for cross-platform peer-to-peer communication with mesh networking. Hype works even without Internet access, connecting devices via other communication channels such as Bluetooth, Wi-Fi direct, and Infrastructural Wi-Fi.

The Hype SDK has been designed by [Hype Labs](http://hypelabs.io/?r=13). It is currently in private Beta for [iOS](https://github.com/Hype-Labs/HypeChatDemo.ios), [Android](https://github.com/Hype-Labs/HypeChatDemo.android), and Windows Universal Platform.

You can start using Hype today, [join the beta by subscribing on our website](http://hypelabs.io/?r=13).

## What does it do?

This project consists of a chat app sketch written to illustrate how to work with the SDK. The app displays a list of devices in close proximity, which can be tapped for exchanging text content. The SDK itself allows sending other kinds of media, such as pictures, or video, but the demo is limited for simplicity purposes.

Most of the documentation is inline with the code, and further information can be found on the Hype Labs [official documentation site](https://hypelabs.io/docs/).

## Setup

To run the project you'll need the Hype SDK binary. To access it, subscribe on the Hype Labs [website](http://hypelabs.io/?r=13) to get early access to the SDK. After downloading the binary and run `Hype.vsix`. This will install Hype on your system, which will make it visible to Visual Studio.

## Overview

Most of the action takes place in `App` class, which you'll find under the `App.xaml.cs` file. There you'll learn about Hype's lifecycle, setup, and maintenance in around 20 lines of code. The `ChatPage` class, which lives under `ChatPage.xaml.cs`, shows how to send and receive messages. Refer to the inline documentation for more details.

#### 1. Download the Hype SDK

The first thing you need is the Hype SDK binary. Subscribe for the Beta program at the Hype Labs [website](http://hypelabs.io/downloads/) and follow the instructions from your inbox. You'll need your subscription to be activated before proceeding.

#### 2. Install the SDK for your Visual Studio project

Hype is really easy to configure! Double click the `Hype.vsix` binary, which should trigger an installation wizard. Follow the instructions given. If necessary, restart Visual Studio afterwards.

#### 3. Configure your project

After creating the project of your choice, access `References` and then `Add Reference`. On the `Universal Windows` menu, under `Extensions` select the option for `Hype` and click `OK`. You'll also need to access your `Package.appxmanifest` and under the `Capabilities` separator select `Internet(Client & Server)`, `Internet(Client)`, and `Private Networks(Client & Server)`. These are required by the Hype SDK in order to manage Infrastructual Wi-Fi networks.

#### 4. Register an app

Go to [the apps page](http://hypelabs.io/apps) and create a new app by pressing the _Create new app_ button on the top left. Enter a name for your app and press Submit. The app dialog that appears afterwards yields a 8-digit hexadecimal number, called a _realm_. Keep that number for step 5. Realms are a means of segregating the network, by making sure that different apps do not communicate with each other, even though they are capable of forwarding each other's contents. If your project requires a deeper understanding of how the technology works we recommend reading the [Overview](http://hypelabs.io/docs/ios/overview/) page. There you'll find a more detailed analysis of what realms are and what they do, as well as other topics about the Hype framework.

#### 5. Setup the realm

The realm must be set when starting the Hype services. For that effect, you can set the `Hype.OptionRealmKey` when starting the framework's services with a `String` value indicating the realm. The following example illustrates how to do this. The 00000000 realm is reserved for testing purposes and apps should not be deployed with this realm.

```cs
	Hype.Instance.Start(new Dictionary<string, Object>
            {
                { Hype.OptionRealmKey, "00000000" },
            });
```

#### 6. Start Hype services

It's time to write some code! Have the view controller of your choice implement the observer protocols (`IStateObserver`, `INetworkObserver`, `IMessageObserver`) and registering itself as an observer. After that, it's time to start the Hype services. Like so:

```cs

sealed partial class App : Application, IStateObserver, INetworkObserver, IMessageObserver
{
    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        // Request Hype to start
        RequestHypeToStart();
    }

    private void OnSuspending(object sender, SuspendingEventArgs e)
    {
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
}


```

This code demonstrates how to add an instance as an Hype observer and starting the framework's services. Observers get notifications from the framework indicating when events happen, such as Hype's lifecycle and receiving messages from other devices. The last line in `OnLaunched(LaunchActivatedEventArgs)` requests Hype to start it's services, by publishing the device on the network and browsing for other devices in proximity. At this point, the framework is still not actively participating on the network, though. Only when the observer method `IStateObserver.OnStart(Hype)` is called is the device actively participating on the network. After that happens, instances can be found at any time if other devices are in proximity. When that happens, the framework triggers a `INetworkObserver.OnInstanceFound(Hype, Instance)` notification, indicating that another peer is ready to communicate. You should keep found instances on a map, as you'll need those later to exchange content. Here's one way how that could be accomplished, while expanding on the previous example:

```cs

sealed partial class App : Application, IStateObserver, INetworkObserver, IMessageObserver
{
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
}
```

### 7. Sending messages

Sending messages is also very simple. All it takes is a previously found instance and a couple lines of code. This code lives under `ChatPage.xaml.cs`.

```cs
private Message SendMessage(String text, Instance instance)
{
    // When sending content there must be some sort of protocol that both parties
    // understand. In this case, we simply send the text encoded in UTF-8. The data
    // must be decoded when received, using the same encoding.
    byte[] data = Encoding.ASCII.GetBytes(text);

    // Sends the data and returns the message that has been generated for it. Messages have
    // identifiers that are useful for keeping track of the message's deliverability state.
    // In order to track message delivery set the last parameter to true. Notice that this
    // is not recommend, as it incurs extra overhead on the network. Use this feature only
    // if progress tracking is really necessary.
    return Hype.Instance.Send(data.AsBuffer(), instance, true);
}
```

Finally, messages are received by all `IMessageObserver` instances actively observing framework events.

```cs

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
```

Notice that the encoding used is the same both when sending and when receiving. The protocol must be the same on both ends of the link.

## License

This project is MIT-licensed.

## Follow us

Follow us on [twitter](http://www.twitter.com/hypelabstech) and [facebook](http://www.facebook.com/hypelabs.io). We promise to keep you up to date!

