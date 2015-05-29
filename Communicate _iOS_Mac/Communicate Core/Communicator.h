//
//  Communicator.h
//  Communicate
//
//  Created by Hugh Bellamy on 27/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>
@class ProtocolInfo, CommunicatorInfo, PublishingManager, ListeningManager, SearchingManager, ConnectionsManager, Connection, CommunicationData;
@protocol CommunicatorDelegate;

typedef NS_ENUM(NSInteger, CommunicatorErrorCode) {
    CommunicatorErrorCodePublishing = 500,
    CommunicatorErrorCodeAlreadyPublished,
    CommunicatorErrorCodeFailedToPublish,
    CommunicatorErrorCodeAlreadyListening,
    CommunicatorErrorCodeNoSocketAvailable,
    CommunicatorErrorCodeCouldNotBindIPV4Address,
    CommunicatorErrorCodeAlreadySearching,
    CommunicatorErrorCodeFailedToSearch,
    CommunicatorErrorCodeAlreadyConnected,
    CommunicatorErrorCodeFailedToConnect,
    CommunicatorErrorCodeUnknown
};

extern NSString* const CommunicatorErrorDomain;

@interface Communicator : NSObject

@property (strong, readonly, nonatomic) ProtocolInfo *protocolInfo;
@property (strong, readonly, nonatomic) CommunicatorInfo *communicatorInfo;

@property (weak, nonatomic) id<CommunicatorDelegate> delegate;

@property (strong, readonly, nonatomic) PublishingManager *publishingManager;
@property (strong, readonly, nonatomic) ListeningManager *listeningManager;
@property (strong, readonly, nonatomic) SearchingManager *searchingManager;
@property (strong, readonly, nonatomic) ConnectionsManager *connectionManager;

- (instancetype)initWithProtocolInfo:(ProtocolInfo *)protocolInfo communicatorInfo:(CommunicatorInfo *)communicatorInfo delegate:(id<CommunicatorDelegate>)delegate;

- (void)publish;
- (void)republish;

- (void)startListening;
- (void)restartListening;

- (void)startSearching;
- (void)restartSearching;

- (void)connectToService:(NSNetService *)service;

- (void)stop;
- (void)unpublish;
- (void)stopListening;
- (void)stopSearching;
- (void)disconnect;

@end

@protocol CommunicatorDelegate <NSObject>
@optional
- (void)communicatorDidStartPublishing:(Communicator *)communicator;
- (void)communicatorDidPublish:(Communicator *)communicator;
- (void)communicatorDidNotPublish:(Communicator *)communicator reason:(NSError *)reason;
- (void)communicatorDidUnpublish:(Communicator *)communicator;

- (void)communicatorDidStartListening:(Communicator *)communicator;
- (void)communicatorDidNotStartListening:(Communicator *)communicator reason:(NSError *)reason;
- (void)communicatorDidStopListening:(Communicator *)communicator;

- (void)communicatorDidStartSearching:(Communicator *)communicator;
- (void)communicator:(Communicator *)communicator didDiscoverServices:(NSArray *)services;
- (void)communicatorDidNotStartSearching:(Communicator *)communicator reason:(NSError *)reason;
- (void)communicatorDidStopSearching:(Communicator *)communicator;

- (void)communicator:(Communicator *)communicator didStartConnecting:(Connection *)client;
- (void)communicator:(Communicator *)communicator didConnect:(Connection *)client;
- (void)communicator:(Communicator *)communicator didNotConnect:(Connection *)client reason:(NSError *)reason;
- (void)communicator:(Communicator *)communicator didDisconnect:(Connection *)client;

- (void)communicator:(Communicator *)communicator didReceiveData:(CommunicationData *)data fromConnection:(Connection *)connection;
- (void)communicator:(Communicator *)communicator didSendData:(CommunicationData *)data toConnection:(Connection *)connection;

@end