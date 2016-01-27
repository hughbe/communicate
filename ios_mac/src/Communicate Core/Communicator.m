//
//  Communicator.m
//  Communicate
//
//  Created by Hugh Bellamy on 27/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "Communicator.h"

#import "ProtocolInfo.h"
#import "CommunicatorInfo.h"

#import "PublishingManager.h"
#import "ListeningManager.h"
#import "SearchingManager.h"
#import "ConnectionsManager.h"

#import "Connection.h"
#import "CommunicationData.h"

NSString* const CommunicatorErrorDomain = @"com.communicator";

@interface Communicator () <PublishingManagerDelegate, ListeningManagerDelegate, SearchingManagerDelegate, ConnectionsManagerDelegate>

@property (strong, readwrite, nonatomic) ProtocolInfo *protocolInfo;
@property (strong, readwrite, nonatomic) CommunicatorInfo *communicatorInfo;

@property (strong, readwrite, nonatomic) PublishingManager *publishingManager;
@property (strong, readwrite, nonatomic) ListeningManager *listeningManager;
@property (strong, readwrite, nonatomic) SearchingManager *searchingManager;
@property (strong, readwrite, nonatomic) ConnectionsManager *connectionManager;

+ (NSError *)errorForErrorCode:(CommunicatorErrorCode)errorCode errorDictionary:(NSDictionary *)errorDictionary;

@end

@implementation Communicator

- (instancetype)initWithProtocolInfo:(ProtocolInfo *)protocolInfo communicatorInfo:(CommunicatorInfo *)communicatorInfo delegate:(id<CommunicatorDelegate>)delegate {
    self = [super init];
    if(self) {
        self.protocolInfo = protocolInfo;
        self.communicatorInfo = communicatorInfo;
        self.delegate = delegate;
    }
    return self;
}

- (void)publish {
    [self.publishingManager publish];
}

- (void)republish {
    [self unpublish];
    [self publish];
}

- (void)publishingManagerDidStartPublishing:(PublishingManager *)publishingManager {
    if([self.delegate respondsToSelector:@selector(communicatorDidStartPublishing:)])
    {
        [self.delegate communicatorDidStartPublishing:self];
    }
}

- (void)publishingManagerDidPublish:(PublishingManager *)publishingManager {
    if([self.delegate respondsToSelector:@selector(communicatorDidPublish:)]) {
        [self.delegate communicatorDidPublish:self];
    }
}

- (void)publishingManagerDidNotPublish:(PublishingManager *)publishingManager reason:(CommunicatorErrorCode)errorCode errorDictionary:(NSDictionary *)errorDictionary {
    if([self.delegate respondsToSelector:@selector(communicatorDidNotPublish:reason:)]) {
        [self.delegate communicatorDidNotPublish:self reason:[Communicator errorForErrorCode:errorCode errorDictionary:errorDictionary]];
    }
}

- (void)publishingManagerDidUnpublish:(PublishingManager *)publishingManager {
    if([self.delegate respondsToSelector:@selector(communicatorDidUnpublish:)]) {
        [self.delegate communicatorDidUnpublish:self];
    }
}

- (void)startListening {
    [self.listeningManager startListening];
}

- (void)restartListening {
    [self stopListening];
    [self startListening];
}

- (void)listeningManagerDidStartListening:(ListeningManager *)listeningManager {
    if([self.delegate respondsToSelector:@selector(communicatorDidStartListening:)]) {
        [self.delegate communicatorDidStartListening:self];
    }
}

- (void)listeningManager:(ListeningManager *)listeningManager didReceiveConnectionRequest:(CFSocketNativeHandle)connectionRequest {
    [self.connectionManager connectToSocket:connectionRequest];
}

- (void)listeningManagerDidNotStartListening:(ListeningManager *)listeningManager errorCode:(CommunicatorErrorCode)errorCode {
    if([self.delegate respondsToSelector:@selector(communicatorDidNotStartListening:reason:)]) {
        [self.delegate communicatorDidNotStartListening:self reason:[Communicator errorForErrorCode:errorCode errorDictionary:nil]];
    }
}

- (void)listeningManagerDidStopListening:(ListeningManager *)listeningManager {
    if([self.delegate respondsToSelector:@selector(communicatorDidStopListening:)]) {
        [self.delegate communicatorDidStopListening:self];
    }
}

- (void)startSearching {
    [self.searchingManager startSearching];
}

- (void)restartSearching {
    [self.searchingManager restartSearching];
}

- (void)connectToService:(NSNetService *)service {
    [self.connectionManager connectToNetService:service];
}

- (void)searchingManagerDidStartSearching:(SearchingManager *)searchingManager {
    if([self.delegate respondsToSelector:@selector(searchingManagerDidStartSearching:)]) {
        [self.delegate communicatorDidStartSearching:self];
    }
}

- (void)searchingManager:(SearchingManager *)searchingManager didFindServices:(NSArray *)services {
    if([self.delegate respondsToSelector:@selector(communicator:didDiscoverServices:)]) {
        [self.delegate communicator:self didDiscoverServices:services];
    }
}

- (void)searchingManagerDidNotStartSearching:(SearchingManager *)searchingManager reason:(CommunicatorErrorCode)errorCode errorDictionary:(NSDictionary *)errorDictionary {
    if([self.delegate respondsToSelector:@selector(communicatorDidNotStartSearching:reason:)]) {
        [self.delegate communicatorDidNotStartSearching:self reason:[Communicator errorForErrorCode:errorCode errorDictionary:errorDictionary]];
    }
}

- (void)searchingManagerDidStopSearching:(SearchingManager *)searchingManager {
    if([self.delegate respondsToSelector:@selector(communicatorDidStopSearching:)]) {
        [self.delegate communicatorDidStopSearching:self];
    }
}

- (void)connectionsManager:(ConnectionsManager *)connectionsManager didStartConnectingToConnection:(Connection *)connection {
    if([self.delegate respondsToSelector:@selector(communicator:didStartConnecting:)]) {
        [self.delegate communicator:self didStartConnecting:connection];
    }
}

- (void)connectionsManager:(ConnectionsManager *)connectionsManager didConnectToConnection:(Connection *)connection {
    if([self.delegate respondsToSelector:@selector(communicator:didConnect:)]) {
        [self.delegate communicator:self didConnect:connection];
    }
}

- (void)connectionsManager:(ConnectionsManager *)connectionsManager didNotConnectToConnection:(Connection *)connection reason:(CommunicatorErrorCode)errorCode errorDictionary:(NSDictionary *)errorDictionary {
    if([self.delegate respondsToSelector:@selector(communicator:didNotConnect:reason:)]) {
        [self.delegate communicator:self didNotConnect:connection reason:[Communicator errorForErrorCode:errorCode errorDictionary:errorDictionary]];
    }
}

- (void)connectionsManager:(ConnectionsManager *)connectionsManager didDisconnectFromConnection:(Connection *)connection {
    if([self.delegate respondsToSelector:@selector(communicator:didDisconnect:)]) {
        [self.delegate communicator:self didDisconnect:connection];
    }
}

- (void)connectionsManager:(ConnectionsManager *)connectionsManager didStartReceivingDataFromConnection:(Connection *)connection {
    if([self.delegate respondsToSelector:@selector(communicator:didStartReceivingDataFromConnection:)]) {
        [self.delegate communicator:self didStartReceivingDataFromConnection:connection];
    }
}

- (void)connectionsManager:(ConnectionsManager *)connectionsManager didUpdateReceivingData:(CGFloat)completionValue fromConnection:(Connection *)connection {
    if([self.delegate respondsToSelector:@selector(communicator:didUpdateReceivingData:fromConnection:)]) {
        [self.delegate communicator:self didUpdateReceivingData:completionValue fromConnection:connection];
    }
}

- (void)connectionsManager:(ConnectionsManager *)connectionsManager didReceiveData:(CommunicationData *)data fromConnection:(Connection *)connection {
    if([self.delegate respondsToSelector:@selector(communicator:didReceiveData:fromConnection:)]) {
        [self.delegate communicator:self didReceiveData:data fromConnection:connection];
    }
}

- (void)connectionsManager:(ConnectionsManager *)connectionsManager didSendData:(CommunicationData *)data toConnection:(Connection *)connection {
    if([self.delegate respondsToSelector:@selector(communicator:didSendData:toConnection:)]) {
        [self.delegate communicator:self didSendData:data toConnection:connection];
    }
}

- (void)dealloc {
    [self stop];
}

- (void)stop {
    [self unpublish];
    [self stopListening];
    [self stopSearching];
    [self disconnect];
    self.publishingManager = nil;
    self.listeningManager = nil;
    self.searchingManager = nil;
    self.connectionManager = nil;
}

- (void)unpublish {
    [_publishingManager unpublish];
}

- (void)stopListening {
    [_listeningManager stopListening];
}

- (void)stopSearching {
    [_searchingManager stopSearching];
}

- (void)disconnect {
    [_connectionManager disconnect];
}

+ (NSError *)errorForErrorCode:(CommunicatorErrorCode)errorCode errorDictionary:(NSDictionary *)errorDictionary {
    NSString *errorDescription = @"Unknown error";
    if(errorCode == CommunicatorErrorCodePublishing) {
        errorDescription = @"Communicator failed to publish. Communicator is already publishing. Call [Communicator -unpublish] to stop publishing.";
    }
    else if(errorCode == CommunicatorErrorCodeAlreadyPublished) {
        errorDescription = @"Communicator failed to publish. Communicator has already published. Call [Communicator -unpublish] to unpublish the server.";
    }
    else if(errorCode == CommunicatorErrorCodeFailedToPublish) {
        NSInteger errorCode = [[errorDictionary objectForKey:NSNetServicesErrorCode] integerValue];
        NSString *coreErrorMessage = @"Communicator failed to publish. ";
        NSString *reason = @"";
        
        if(errorCode == NSNetServicesCollisionError) {
            reason = @"A communicator has already been published with the same domain, type and name that was already present when the publication request was made.";
        }
        else if(errorCode == NSNetServicesActivityInProgress) {
            reason = @"The communicator has already been published";
        }
        else if(errorCode == NSNetServicesCancelledError) {
            reason = @"The request to publish the communicator was cancelled.";
        }
        else if(errorCode == NSNetServicesTimeoutError) {
            reason = @"The request to publish the communicator timed out.";
        }
        else {
            reason = @"An unknown error occurred.";
        }
        
        errorDescription = [coreErrorMessage stringByAppendingString:reason];
    }
    else if(errorCode == CommunicatorErrorCodeAlreadyListening) {
        errorDescription = @"Communicator failed to start listening. The communicator is already listening. Call [Communicator -stopListening] to stop the communicator";
    }
    else if(errorCode == CommunicatorErrorCodeNoSocketAvailable) {
        errorDescription = @"Communicator failed to start listening. No sockets are avaiable to listen on.";
    }
    else if(errorCode == CommunicatorErrorCodeCouldNotBindIPV4Address) {
        errorDescription = @"Communicator failed to start listening. An error occured binding an IPV4 socket.";
    }
    else if(errorCode == CommunicatorErrorCodeAlreadySearching) {
        errorDescription = @"Communicator failed to start searching. The communicator is already searching.";
    }
    else if(errorCode == CommunicatorErrorCodeFailedToSearch) {
        NSInteger errorCode = [[errorDictionary objectForKey:NSNetServicesErrorCode] integerValue];
        NSString *coreErrorMessage = @"Communicator failed to start searching. ";
        NSString *reason = @"";
        
        if(errorCode == NSNetServicesActivityInProgress) {
            reason = @"The communicator is already searching.";
        }
        else if(errorCode == NSNetServicesCancelledError) {
            reason = @"The request to search for devices on the network was cancelled.";
        }
        else {
            reason = @"An unknown error occurred.";
        }
        
        errorDescription = [coreErrorMessage stringByAppendingString:reason];
    }
    else if(errorCode == CommunicatorErrorCodeAlreadyConnected) {
        errorDescription = @"Communicator failed to connect. The communicator is already connected.";
    }
    else if(errorCode == CommunicatorErrorCodeFailedToConnect) {
        NSInteger errorCode = [[errorDictionary objectForKey:NSNetServicesErrorCode] integerValue];
        NSString *coreErrorMessage = @"Communicator failed to connect. ";
        NSString *reason = @"";
        
        if(errorCode == NSNetServicesActivityInProgress) {
            reason = @"The communicator is already connecting.";
        }
        else if(errorCode == NSNetServicesCancelledError) {
            reason = @"The request to connect to a device on the network was cancelled.";
        }
        else if(errorCode == NSNetServicesTimeoutError) {
            reason = @"The request to connect to a device on the network timed out.";
        }
        else {
            reason = @"An unknown error occurred.";
        }
        
        errorDescription = [coreErrorMessage stringByAppendingString:reason];
    }
    NSDictionary *userInfo = @{NSLocalizedDescriptionKey : errorDescription};
    NSError *error = [NSError errorWithDomain:CommunicatorErrorDomain code:errorCode userInfo:userInfo];
    return error;
}

- (ProtocolInfo *)protocolInfo {
    if(!_protocolInfo) {
        _protocolInfo = [[ProtocolInfo alloc]init];
    }
    return _protocolInfo;
}

- (CommunicatorInfo *)communicatorInfo {
    if(!_communicatorInfo) {
        _communicatorInfo = [[CommunicatorInfo alloc]init];
    }
    
    return _communicatorInfo;
}

- (PublishingManager *)publishingManager {
    if(!_publishingManager) {
        _publishingManager = [[PublishingManager alloc]initWithProtocol:self.protocolInfo communicatorInfo:self.communicatorInfo delegate:self];
    }
    return _publishingManager;
}

- (ListeningManager *)listeningManager {
    if(!_listeningManager) {
        _listeningManager = [[ListeningManager alloc]initWithCommunicatorInfo:self.communicatorInfo delegate:self];
    }
    return _listeningManager;
}

- (SearchingManager *)searchingManager {
    if(!_searchingManager) {
        _searchingManager = [[SearchingManager alloc] initWithProtocol:self.protocolInfo delegate:self];
    }
    return _searchingManager;
}

- (ConnectionsManager *)connectionManager {
    if(!_connectionManager) {
        _connectionManager = [[ConnectionsManager alloc]initWithDelegate:self];
    }
    return _connectionManager;
}

@end
