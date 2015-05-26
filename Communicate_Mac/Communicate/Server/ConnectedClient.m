//
//  ConnectedClient.m
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "ConnectedClient.h"
#import "Server.h"
#import "ConnectionHandler.h"
#import "CommunicationData.h"

@interface ConnectedClient() <ConnectionHandlerDelegate>

@property (weak, readwrite, nonatomic) Server *server;
@property (strong, readwrite, nonatomic) ConnectionHandler *connectionHandler;

@end

@implementation ConnectedClient

- (instancetype)initWithServer:(Server *)server data:(CFDataRef)data {
    self = [super init];
    if(self) {
        self.server = server;
        NSData *dataObject = (__bridge NSData *)(data);
        NSString *host = [dataObject host];
        int port = [dataObject port];
        NSLog(@"host: %@; port: %d", host, port);
        [self.connectionHandler connectToAddress:host port:port];
    }
    return self;
}

- (void)startReceiving {
    [self.connectionHandler startReceiving];
}

- (void)sendData:(CommunicationData *)communicationData {
    [self.connectionHandler sendCommunicationData:communicationData];
}

- (void)connectionHandlerDidConnect:(ConnectionHandler *)connectionHandler {
    if([self.server.delegate respondsToSelector:@selector(server:didConnectToClient:)]) {
        [self.server.delegate server:self.server didConnectToClient:self];
    }
}

- (void)connectionHandlerDidDisconnect:(ConnectionHandler *)connectionHandler {
    if([self.server.delegate respondsToSelector:@selector(server:didDisconnectFromClient:)]) {
        [self.server.delegate server:self.server didDisconnectFromClient:self];
    }
}

- (void)connectionHandler:(ConnectionHandler *)connectionHandler didReceiveData:(CommunicationData *)data {
    if([self.server.delegate respondsToSelector:@selector(server:didReceiveData:)]) {
        [self.server.delegate server:self.server didReceiveData:data];
    }
}

- (void)connectionHandler:(ConnectionHandler *)connectionHandler didSendData:(CommunicationData *)data {
    if([self.server.delegate respondsToSelector:@selector(server:didSendData:)]) {
        [self.server.delegate server:self.server didSendData:data];
    }
}

- (void)disconnect {
    
}

- (ConnectionHandler *)connectionHandler {
    if(!_connectionHandler) {
        _connectionHandler = [[ConnectionHandler alloc]initWithDelegate:self];
    }
    return _connectionHandler;
}
@end
