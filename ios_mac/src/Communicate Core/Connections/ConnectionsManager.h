//
//  ConnectionsManager.h
//  Communicate
//
//  Created by Hugh Bellamy on 27/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>

#if TARGET_OS_IPHONE
@import UIKit;
#elif TARGET_OS_MAC
@import AppKit;
#endif

#import "Connection.h"
#import "Communicator.h"

@class CommunicationData;

@protocol ConnectionsManagerDelegate;

@interface ConnectionsManager : NSObject

@property (weak, nonatomic) id<ConnectionsManagerDelegate> delegate;

@property (copy, readonly, nonatomic) NSArray *connections;

- (instancetype)initWithDelegate:(id<ConnectionsManagerDelegate>)delegate;

//TODO
- (void)connectToSocket:(CFSocketNativeHandle)socket;
- (void)connectToNetService:(NSNetService *)service;

- (void)disconnect;

- (void)sendStringToAllConnections:(NSString *)string;
- (void)sendStringToAllConnections:(NSString *)string encoding:(NSStringEncoding)encoding;

#if TARGET_OS_IPHONE

- (void)sendImageToAllConnections:(UIImage *)image;
- (void)sendImageToAllConnections:(UIImage *)image name:(NSString*)name;

#elif TARGET_OS_MAC

- (void)sendImageToAllConnections:(NSImage *)image;
- (void)sendImageToAllConnections:(NSImage *)image name:(NSString*)name;

#endif

- (void)sendFileToAllConnections:(NSString *)file;
- (void)sendFileToAllConnections:(NSString *)filePath name:(NSString *)name;

- (void)sendDictionaryToAllConnections:(NSDictionary *)dictionary;
- (void)sendArrayToAllConnections:(NSArray *)array;
- (void)sendJSONStringToAllConnections:(NSString *)JSON;

- (void)sendDataToAllConnections:(NSData *)data;
- (void)sendCommunicationDataToAllConnections:(CommunicationData *)communicationData;

- (void)sendString:(NSString *)string toConnection:(Connection *)connection;
- (void)sendString:(NSString *)string encoding:(NSStringEncoding)encoding toConnection:(Connection *)connection;

#if TARGET_OS_IPHONE

- (void)sendImage:(UIImage *)image toConnection:(Connection *)connection;
- (void)sendImage:(UIImage *)image name:(NSString*)name toConnection:(Connection *)connection;

#elif TARGET_OS_MAC

- (void)sendImage:(NSImage *)image toConnection:(Connection *)connection;
- (void)sendImage:(NSImage *)image name:(NSString*)name toConnection:(Connection *)connection;

#endif

- (void)sendFile:(NSString *)file toConnection:(Connection *)connection;
- (void)sendFile:(NSString *)filePath name:(NSString *)name toConnection:(Connection *)connection;

- (void)sendDictionary:(NSDictionary *)dictionary toConnection:(Connection *)connection;
- (void)sendArray:(NSArray *)array toConnection:(Connection *)connection;
- (void)sendJSONString:(NSString *)JSON toConnection:(Connection *)connection;

- (void)sendData:(NSData *)data toConnection:(Connection *)connection;
- (void)sendCommunicationData:(CommunicationData *)communicationData toConnection:(Connection *)connection;

@end

@protocol ConnectionsManagerDelegate <NSObject>

- (void)connectionsManager:(ConnectionsManager *)connectionsManager didStartConnectingToConnection:(Connection *)connection;
- (void)connectionsManager:(ConnectionsManager *)connectionsManager didConnectToConnection:(Connection *)connection;
- (void)connectionsManager:(ConnectionsManager *)connectionsManager didNotConnectToConnection:(Connection *)connection reason:(CommunicatorErrorCode)errorCode errorDictionary:(NSDictionary *)errorDictionary;
- (void)connectionsManager:(ConnectionsManager *)connectionsManager didDisconnectFromConnection:(Connection *)connection;

- (void)connectionsManager:(ConnectionsManager *)connectionsManager didStartReceivingDataFromConnection:(Connection *)connection;
- (void)connectionsManager:(ConnectionsManager *)connectionsManager didUpdateReceivingData:(CGFloat)completionValue fromConnection:(Connection *)connection;

- (void)connectionsManager:(ConnectionsManager *)connectionsManager didReceiveData:(CommunicationData *)data fromConnection:(Connection *)connection;
- (void)connectionsManager:(ConnectionsManager *)connectionsManager didSendData:(CommunicationData *)data toConnection:(Connection *)connection;

@end
