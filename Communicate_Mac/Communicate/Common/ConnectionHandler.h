//
//  DataHandler.h
//  Communicate
//
//  Created by Hugh Bellamy on 25/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>

#if TARGET_OS_IPHONE
@import UIKit;
#elif TARGET_OS_MAC
@import AppKit;
#endif

@protocol ConnectionHandlerDelegate;
@class CommunicationData;

@interface ConnectionHandler : NSObject <NSStreamDelegate>

@property (weak, nonatomic) id<ConnectionHandlerDelegate> delegate;
@property (strong, readonly, nonatomic) NSInputStream *inputStream;
@property (strong, readonly, nonatomic) NSOutputStream *outputStream;

- (instancetype)initWithDelegate:(id<ConnectionHandlerDelegate>)delegate;

- (void)connectToSocket:(CFSocketRef)socket;
- (void)connectToAddress:(NSString *)address port:(int)port;

- (void)startReceiving;
- (void)disconnect;

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

@protocol ConnectionHandlerDelegate <NSObject>

- (void)connectionHandlerDidConnect:(ConnectionHandler *)connectionHandler;
- (void)connectionHandlerDidDisconnect:(ConnectionHandler *)connectionHandler;

- (void)connectionHandler:(ConnectionHandler *)connectionHandler didReceiveData:(CommunicationData *)data;
- (void)connectionHandler:(ConnectionHandler *)connectionHandler didSendData:(CommunicationData *)data;

@end
