//
//  ConnectionsManager.m
//  Communicate
//
//  Created by Hugh Bellamy on 27/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "ConnectionsManager.h"

@interface ConnectionsManager () <ConnectionDelegate>

@property (copy, readwrite, nonatomic) NSArray *connections;
@property (strong, readwrite, nonatomic) NSMutableArray *connecting;

- (void)setupConnection:(Connection *)connection;

@end

@implementation ConnectionsManager

- (instancetype)initWithDelegate:(id<ConnectionsManagerDelegate>)delegate {
    self = [super init];
    if(self) {
        self.delegate = delegate;
    }
    return self;
}

- (void)connectToSocket:(CFSocketNativeHandle)socket {
    Connection *connection = [[Connection alloc]initWithSocket:socket delegate:self];
    [self setupConnection:connection];
}

- (void)connectToNetService:(NSNetService *)service {
    Connection *connection = [[Connection alloc]initWithNetService:service delegate:self];
    [self setupConnection:connection];
}

- (void)setupConnection:(Connection *)connection {
    if(![self.connections containsObject:connection]) {
        connection.delegate = self;
        [connection connect];
        [self.connecting addObject:connection];
    }
}

- (void)disconnect {
    for (Connection *connection in [self.connections copy]) {
        [connection disconnect:YES];
    }
}

- (void)connectionDidStartConnecting:(Connection *)connection {
    if([self.delegate respondsToSelector:@selector(connectionsManager:didStartConnectingToConnection:)]) {
        [self.delegate connectionsManager:self didStartConnectingToConnection:connection];
    }
}

- (void)connectionDidConnect:(Connection *)connection {
    if([self.connecting containsObject:connection]) {
        [self.connecting removeObject:connection];
    }
    
    if(![self.connections containsObject:connection]) {
        self.connections = [self.connections arrayByAddingObject:connection];
    }
    
    if([self.delegate respondsToSelector:@selector(connectionsManager:didConnectToConnection:)]) {
        [self.delegate connectionsManager:self didConnectToConnection:connection];
    }
}

- (void)connectionDidNotConnect:(Connection *)connection reason:(CommunicatorErrorCode)errorCode errorDictionary:(NSDictionary *)errorDictionary {
    if([self.connecting containsObject:connection]) {
        [self.connecting removeObject:connection];
    }
    
    if([self.delegate respondsToSelector:@selector(connectionsManager:didNotConnectToConnection:reason:errorDictionary:)]) {
        [self.delegate connectionsManager:self didNotConnectToConnection:connection reason:errorCode errorDictionary:errorDictionary];
    }
}

- (void)connectionDidDisconnect:(Connection *)connection {
    if([self.connections containsObject:connection]) {
        NSMutableArray *mutableConnections = [self.connections mutableCopy];
        [mutableConnections removeObject:connection];
        self.connections = [NSArray arrayWithArray:mutableConnections];
    }
    if([self.delegate respondsToSelector:@selector(connectionsManager:didDisconnectFromConnection:)]) {
        [self.delegate connectionsManager:self didDisconnectFromConnection:connection];
    }
}

- (void)connectionDidStartReceivingData:(Connection *)connection {
    if([self.delegate respondsToSelector:@selector(connectionsManager:didStartReceivingDataFromConnection:)]) {
        [self.delegate connectionsManager:self didStartReceivingDataFromConnection:connection];
    }
}

- (void)connection:(Connection *)connection didUpdateReceivingData:(CGFloat)completionValue {
    if([self.delegate respondsToSelector:@selector(connectionsManager:didUpdateReceivingData:fromConnection:)]) {
        [self.delegate connectionsManager:self didUpdateReceivingData:completionValue fromConnection:connection];
    }
}

- (void)connection:(Connection *)connection didReceiveData:(CommunicationData *)data {
    if([self.delegate respondsToSelector:@selector(connectionsManager:didReceiveData:fromConnection:)]) {
        [self.delegate connectionsManager:self didReceiveData:data fromConnection:connection];
    }
}

- (void)connection:(Connection *)connection didSendData:(CommunicationData *)data {
    if([self.delegate respondsToSelector:@selector(connectionsManager:didSendData:toConnection:)]) {
        [self.delegate connectionsManager:self didSendData:data toConnection:connection];
    }
}

- (void)sendStringToAllConnections:(NSString *)string {
    [self sendStringToAllConnections:string encoding:NSUTF8StringEncoding];
}

- (void)sendStringToAllConnections:(NSString *)string encoding:(NSStringEncoding)encoding {
    [self sendString:string encoding:encoding toConnection:nil];
}

#if TARGET_OS_IPHONE
- (void)sendImageToAllConnections:(UIImage *)image {
    [self sendImageToAllConnections:image name:@"Untitled"];
}

- (void)sendImageToAllConnections:(UIImage *)image name:(NSString *)name {
    [self sendImage:image name:name toConnection:nil];
}
#elif TARGET_OS_MAC
- (void)sendImageToAllConnections:(NSImage *)image {
    [self sendImageToAllConnections:image name:@"Untitled"];
}

- (void)sendImageToAllConnections:(NSImage *)image name:(NSString *)name {
    [self sendImage:image name:name toConnection:nil];
}
#endif
- (void)sendFileToAllConnections:(NSString *)file {
    [self sendFileToAllConnections:file name:[file lastPathComponent]];
}

- (void)sendFileToAllConnections:(NSString *)filePath name:(NSString *)name {
    [self sendFile:filePath name:name toConnection:nil];
}

- (void)sendDictionaryToAllConnections:(NSDictionary *)dictionary {
    [self sendDictionary:dictionary toConnection:nil];
}

- (void)sendArrayToAllConnections:(NSArray *)array {
    [self sendArray:array toConnection:nil];
}

- (void)sendJSONStringToAllConnections:(NSString *)JSON {
    [self sendJSONString:JSON toConnection:nil];
}

- (void)sendDataToAllConnections:(NSData *)data {
    [self sendData:data toConnection:nil];
}

- (void)sendCommunicationDataToAllConnections:(CommunicationData *)communicationData {
    [self sendCommunicationData:communicationData toConnection:nil];
}

- (void)sendString:(NSString *)string toConnection:(Connection *)connection {
    if(connection) {
        [connection sendString:string];
    }
    else {
        for(Connection *otherConnection in [self.connections copy]) {
            [otherConnection sendString:string];
        }
    }
}

- (void)sendString:(NSString *)string encoding:(NSStringEncoding)encoding toConnection:(Connection *)connection {
    if(connection) {
        [connection sendString:string encoding:encoding];
    }
    else {
        for(Connection *otherConnection in [self.connections copy]) {
            [otherConnection sendString:string encoding:encoding];
        }
    }
}

#if TARGET_OS_IPHONE

- (void)sendImage:(UIImage *)image toConnection:(Connection *)connection {
    if(connection) {
        [connection sendImage:image];
    }
    else {
        for(Connection *otherConnection in [self.connections copy]) {
            [otherConnection sendImage:image];
        }
    }
}

- (void)sendImage:(UIImage *)image name:(NSString*)name toConnection:(Connection *)connection {
    if(connection) {
        [connection sendImage:image name:name];
    }
    else {
        for(Connection *otherConnection in [self.connections copy]) {
            [otherConnection sendImage:image name:name];
        }
    }
}

#elif TARGET_OS_MAC

- (void)sendImage:(NSImage *)image toConnection:(Connection *)connection {
    if(connection) {
        [connection sendImage:image];
    }
    else {
        for(Connection *otherConnection in [self.connections copy]) {
            [otherConnection sendImage:image];
        }
    }
}

- (void)sendImage:(NSImage *)image name:(NSString*)name toConnection:(Connection *)connection {
    if(connection) {
        [connection sendImage:image name:name];
    }
    else {
        for(Connection *otherConnection in [self.connections copy]) {
            [otherConnection sendImage:image name:name];
        }
    }
}
#endif

- (void)sendFile:(NSString *)file toConnection:(Connection *)connection {
    if(connection) {
        [connection sendFile:file];
    }
    else {
        for(Connection *otherConnection in [self.connections copy]) {
            [otherConnection sendFile:file];
        }
    }
}

- (void)sendFile:(NSString *)filePath name:(NSString *)name toConnection:(Connection *)connection {
    if(connection) {
        [connection sendFile:filePath name:name];
    }
    else {
        for(Connection *otherConnection in [self.connections copy]) {
            [otherConnection sendFile:filePath name:name];
        }
    }
}

- (void)sendDictionary:(NSDictionary *)dictionary toConnection:(Connection *)connection {
    if(connection) {
        [connection sendDictionary:dictionary];
    }
    else {
        for(Connection *otherConnection in [self.connections copy]) {
            [otherConnection sendDictionary:dictionary];
        }
    }
}

- (void)sendArray:(NSArray *)array toConnection:(Connection *)connection {
    if(connection) {
        [connection sendArray:array];
    }
    else {
        for(Connection *otherConnection in [self.connections copy]) {
            [otherConnection sendArray:array];
        }
    }
}

- (void)sendJSONString:(NSString *)JSON toConnection:(Connection *)connection {
    if(connection) {
        [connection sendJSONString:JSON];
    }
    else {
        for(Connection *otherConnection in [self.connections copy]) {
            [otherConnection sendJSONString:JSON];
        }
    }
}

- (void)sendData:(NSData *)data toConnection:(Connection *)connection {
    if(connection) {
        [connection sendData:data];
    }
    else {
        for(Connection *otherConnection in [self.connections copy]) {
            [otherConnection sendData:data];
        }
    }
}

- (void)sendCommunicationData:(CommunicationData *)communicationData toConnection:(Connection *)connection {
    if(connection) {
        [connection sendCommunicationData:communicationData];
    }
    else {
        for(Connection *otherConnection in [self.connections copy]) {
            [otherConnection sendCommunicationData:communicationData];
        }
    }
}

- (NSArray *)connections {
    if(!_connections) {
        _connections = [NSArray array];
    }
    return _connections;
}

- (NSMutableArray *)connecting {
    if(!_connecting) {
        _connecting = [NSMutableArray array];
    }
    return _connecting;
}

@end
