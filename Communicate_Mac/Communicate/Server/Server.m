//
//  Server.m
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "Server.h"
#import "ServerInfo.h"
#import "ProtocolInfo.h"
#import "TXTRecordList.h"
#import "ConnectedClients.h"
#import "ConnectedClient.h"
#import "CommunicationData.h"
#import "ConnectionHandler.h"

#include <CoreFoundation/CoreFoundation.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>

NSString* const ServerErrorDomain = @"server";

@interface Server ()

@property (strong, readwrite, nonatomic) ServerInfo *serverInfo;
@property (strong, readwrite, nonatomic) ProtocolInfo *protocolInfo;

@property (assign, readwrite, nonatomic) ServerPublishingState publishingState;
@property (assign, readwrite, nonatomic) ServerListeningState listeningState;

@property (strong, readwrite, nonatomic) NSNetService *publishedService;

@property (assign, nonatomic) CFSocketRef ipv4Socket;
@property (assign, nonatomic) CFSocketRef ipv6Socket;

@property (strong, readwrite, nonatomic) ConnectedClients *connectedClients;

+ (NSError *)serverErrorForErrorCode:(ServerErrorCode)serverErrorCode extraInformation:(NSDictionary *)extraInformation;

@end

@implementation Server

- (instancetype)initWithServerInfo:(ServerInfo *)serverInfo protocolInfo:(ProtocolInfo *)protocolInfo delegate:(id<ServerDelegate>)delegate {
    self = [super init];
    if(self) {
        self.serverInfo = serverInfo;
        self.protocolInfo = protocolInfo;
        self.delegate = delegate;
    }
    return self;
}

- (void)publishAndStartListening {
    [self publish];
    [self startListening];
}

- (void)publish {
    if(self.publishingState == ServerPublishingStatePublishing) {
        if([self.delegate respondsToSelector:@selector(serverDidNotPublish:reason:)]) {
            [self.delegate serverDidNotPublish:self reason:[Server serverErrorForErrorCode:ServerErrorCodesPublishing extraInformation:nil]];
        }
        return;
    }
    else if(self.publishingState == ServerPublishingStatePublished) {
        if([self.delegate respondsToSelector:@selector(serverDidNotPublish:reason:)]) {
            [self.delegate serverDidNotPublish:self reason:[Server serverErrorForErrorCode:ServerErrorCodesAlreadyPublished extraInformation:nil]];
        }
        return;
    }
    
    self.publishingState = ServerPublishingStatePublishing;
    if([self.delegate respondsToSelector:@selector(serverDidStartPublishing:)]) {
        [self.delegate serverDidStartPublishing:self];
    }
    
    self.publishedService = [[NSNetService alloc]initWithDomain:self.protocolInfo.domain type:[self.protocolInfo serializeTypeWithDomain:NO] name:self.serverInfo.readableName port:self.serverInfo.port];
    self.publishedService.TXTRecordData = [self.serverInfo.TXTRecordList serialize];
    self.publishedService.delegate = self;
    [self.publishedService publish];
}

- (void)startListening {
    if(self.listeningState == ServerListeningStateListening) {
        if([self.delegate respondsToSelector:@selector(serverDidNotStartListening:reason:)])  {
            [self.delegate serverDidNotStartListening:self reason:[Server serverErrorForErrorCode:ServerErrorCodesAlreadyPublished extraInformation:nil]];
        }
        return;
    }
    self.listeningState = ServerListeningStateListening;
    
    const CFSocketContext context = { 0, (__bridge void *)(self), NULL, NULL, NULL };
    
    self.ipv4Socket = CFSocketCreate(kCFAllocatorDefault, PF_INET, SOCK_STREAM, IPPROTO_TCP, kCFSocketAcceptCallBack, (CFSocketCallBack)&ServerAcceptCallBack, &context);
    self.ipv6Socket = CFSocketCreate(kCFAllocatorDefault, PF_INET6, SOCK_STREAM, IPPROTO_TCP, kCFSocketAcceptCallBack, (CFSocketCallBack)&ServerAcceptCallBack, &context);
    
    if(self.ipv4Socket == NULL || self.ipv6Socket ==  NULL) {
        self.listeningState = ServerListeningStateErrorListening;
        if(self.ipv4Socket != NULL) {
            CFRelease(self.ipv4Socket);
        }
        if(self.ipv6Socket != NULL) {
            CFRelease(self.ipv6Socket);
        }
        self.ipv4Socket = NULL;
        self.ipv6Socket = NULL;
        
        if([self.delegate respondsToSelector:@selector(serverDidNotStartListening:reason:)])  {
            [self.delegate serverDidNotStartListening:self reason:[Server serverErrorForErrorCode:ServerErrorCodesNoSocketAvailable extraInformation:nil]];
        }
        
        return;
    }
    
    struct sockaddr_in sin;
    
    memset(&sin, 0, sizeof(sin));
    sin.sin_len = sizeof(sin);
    sin.sin_family = AF_INET;
    sin.sin_port = htons(self.serverInfo.port);
    sin.sin_addr.s_addr= INADDR_ANY;
    
    CFDataRef sincfd = CFDataCreate(kCFAllocatorDefault, (UInt8 *)&sin, sizeof(sin));
    
    if(CFSocketSetAddress(self.ipv4Socket, sincfd) != kCFSocketSuccess) {
        self.listeningState = ServerListeningStateErrorListening;
        if(self.ipv4Socket != NULL) {
            CFRelease(self.ipv4Socket);
        }
        if(self.ipv6Socket != NULL) {
            CFRelease(self.ipv6Socket);
        }
        self.ipv4Socket = NULL;
        self.ipv6Socket = NULL;
        CFRelease(sincfd);
        
        if([self.delegate respondsToSelector:@selector(serverDidNotStartListening:reason:)])  {
            [self.delegate serverDidNotStartListening:self reason:[Server serverErrorForErrorCode:ServerErrorCodesCouldNotBindIPV4Address extraInformation:nil]];
        }

        return;
    }
    
    CFRelease(sincfd);
    
    struct sockaddr_in6 sin6;
    
    memset(&sin6, 0, sizeof(sin6));
    sin6.sin6_len = sizeof(sin6);
    sin6.sin6_family = AF_INET6;
    sin6.sin6_port = htons(self.serverInfo.port);
    sin6.sin6_addr = in6addr_any;
    
    CFDataRef sin6cfd = CFDataCreate(kCFAllocatorDefault, (UInt8 *)&sin6, sizeof(sin6));
    
    if(CFSocketSetAddress(self.ipv6Socket, sin6cfd) != kCFSocketSuccess) {
        self.listeningState = ServerListeningStateErrorListening;
        if(self.ipv4Socket != NULL) {
            CFRelease(self.ipv4Socket);
        }
        if(self.ipv6Socket != NULL) {
            CFRelease(self.ipv6Socket);
            CFRelease(self.ipv6Socket);
        }
        self.ipv4Socket = NULL;
        self.ipv6Socket = NULL;
        CFRelease(sin6cfd);
        
        if([self.delegate respondsToSelector:@selector(serverDidNotStartListening:reason:)])  {
            [self.delegate serverDidNotStartListening:self reason:[Server serverErrorForErrorCode:ServerErrorCodesCouldNotBindIPV6Address extraInformation:nil]];
        }
        
        return;
    }
    
    CFRelease(sin6cfd);
    
    CFRunLoopSourceRef socketsource = CFSocketCreateRunLoopSource(kCFAllocatorDefault, self.ipv4Socket,0);
    CFRunLoopAddSource(CFRunLoopGetCurrent(), socketsource, kCFRunLoopDefaultMode);
    
    CFRunLoopSourceRef socketsource6 = CFSocketCreateRunLoopSource(kCFAllocatorDefault, self.ipv6Socket, 0);
    CFRunLoopAddSource(CFRunLoopGetCurrent(), socketsource6, kCFRunLoopDefaultMode);
    
    if([self.delegate respondsToSelector:@selector(serverDidStartListening:)]) {
        [self.delegate serverDidStartListening:self];
    }
}

static void ServerAcceptCallBack(CFSocketRef socket, CFSocketCallBackType type, CFDataRef address, const void *data, void *info) {
    Server *server = (__bridge Server *)info;
    NSLog(@"Hi");
    if (kCFSocketAcceptCallBack == type) {
        ConnectedClient *client = [[ConnectedClient alloc]initWithServer:server data:address];
        [server.connectedClients addClient:client];
        [client startReceiving];
    }
}

- (void)netServiceDidPublish:(NSNetService *)sender {
    self.publishingState = ServerPublishingStatePublished;
    if([self.delegate respondsToSelector:@selector(serverDidPublish:)]) {
        [self.delegate serverDidPublish:self];
    }
}

- (void)netService:(NSNetService *)sender didNotPublish:(NSDictionary *)errorDict {
    self.publishingState = ServerPublishingStateErrorPublishing;
    
    if([self.delegate respondsToSelector:@selector(serverDidNotPublish:reason:)]) {
        [self.delegate serverDidNotPublish:self reason:[Server serverErrorForErrorCode:ServerErrorCodesFailedToPublish extraInformation:errorDict]];
    }
}

- (void)sendStringToAllClients:(NSString *)string {
    [self sendStringToAllClients:string encoding:NSUTF8StringEncoding];
}

- (void)sendStringToAllClients:(NSString *)string encoding:(NSStringEncoding)encoding {
    [self sendString:string encoding:encoding toClient:nil];
}

#if TARGET_OS_IPHONE
- (void)sendImageToAllClients:(UIImage *)image {
    [self sendImageToAllClients:image name:@"Untitled"];
}

- (void)sendImageToAllClients:(UIImage *)image name:(NSString *)name {
    [self sendImage:image name:name toClient:nil];
}
#elif TARGET_OS_MAC
- (void)sendImageToAllClients:(NSImage *)image {
    [self sendImageToAllClients:image name:@"Untitled"];
}

- (void)sendImageToAllClients:(NSImage *)image name:(NSString *)name {
    [self sendImage:image name:name toClient:nil];
}
#endif
- (void)sendFileToAllClients:(NSString *)file {
    [self sendFileToAllClients:file name:[file lastPathComponent]];
}

- (void)sendFileToAllClients:(NSString *)filePath name:(NSString *)name {
    [self sendFile:filePath name:name toClient:nil];
}
    
- (void)sendDictionaryToAllClients:(NSDictionary *)dictionary {
    [self sendDictionary:dictionary toClient:nil];
}
    
- (void)sendArrayToAllClients:(NSArray *)array {
    [self sendArray:array toClient:nil];
}

- (void)sendJSONStringToAllClients:(NSString *)JSON {
    [self sendJSONString:JSON toClient:nil];
}
    
- (void)sendDataToAllClients:(NSData *)data {
    [self sendData:data toClient:nil];
}
    
- (void)sendCommunicationDataToAllClients:(CommunicationData *)communicationData {
    [self sendCommunicationData:communicationData toClient:nil];
}

- (void)sendString:(NSString *)string toClient:(ConnectedClient *)client {
    if(client) {
        [client.connectionHandler sendString:string];
    }
    else {
        for(ConnectedClient *connectedClient in self.connectedClients) {
            [connectedClient.connectionHandler sendString:string];
        }
    }
}

- (void)sendString:(NSString *)string encoding:(NSStringEncoding)encoding toClient:(ConnectedClient *)client {
    if(client) {
        [client.connectionHandler sendString:string encoding:encoding];
    }
    else {
        for(ConnectedClient *connectedClient in self.connectedClients) {
            [connectedClient.connectionHandler sendString:string encoding:encoding];
        }
    }
}

#if TARGET_OS_IPHONE

- (void)sendImage:(UIImage *)image toClient:(ConnectedClient *)client {
    if(client) {
        [client.connectionHandler sendImage:image];
    }
    else {
        for(ConnectedClient *connectedClient in self.connectedClients) {
            [connectedClient.connectionHandler sendImage:image];
        }
    }
}

- (void)sendImage:(UIImage *)image name:(NSString*)name toClient:(ConnectedClient *)client {
    if(client) {
        [client.connectionHandler sendImage:image name:name];
    }
    else {
        for(ConnectedClient *connectedClient in self.connectedClients) {
            [connectedClient.connectionHandler sendImage:image name:name];
        }
    }
}

#elif TARGET_OS_MAC

- (void)sendImage:(NSImage *)image toClient:(ConnectedClient *)client {
    if(client) {
        [client.connectionHandler sendImage:image];
    }
    else {
        for(ConnectedClient *connectedClient in self.connectedClients) {
            [connectedClient.connectionHandler sendImage:image];
        }
    }
}

- (void)sendImage:(NSImage *)image name:(NSString*)name toClient:(ConnectedClient *)client {
    if(client) {
        [client.connectionHandler sendImage:image name:name];
    }
    else {
        for(ConnectedClient *connectedClient in self.connectedClients) {
            [connectedClient.connectionHandler sendImage:image name:name];
        }
    }
}
#endif
    
- (void)sendFile:(NSString *)file toClient:(ConnectedClient *)client {
    if(client) {
        [client.connectionHandler sendFile:file];
    }
    else {
        for(ConnectedClient *connectedClient in self.connectedClients) {
            [connectedClient.connectionHandler sendFile:file];
        }
    }
}

- (void)sendFile:(NSString *)filePath name:(NSString *)name toClient:(ConnectedClient *)client {
    if(client) {
        [client.connectionHandler sendFile:filePath name:name];
    }
    else {
        for(ConnectedClient *connectedClient in self.connectedClients) {
            [connectedClient.connectionHandler sendFile:filePath name:name];
        }
    }
}

- (void)sendDictionary:(NSDictionary *)dictionary toClient:(ConnectedClient *)client {
    if(client) {
        [client.connectionHandler sendDictionary:dictionary];
    }
    else {
        for(ConnectedClient *connectedClient in self.connectedClients) {
            [connectedClient.connectionHandler sendDictionary:dictionary];
        }
    }
}

- (void)sendArray:(NSArray *)array toClient:(ConnectedClient *)client {
    if(client) {
        [client.connectionHandler sendArray:array];
    }
    else {
        for(ConnectedClient *connectedClient in self.connectedClients) {
            [connectedClient.connectionHandler sendArray:array];
        }
    }
}

- (void)sendJSONString:(NSString *)JSON toClient:(ConnectedClient *)client {
    if(client) {
        [client.connectionHandler sendJSONString:JSON];
    }
    else {
        for(ConnectedClient *connectedClient in self.connectedClients) {
            [connectedClient.connectionHandler sendJSONString:JSON];
        }
    }
}

- (void)sendData:(NSData *)data toClient:(ConnectedClient *)client {
    if(client) {
        [client.connectionHandler sendData:data];
    }
    else {
        for(ConnectedClient *connectedClient in self.connectedClients) {
            [connectedClient.connectionHandler sendData:data];
        }
    }
}

- (void)sendCommunicationData:(CommunicationData *)communicationData toClient:(ConnectedClient *)client {
    if(client) {
    [   client.connectionHandler sendCommunicationData:communicationData];
    }
    else {
        for(ConnectedClient *connectedClient in self.connectedClients) {
            [connectedClient.connectionHandler sendCommunicationData:communicationData];
        }
    }
}

- (void)republishAndRestartListening {
    [self republish];
    [self restartListening];
}

- (void)republish {
    [self unpublish];
    [self publish];
}

- (void)restartListening {
    [self stopListening];
    [self startListening];
}

- (void)stop {
    [self unpublish];
    [self stopListening];
}

- (void)dealloc {
    [self stop];
}
    
- (void)unpublish {
    if(self.listeningState == ServerPublishingStatePublished || ServerPublishingStatePublishing) {
        self.publishingState = ServerPublishingStateUnpublished;
        [self.publishedService stop];
        self.publishedService = nil;
        
        if([self.delegate respondsToSelector:@selector(serverDidUnpublish:)]) {
            [self.delegate serverDidUnpublish:self];
        }
    }
}

- (void)stopListening {
    if(self.listeningState != ServerListeningStateStoppedListening) {
        self.listeningState = ServerListeningStateStoppedListening;
        
        if(self.ipv4Socket != NULL) {
            CFSocketInvalidate(self.ipv4Socket);
            CFRelease(self.ipv4Socket);
            self.ipv4Socket = NULL;
        }
        if(self.ipv6Socket != NULL) {
            CFSocketInvalidate(self.ipv6Socket);
            CFRelease(self.ipv6Socket);
            self.ipv6Socket = NULL;
        }
        
        if([self.delegate respondsToSelector:@selector(serverDidStopListening:)]) {
            [self.delegate serverDidStopListening:self];
        }
    }
}

+ (NSError *)serverErrorForErrorCode:(ServerErrorCode)serverErrorCode extraInformation:(NSDictionary *)extraInformation {
    NSString *errorDescription = @"Unknown error";
    if(serverErrorCode == ServerErrorCodesPublishing) {
        errorDescription = @"Server failed to publish. Server is already publishing. Call [Server -unpublish] to stop publishing.";
    }
    else if(serverErrorCode == ServerErrorCodesAlreadyPublished) {
        errorDescription = @"Server failed to publish. Server has already published. Call [Server -unpublish] to unpublish the server.";
    }
    else if(serverErrorCode == ServerErrorCodesFailedToPublish) {
        NSInteger errorCode = [[extraInformation objectForKey:NSNetServicesErrorCode] integerValue];
        NSString *coreErrorMessage = @"Server failed to publish. ";
        NSString *reason = @"";
        
        if(errorCode == NSNetServicesCollisionError) {
            reason = @"A server has already been published with the same domain, type and name that was already present when the publication request was made.";
        }
        else if(errorCode == NSNetServicesActivityInProgress) {
            reason = @"The server has already been published";
        }
        else if(errorCode == NSNetServicesCancelledError) {
            reason = @"The request to publish the server was cancelled.";
        }
        else if(errorCode == NSNetServicesTimeoutError) {
            reason = @"The request to publish the server timed out.";
        }
        else {
            reason = @"An unknown error occurred.";
        }
        
        errorDescription = [coreErrorMessage stringByAppendingString:reason];
    }
    else if(serverErrorCode == ServerErrorCodesAlreadyListening) {
        errorDescription = @"Failed to start listening. The server is already listening. Call [Server -stopListening] to stop the server";
    }
    else if(serverErrorCode == ServerErrorCodesNoSocketAvailable) {
        errorDescription = @"Failed to start listening. No sockets are avaiable to listen on.";
    }
    else if(serverErrorCode == ServerErrorCodesCouldNotBindIPV4Address) {
        errorDescription = @"Failed to start listening. An error occured binding an IPV4 socket.";
    }
    else if(serverErrorCode == ServerErrorCodesCouldNotBindIPV6Address) {
        errorDescription = @"Failed to start listening. An error occured binding an IPV6 socket.";
    }
    NSDictionary *userInfo = @{NSLocalizedDescriptionKey : errorDescription};
    NSError *error = [NSError errorWithDomain:ServerErrorDomain code:serverErrorCode userInfo:userInfo];
    return error;
}

- (ServerInfo *)serverInfo {
    if(!_serverInfo) {
        _serverInfo = [[ServerInfo alloc]init];
    }
    return _serverInfo;
}

- (ProtocolInfo *)protocolInfo {
    if(!_protocolInfo) {
        _protocolInfo = [[ProtocolInfo alloc]init];
    }
    return _protocolInfo;
}

- (ConnectedClients *)connectedClients {
    if(!_connectedClients) {
        _connectedClients = [[ConnectedClients alloc]init];
    }
    return _connectedClients;
}

@end

@implementation NSData (NetworkingAdditions)
    
- (int)port {
    int port;
    struct sockaddr *addr;
    
    addr = (struct sockaddr *)[self bytes];
    if(addr->sa_family == AF_INET) {
        // IPv4 family
        port = ntohs(((struct sockaddr_in *)addr)->sin_port);
    }
    else if(addr->sa_family == AF_INET6) {
        // IPv6 family
        port = ntohs(((struct sockaddr_in6 *)addr)->sin6_port);
    }
    else {
        // The family is neither IPv4 nor IPv6. Can't handle.
        port = 0;
    }
    
    return port;
}
    
    
- (NSString *)host {
    struct sockaddr *addr = (struct sockaddr *)[self bytes];
    if(addr->sa_family == AF_INET) {
        char *address =
        inet_ntoa(((struct sockaddr_in *)addr)->sin_addr);
        if (address) {
            return [NSString stringWithCString: address encoding:NSUTF8StringEncoding];
        }
    }
    else if(addr->sa_family == AF_INET6) {
        struct sockaddr_in6 *addr6 = (struct sockaddr_in6 *)addr;
        char straddr[INET6_ADDRSTRLEN];
        inet_ntop(AF_INET6, &(addr6->sin6_addr), straddr, sizeof(straddr));
        return [NSString stringWithCString: straddr encoding:NSUTF8StringEncoding];
    }
    return nil;
}
    
@end