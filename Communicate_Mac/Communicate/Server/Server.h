//
//  Server.h
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>
#if TARGET_OS_IPHONE
@import UIKit;
#elif TARGET_OS_MAC
@import AppKit;
#endif

@class ServerInfo, ProtocolInfo, ConnectedClients, ConnectedClient, CommunicationData;
@protocol ServerDelegate;

typedef NS_ENUM(NSInteger, ServerPublishingState) {
    ServerPublishingStateNotPublished,
    ServerPublishingStatePublishing,
    ServerPublishingStatePublished,
    ServerPublishingStateErrorPublishing,
    ServerPublishingStateUnpublished
};

typedef NS_ENUM(NSInteger, ServerListeningState) {
    ServerListeningStateNotListening,
    ServerListeningStateListening,
    ServerListeningStateErrorListening,
    ServerListeningStateStoppedListening
};

typedef NS_ENUM(NSInteger, ServerErrorCode) {
    ServerErrorCodesPublishing = 500,
    ServerErrorCodesAlreadyPublished,
    ServerErrorCodesFailedToPublish,
    ServerErrorCodesAlreadyListening,
    ServerErrorCodesNoSocketAvailable,
    ServerErrorCodesCouldNotBindIPV4Address,
    ServerErrorCodesCouldNotBindIPV6Address,
    ServerErrorCodeUnknown
};

extern NSString* const ServerErrorDomain;

@interface Server : NSObject <NSNetServiceDelegate>

@property (strong, readonly, nonatomic) ServerInfo *serverInfo;
@property (strong, readonly, nonatomic) ProtocolInfo *protocolInfo;

@property (assign, readonly, nonatomic) ServerPublishingState publishingState;
@property (assign, readonly, nonatomic) ServerListeningState listeningState;

@property (strong, readonly, nonatomic) NSNetService *publishedService;

@property (strong, readonly, nonatomic) ConnectedClients *connectedClients;

@property (assign, nonatomic) id<ServerDelegate> delegate;

- (instancetype)initWithServerInfo:(ServerInfo *)serverInfo protocolInfo:(ProtocolInfo *)protocolInfo delegate:(id<ServerDelegate>)delegate;

- (void)publishAndStartListening;
- (void)publish;
- (void)startListening;

- (void)sendStringToAllClients:(NSString *)string;
- (void)sendStringToAllClients:(NSString *)string encoding:(NSStringEncoding)encoding;

#if TARGET_OS_IPHONE

- (void)sendImageToAllClients:(UIImage *)image;
- (void)sendImageToAllClients:(UIImage *)image name:(NSString*)name;

#elif TARGET_OS_MAC

- (void)sendImageToAllClients:(NSImage *)image;
- (void)sendImageToAllClients:(NSImage *)image name:(NSString*)name;

#endif

- (void)sendFileToAllClients:(NSString *)file;
- (void)sendFileToAllClients:(NSString *)filePath name:(NSString *)name;

- (void)sendDictionaryToAllClients:(NSDictionary *)dictionary;
- (void)sendArrayToAllClients:(NSArray *)array;
- (void)sendJSONStringToAllClients:(NSString *)JSON;

- (void)sendDataToAllClients:(NSData *)data;
- (void)sendCommunicationDataToAllClients:(CommunicationData *)communicationData;

- (void)sendString:(NSString *)string toClient:(ConnectedClient *)client;
- (void)sendString:(NSString *)string encoding:(NSStringEncoding)encoding toClient:(ConnectedClient *)client;

#if TARGET_OS_IPHONE

- (void)sendImage:(UIImage *)image toClient:(ConnectedClient *)client;
- (void)sendImage:(UIImage *)image name:(NSString*)name toClient:(ConnectedClient *)client;

#elif TARGET_OS_MAC

- (void)sendImage:(NSImage *)image toClient:(ConnectedClient *)client;
- (void)sendImage:(NSImage *)image name:(NSString*)name toClient:(ConnectedClient *)client;

#endif

- (void)sendFile:(NSString *)file toClient:(ConnectedClient *)client;
- (void)sendFile:(NSString *)filePath name:(NSString *)name toClient:(ConnectedClient *)client;

- (void)sendDictionary:(NSDictionary *)dictionary toClient:(ConnectedClient *)client;
- (void)sendArray:(NSArray *)array toClient:(ConnectedClient *)client;
- (void)sendJSONString:(NSString *)JSON toClient:(ConnectedClient *)client;

- (void)sendData:(NSData *)data toClient:(ConnectedClient *)client;
- (void)sendCommunicationData:(CommunicationData *)communicationData toClient:(ConnectedClient *)client;

-(void)republishAndRestartListening;
- (void)republish;
- (void)restartListening;

- (void)stop;

- (void)unpublish;
- (void)stopListening;

@end


@protocol ServerDelegate <NSObject>
@optional
- (void)serverDidStartPublishing:(Server *)server;
- (void)serverDidPublish:(Server *)server;
- (void)serverDidNotPublish:(Server *)server reason:(NSError *)error;
- (void)serverDidUnpublish:(Server *)server;

- (void)serverDidStartListening:(Server *)server;
- (void)serverDidNotStartListening:(Server *)server reason:(NSError *)error;
- (void)serverDidStopListening:(Server *)server;;

- (void)server:(Server *)server didStartConnectingToClient:(ConnectedClient *)client;
- (void)server:(Server *)server didConnectToClient:(ConnectedClient *)client;
- (void)server:(Server *)server didNotConnectToClient:(ConnectedClient *)client reason:(NSError *)error;
- (void)server:(Server *)server didDisconnectFromClient:(ConnectedClient *)client;

- (void)server:(Server *)server didSendData:(CommunicationData *)data;
- (void)server:(Server *)server didReceiveData:(CommunicationData *)data;

@end

@interface NSData (NetworkingAdditions)

- (int)port;
- (NSString *)host;

@end