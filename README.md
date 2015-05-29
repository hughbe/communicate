# Communicate - a Cross Platform Connectivity and Communication Library using Bonjour and ZeroConf
A library that aims to make it easy to connect iOS, Mac and Windows devices to each other.

A library that allows you to send data between devices simply, reliably and effectively.

#Windows  
##How to install
1. Download or clone this repo
2. Open the **Communicate_Windows** folder and open **Communicate.sln**
3. Open solution explorer and right click on the Communicate project
4. Select **Set as StartUp Project**
5. Now **build the class library** and you can find the dll in the **projectfolder/bin/debug/Communicate.dll** folder

##How to Use
1. **Add Communicate.dll as a reference to your project** by copying the dll built in the previous section
2. Open the file you wish to use Communicate in and type  below all the using statments at the top of the file `using Communicate`
3. Create the `Communicator` object using the constructors for `ProtocolInfo` and `CommunicatorInfo` and use the method `server.Publish()`, `server.StartListening()` or `server.StartSearching()` to begin.
4. **Subscribe to the delegate events** and look at the demo programs to learn more

###Example use as a Server
```
TXTRecordList recordList = new TXTRecordList();
recordList.AddTXTRecord("platform", "windows");
recordList.AddTXTRecord("publish_time", DateTime.Now.ToString());

CommunicatorInfo communicatorInfo = new CommunicatorInfo(Environment.MachineName, 12345, recordList);
ProtocolInfo protocol = new ProtocolInfo("Test", TransportProtcolType.TCP, ProtocolInfo.ProtocolDomainLocal);

Server server = new Communicator(protocol, communicatorInfo);
server.Publish();
server.StartListening();
```

###Example use as a Client
```
CommunicatorInfo communicatorInfo = new CommunicatorInfo(Environment.MachineName, 54321, recordList);
ProtocolInfo protocol = new ProtocolInfo("Test", TransportProtcolType.TCP, ProtocolInfo.ProtocolDomainLocal);

Client client = new Communicator(protocol, communicatorInfo);
client.StartSearching();
```

#iOS and Mac OSX  
##How to install
1. Download or clone this repo

##How to Use
1. Copy or reference the files in the **Communicate _iOS_Mac/Communicate Core** folder to your project
2. Type **#import "Communicate.h"** at the top of any file you wish to use Communicate with
3. Create the `Communicator` object using the constructors for `ProtocolInfo` and `CommunicatorInfo` and use the methods `[server publish]`, `[server startListening]` or `[server startSearching]` to begin.
4. **Subscribe to the delegate events** and look at the demo programs to learn more

###Example use as a Server
TXTRecordList *TXTRecords = [[TXTRecordList alloc]init];
[TXTRecords addTXTRecordWithKey:"platform" value:"iOS"];

CommunicatorInfo *communicatorInfo = [[CommunicatorInfo alloc]initWithName:[UIDevice currentDevice].name port:12345 txtRecordList:TXTRecords];
ProtocolInfo *protocol = [[ProtocolInfo alloc]initWithProtocolName:@"Test" protocolType:TransportProtocolTypeTCP domain:nil];
    
Server *server = [[Communicator alloc]initWithProtocolInfo:protocol communicatorInfo:communicatorInfo delegate:self];
[server publish];
[server startListening];
```

###Example use as a Client
```
ProtocolInfo *protocol = [[ProtocolInfo alloc]initWithProtocolName:@"Test" protocolType:TransportProtocolTypeTCP domain:nil];    CommunicatorInfo *communicatorInfo = [[CommunicatorInfo alloc]initWithName:[UIDevice currentDevice].name port:54321 txtRecordList:nil];
Client *client = self.client = [[Communicator alloc]initWithProtocolInfo:protocol communicatorInfo:communicatorInfo delegate:self];
[client startSearching];
```

[logo]: https://github.com/hughbe/Cross-Platform-Bonjour-Connectivity-and-Communication-Library/blob/master/ConnComm_Windows/Screenshot.png" "Evidence"
