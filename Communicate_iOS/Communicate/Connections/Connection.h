//
//  Connection.h
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

#import "Communicator.h"
@class CommunicationData;

@protocol ConnectionDelegate;

typedef NS_ENUM(NSInteger, ConnectionState) {
    ConnectionStateNotConnected,
    ConnectionStateConnecting,
    ConnectionStateConnected,
    ConnectionStateErrorConnecting,
    ConnectionStateDisconnected
};

@interface Connection : NSObject

@property (weak, nonatomic) id<ConnectionDelegate> delegate;

@property (assign, readonly, nonatomic) ConnectionState connectionState;

@property (strong, readonly, nonatomic) NSInputStream *inputStream;
@property (strong, readonly, nonatomic) NSOutputStream *outputStream;

- (void)connect;
- (void)disconnect:(BOOL)sendTerminationMessage;

- (instancetype)initWithSocket:(CFSocketNativeHandle)socket delegate:(id<ConnectionDelegate>)delegate;
- (instancetype)initWithNetService:(NSNetService *)service delegate:(id<ConnectionDelegate>)delegate;

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

@end

@protocol ConnectionDelegate <NSObject>
@optional
- (void)connectionDidStartConnecting:(Connection *)connection;
- (void)connectionDidConnect:(Connection *)connection;
- (void)connectionDidNotConnect:(Connection *)connection reason:(CommunicatorErrorCode)errorCode errorDictionary:(NSDictionary *)errorDictionary;
- (void)connectionDidDisconnect:(Connection *)connection;

- (void)connection:(Connection *)connection didReceiveData:(CommunicationData *)data;
- (void)connection:(Connection *)connection didSendData:(CommunicationData *)data;

@end