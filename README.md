# Connect and Communicate - a Cross Platform Connectivity and Communication Library using Bonjour and ZeroConf
A library that aims to make it easy to connect iOS, Mac and Windows devices to each other.

A library that allows you to send data between devices simply, reliably and effectively.

#Windows  
##How to install
1. Download or clone this repo
2. Open the **ConnComm_Windows** folder and open **ConnComm.sln**
3. Open solution explorer and right click on the ConnComm project
4. Select **Set as StartUp Project**
5. Now **build the class library** and you can find the dll in the **projectfolder/bin/debug/ConnComm.dll** folder

##How to Use
1. **Add ConnComm.dll as a reference to your project** by copying the dll built in the previous section
2. Open the file you wish to use ConnComm in and type  below all the using statments at the top of the file `using ConnComm`
3. Create the `Server` object using the constructors for `ServerInfo` and `ProtocolInfo` and use the method `server.PublishAndListen()` to begin.
4. Use the delegate methods and the demo program to learn more

E.g
```
TXTRecordList recordList = new TXTRecordList();
recordList.AddTXTRecord("platform", "windows");
recordList.AddTXTRecord("publish_time", DateTime.Now.ToString());

int dataBufferSize = 256;

ServerInfo serverInfo = new ServerInfo(Environment.MachineName, 12345, recordList, dataBufferSize);
ProtocolInfo protocolInfo = new ProtocolInfo("_ClickBoard", TransportProtcolType.TCP, ProtocolInfo.ProtocolDomainLocal);

server = new Server(serverInfo, protocolInfo);
server.PublishAndListen();
```

[logo]: https://github.com/hughbe/Cross-Platform-Bonjour-Connectivity-and-Communication-Library/blob/master/ConnComm_Windows/Screenshot.png" "Evidence"
