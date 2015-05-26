
//
//  Client.m
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "Client.h"
#import "ClientInfo.h"
#import "ProtocolInfo.h"
#import "CommunicationData.h"
#import "DataInfo.h"
#import "DataHeader.h"
#import "DataContent.h"
#import "DataFooter.h"
#import "ConnectionHandler.h"

#include <sys/socket.h>
#include <netinet/in.h>
#include <unistd.h>
#include <arpa/inet.h>

NSString* const ClientErrorDomain = @"client";

@interface Client () <ConnectionHandlerDelegate>

@property (strong, readwrite, nonatomic) ClientInfo *clientInfo;
@property (strong, readwrite, nonatomic) ProtocolInfo *protocolInfo;

@property (assign, readwrite, nonatomic) ClientSearchingState searchingState;
@property (assign, readwrite, nonatomic) ClientConnectedState connectedState;

@property (copy, readwrite, nonatomic) NSArray *services;
@property (strong, readwrite, nonatomic) NSNetService *connectedService;;

@property (strong, nonatomic) NSNetServiceBrowser *serviceBrowser;

@property (strong, nonatomic) ConnectionHandler *connectionHandler;

+ (NSError *)clientErrorForErrorCode:(ClientErrorCode)clientErrorCode extraInformation:(NSDictionary *)extraInformation;

@end

@implementation Client

- (instancetype)initWithClientInfo:(ClientInfo *)clientInfo protocolInfo:(ProtocolInfo *)protocolInfo delegate:(id<ClientDelegate>)delegate {
    self = [super init];
    if(self) {
        self.clientInfo = clientInfo;
        self.protocolInfo = protocolInfo;
        self.delegate = delegate;
    }
    return self;
}

- (void)search {
    if(self.searchingState == ClientSearchingStateSearching) {
        if([self.delegate respondsToSelector:@selector(clientDidNotSearch:reason:)]) {
            [self.delegate clientDidNotSearch:self reason:[Client clientErrorForErrorCode:ClientErrorCodeAlreadySearching extraInformation:nil]];
        }
        return;
    }
    
    self.searchingState = ClientSearchingStateSearching;
    if([self.delegate respondsToSelector:@selector(clientDidStartSearching:)]) {
        [self.delegate clientDidStartSearching:self];
    }
    
    self.services = [NSArray array];
    self.serviceBrowser = [[NSNetServiceBrowser alloc]init];
    self.serviceBrowser.delegate = self;
    [self.serviceBrowser searchForServicesOfType:[self.protocolInfo serializeTypeWithDomain:NO] inDomain:self.protocolInfo.domain];
}

- (void)netServiceBrowser:(NSNetServiceBrowser *)aNetServiceBrowser didFindService:(NSNetService *)aNetService moreComing:(BOOL)moreComing {
    if(![self.services containsObject:aNetService]) {
        self.services = [self.services arrayByAddingObject:aNetService];
    }
    if(!moreComing) {
        if([self.delegate respondsToSelector:@selector(clientDidUpdateServices:)]) {
            [self.delegate clientDidUpdateServices:self];
        }
    }
}

- (void)netServiceBrowser:(NSNetServiceBrowser *)aNetServiceBrowser didRemoveService:(NSNetService *)aNetService moreComing:(BOOL)moreComing {
    if([self.services containsObject:aNetService]) {
        NSMutableArray *mutableServices = [self.services mutableCopy];
        [mutableServices removeObject:aNetService];
        self.services = [NSArray arrayWithArray:mutableServices];
    }
    if(!moreComing) {
        if([self.delegate respondsToSelector:@selector(clientDidUpdateServices:)]) {
            [self.delegate clientDidUpdateServices:self];
        }
    }
}

- (void)netServiceBrowser:(NSNetServiceBrowser *)aNetServiceBrowser didNotSearch:(NSDictionary *)errorDict {
    self.searchingState = ClientSearchingStateErrorSearching;
    if([self.delegate respondsToSelector:@selector(clientDidNotSearch:reason:)]) {
        [self.delegate clientDidNotSearch:self reason:[Client clientErrorForErrorCode:ClientErrorCodeFailedToSearch extraInformation:errorDict]];
    }
}

- (void)netServiceBrowserDidStopSearch:(NSNetServiceBrowser *)aNetServiceBrowser {
    self.searchingState = ClientSearchingStateStoppedSearching;
    if([self.delegate respondsToSelector:@selector(clientDidStopSearching:)]) {
        [self.delegate clientDidStopSearching:self];
    }
}

- (void)connectToService:(NSNetService *)service {
    self.connectedState = ClientConnectedStateConnecting;
    if([self.delegate respondsToSelector:@selector(clientDidStartConnecting:)]) {
        [self.delegate clientDidStartConnecting:self];
    }
    service.delegate = self;
    [service resolveWithTimeout:5];
}

- (void)netServiceDidResolveAddress:(NSNetService *)service {
    char addressBuffer[INET6_ADDRSTRLEN];
    
    for (NSData *data in service.addresses)
    {
        memset(addressBuffer, 0, INET6_ADDRSTRLEN);
        
        typedef union {
            struct sockaddr sa;
            struct sockaddr_in ipv4;
            struct sockaddr_in6 ipv6;
        } ip_socket_address;
        
        ip_socket_address *socketAddress = (ip_socket_address *)[data bytes];
        
        if (socketAddress && (socketAddress->sa.sa_family == AF_INET))// || socketAddress->sa.sa_family == AF_INET6))
        {
            const char *addressStr = inet_ntop(socketAddress->sa.sa_family,
                                               (void *)&(socketAddress->ipv4.sin_addr),
                                               addressBuffer,
                                               sizeof(addressBuffer));
            
            int port = ntohs(socketAddress->ipv4.sin_port);
            
            if (addressStr && port)
            {
                [self.connectionHandler connectToAddress:[NSString stringWithCString:addressStr encoding:NSUTF8StringEncoding] port:port];
                [self.connectionHandler startReceiving];
                break;
            }
        }
    }
    
    self.connectedService = service;
}

- (void)netService:(NSNetService *)sender didNotResolve:(NSDictionary *)errorDict {
    self.connectedState = ClientConnectedStateErrorConnecting;
    if([self.delegate respondsToSelector:@selector(clientDidNotConnect:reason:)]) {
        [self.delegate clientDidNotConnect:self reason:[Client clientErrorForErrorCode:ClientErrorCodeErrorConnecting extraInformation:errorDict]];
    }
}

- (void)sendString:(NSString *)string {
    [self.connectionHandler sendString:string];
}

- (void)sendString:(NSString *)string encoding:(NSStringEncoding)encoding {
    [self.connectionHandler sendString:string encoding:encoding];
}

#if TARGET_OS_IPHONE

- (void)sendImage:(UIImage *)image {
    [self.connectionHandler sendImage:image name:@"Untitled"];
}
- (void)sendImage:(UIImage *)image name:(NSString*)name {
    [self.connectionHandler sendImage:image name:name];
}

#elif TARGET_OS_MAC

- (void)sendImage:(NSImage *)image {
    [self.connectionHandler sendImage:image name:@"Untitled"];
}

- (void)sendImage:(NSImage *)image name:(NSString*)name {
    [self.connectionHandler sendImage:image name:name];
}

#endif

- (void)sendFile:(NSString *)file {
    [self.connectionHandler sendFile:file name:[file lastPathComponent]];
}

- (void)sendFile:(NSString *)filePath name:(NSString *)name {
    [self.connectionHandler sendFile:filePath name:name];
}

- (void)sendDictionary:(NSDictionary *)dictionary {
    [self.connectionHandler sendDictionary:dictionary];
}

- (void)sendArray:(NSArray *)array {
    [self.connectionHandler sendArray:array];
}

- (void)sendJSONString:(NSString *)JSON {
    [self.connectionHandler sendJSONString:JSON];
}

- (void)sendData:(NSData *)data {
    [self.connectionHandler sendData:data];
}

- (void)sendCommunicationData:(CommunicationData *)communicationData {
    [self.connectionHandler sendCommunicationData:communicationData];
}

- (void)connectionHandlerDidConnect:(ConnectionHandler *)connectionHandler {
     self.connectedState = ClientConnectedStateConnected;
     if([self.delegate respondsToSelector:@selector(clientDidConnect:)]) {
         [self.delegate clientDidConnect:self];
     }
}

- (void)connectionHandlerDidDisconnect:(ConnectionHandler *)connectionHandler {
    if(self.connectedState == ClientConnectedStateConnected) {
        self.connectedState = ClientConnectedStateErrorConnecting;
        if([self.delegate respondsToSelector:@selector(clientDidDisconnect:)]) {
            [self.delegate clientDidDisconnect:self];
        }
    }
}

- (void)connectionHandler:(ConnectionHandler *)connectionHandler didReceiveData:(CommunicationData *)data {
    if([self.delegate respondsToSelector:@selector(client:didReceiveData:)]) {
        [self.delegate client:self didReceiveData:data];
    }
}

- (void)connectionHandler:(ConnectionHandler *)connectionHandler didSendData:(CommunicationData *)data {
    if([self.delegate respondsToSelector:@selector(client:didSendData:)]) {
        [self.delegate client:self didSendData:data];
    }
}

- (void)disconnect {
    if(self.connectedState == ClientConnectedStateConnected) {
        self.connectedState = ClientConnectedStateDisconnected;
    }
    [self.connectionHandler disconnect];
    self.connectionHandler.delegate = nil;
    self.connectionHandler = nil;
    
    [self.connectedService stop];
    self.connectedService.delegate = nil;
    self.connectedService = nil;
}

+ (NSError *)clientErrorForErrorCode:(ClientErrorCode)clientErrorCode extraInformation:(NSDictionary *)extraInformation {
    NSString *errorDescription = @"Unknown error";
    if(clientErrorCode == ClientErrorCodeAlreadySearching) {
        errorDescription = @"Client is already searchign. Call [Client -stopSearching] to stop searching.";
    }
    else if(clientErrorCode == ClientErrorCodeFailedToSearch) {
        NSInteger errorCode = [[extraInformation objectForKey:NSNetServicesErrorCode] integerValue];
        NSString *coreErrorMessage = @"Client failed to search. ";
        NSString *reason = @"";
        
        if(errorCode == NSNetServicesActivityInProgress) {
            reason = @"The client is already searching.";
        }
        else if(errorCode == NSNetServicesCancelledError) {
            reason = @"The request to search for a server was cancelled.";
        }
        else {
            reason = @"An unknown error occurred.";
        }
        
        errorDescription = [coreErrorMessage stringByAppendingString:reason];
    }
    else if(ClientErrorCodeErrorConnecting) {
        NSInteger errorCode = [[extraInformation objectForKey:NSNetServicesErrorCode] integerValue];
        NSString *coreErrorMessage = @"Client failed to connect. ";
        NSString *reason = @"";
        
        if(errorCode == NSNetServicesActivityInProgress) {
            reason = @"The client is already connecting.";
        }
        else if(errorCode == NSNetServicesCancelledError) {
            reason = @"The request to connect to a server was cancelled.";
        }
        else if(errorCode == NSNetServicesTimeoutError) {
            reason = @"The request to connect to a server timed out.";
        }
        else {
            reason = @"An unknown error occurred.";
        }
        
        errorDescription = [coreErrorMessage stringByAppendingString:reason];
    }
    
    NSDictionary *userInfo = @{NSLocalizedDescriptionKey : errorDescription};
    NSError *error = [NSError errorWithDomain:ClientErrorDomain code:clientErrorCode userInfo:userInfo];
    return error;
}

- (NSString *)description {
    return [NSString stringWithFormat:@"Client: %@; %@", self.clientInfo, self.protocolInfo];
}

- (ClientInfo *)clientInfo {
    if(!_clientInfo) {
        _clientInfo = [[ClientInfo alloc]init];
    }
    return _clientInfo;
}

- (ProtocolInfo *)protocolInfo {
    if(!_protocolInfo) {
        _protocolInfo = [[ProtocolInfo alloc]init];
    }
    return _protocolInfo;
}

- (NSArray *)services {
    if(!_services) {
        _services = [NSArray array];
    }
    return _services;
}

- (ConnectionHandler *)connectionHandler {
    if(!_connectionHandler) {
        _connectionHandler = [[ConnectionHandler alloc]initWithDelegate:self];
    }
    return _connectionHandler;
}

@end
