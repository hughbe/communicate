//
//  ConnectedClient.h
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>
@class Server, ConnectionHandler, CommunicationData;

@interface ConnectedClient : NSObject

@property (weak, readonly, nonatomic) Server *server;
@property (strong, readonly, nonatomic) ConnectionHandler *connectionHandler;

- (instancetype)initWithServer:(Server *)server data:(CFDataRef)data;

- (void)sendData:(CommunicationData *)communicationData;

- (void)startReceiving;
- (void)disconnect;

@end
