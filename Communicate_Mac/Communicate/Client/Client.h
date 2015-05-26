//
//  Client.h
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>

#import "CommunicationDataType.h"

@class CommunicationData;

#if TARGET_OS_IPHONE
@import UIKit;
#elif TARGET_OS_MAC
@import AppKit;
#endif

@class ClientInfo, ProtocolInfo;
@protocol ClientDelegate;

typedef NS_ENUM(NSInteger, ClientSearchingState) {
    ClientSearchingStateNotSearching,
    ClientSearchingStateSearching,
    ClientSearchingStateErrorSearching,
    ClientSearchingStateStoppedSearching
};

typedef NS_ENUM(NSInteger, ClientConnectedState) {
    ClientConnectedStateNotConnected,
    ClientConnectedStateConnecting,
    ClientConnectedStateConnected,
    ClientConnectedStateErrorConnecting,
    ClientConnectedStateDisconnected
};

typedef NS_ENUM(NSInteger, ClientErrorCode) {
    ClientErrorCodeAlreadySearching,
    ClientErrorCodeFailedToSearch,
    ClientErrorCodeErrorConnecting
};

extern NSString* const ClientErrorDomain;

@interface Client : NSObject <NSNetServiceDelegate, NSNetServiceBrowserDelegate>

@property (strong, readonly, nonatomic) ClientInfo *clientInfo;
@property (strong, readonly, nonatomic) ProtocolInfo *protocolInfo;

@property (weak, nonatomic) id<ClientDelegate> delegate;

@property (assign, readonly, nonatomic) ClientSearchingState searchingState;
@property (assign, readonly, nonatomic) ClientConnectedState connectedState;

@property (copy, readonly, nonatomic) NSArray *services;
@property (strong, readonly, nonatomic) NSNetService *connectedService;

- (instancetype)initWithClientInfo:(ClientInfo *)clientInfo protocolInfo:(ProtocolInfo *)protocolInfo delegate:(id<ClientDelegate>)delegate;

- (void)search;
- (void)connectToService:(NSNetService *)service;

- (void)sendString:(NSString *)string;
- (void)sendString:(NSString *)string encoding:(NSStringEncoding)encoding;

#if TARGET_OS_IPHONE

- (void)sendImage:(UIImage *)image;
- (void)sendImage:(UIImage *)image name:(NSString*)name;

#elif TARGET_OS_MAC

- (void)sendImage:(NSImage *)image;
- (void)sendImage:(NSImage *)image name:(NSString*)name;

#endif

- (void)sendFile:(NSString *)file;
- (void)sendFile:(NSString *)filePath name:(NSString *)name;

- (void)sendDictionary:(NSDictionary *)dictionary;
- (void)sendArray:(NSArray *)array;
- (void)sendJSONString:(NSString *)JSON;

- (void)sendData:(NSData *)data;
- (void)sendCommunicationData:(CommunicationData *)communicationData;

- (void)disconnect;

@end

@protocol ClientDelegate <NSObject>
@optional
- (void)clientDidStartSearching:(Client *)client;
- (void)clientDidNotSearch:(Client *)client reason:(NSError *)reason;
- (void)clientDidStopSearching:(Client *)client;
- (void)clientDidUpdateServices:(Client *)client;

- (void)clientDidStartConnecting:(Client *)client;
- (void)clientDidNotConnect:(Client *)client reason:(NSError *)reason;
- (void)clientDidConnect:(Client *)client;
- (void)clientDidDisconnect:(Client *)client;

- (void)client:(Client *)client didReceiveData:(CommunicationData *)data;
- (void)client:(Client *)client didSendData:(CommunicationData *)data;

@end
