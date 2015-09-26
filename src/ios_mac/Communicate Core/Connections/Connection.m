//
//  Connection.m
//  Communicate
//
//  Created by Hugh Bellamy on 27/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "Connection.h"
#import "CommunicationData.h"
#import "DataInfo.h"
#import "DataHeader.h"
#import "DataContent.h"
#import "DataFooter.h"
#import <arpa/inet.h>

@interface Connection () <NSNetServiceDelegate, NSStreamDelegate>

@property (assign, readwrite, nonatomic) ConnectionState connectionState;

@property (strong, readwrite, nonatomic) NSInputStream *inputStream;
@property (strong, readwrite, nonatomic) NSOutputStream *outputStream;

@property (assign, nonatomic) CFSocketNativeHandle socket;;
@property (strong, nonatomic) NSNetService *netService;

- (void)setupWithReadStream:(CFReadStreamRef)readStream writeStream:(CFWriteStreamRef)writeStream;


@property (strong, nonatomic) NSRunLoop *runLoop;

@end

@implementation Connection

//TODO
- (instancetype)initWithSocket:(CFSocketNativeHandle)socket delegate:(id<ConnectionDelegate>)delegate {
    self = [super init];
    if(self) {
        self.socket = socket;
        self.delegate = delegate;
        
        self.connectionState = ConnectionStateNotConnected;
    }
    return self;
}

- (instancetype)initWithNetService:(NSNetService *)service delegate:(id<ConnectionDelegate>)delegate {
    self = [super init];
    if(self) {
        self.netService = service;
        self.netService.delegate = self;
        self.delegate = delegate;
        
        self.connectionState = ConnectionStateNotConnected;
    }
    return self;
}

- (void)connectToSocket:(CFSocketNativeHandle)socketHandle {
    CFReadStreamRef readStream = NULL;
    CFWriteStreamRef writeStream = NULL;
    CFStreamCreatePairWithSocket(kCFAllocatorDefault, socketHandle,
                                 &readStream, &writeStream);
    [self setupWithReadStream:readStream writeStream:writeStream];
}

- (void)connectToAddress:(NSString *)address port:(int)port {
    CFReadStreamRef readStream;
    CFWriteStreamRef writeStream;
    CFStreamCreatePairWithSocketToHost(NULL, (__bridge CFStringRef)(address), port, &readStream, &writeStream);
    [self setupWithReadStream:readStream writeStream:writeStream];
}

- (void)setupWithReadStream:(CFReadStreamRef)readStream writeStream:(CFWriteStreamRef)writeStream {
    self.connectionState = ConnectionStateConnected;
    
    self.inputStream = (__bridge NSInputStream *)readStream;
    self.outputStream = (__bridge NSOutputStream *)writeStream;
    
    self.inputStream.delegate = self;
    self.outputStream.delegate = self;
    // here: change the queue type and use a background queue (you can change priority)
    dispatch_queue_t queue = dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0);
    dispatch_async(queue, ^ {
        [self.inputStream scheduleInRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
        [self.outputStream scheduleInRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
        
        [self.inputStream open];
        [self.outputStream open];
        self.runLoop = [NSRunLoop currentRunLoop];
        dispatch_async(dispatch_get_main_queue(), ^ {
            if([self.delegate respondsToSelector:@selector(connectionDidConnect:)]) {
                [self.delegate connectionDidConnect:self];
            }
        });
        [[NSRunLoop currentRunLoop] run];
    });
    
    if (readStream != NULL) {
        CFRelease(readStream);
    }
    if(writeStream != NULL) {
        CFRelease(writeStream);
    }
}

- (void)connect {
    if(self.connectionState == ConnectionStateConnected) {
        if([self.delegate respondsToSelector:@selector(connectionDidNotConnect:reason:errorDictionary:)]) {
            [self.delegate connectionDidNotConnect:self reason:CommunicatorErrorCodeAlreadyConnected errorDictionary:nil];
        }
        return;
    }
    if(self.connectionState != ConnectionStateConnecting) {
        self.connectionState = ConnectionStateConnecting;
        if([self.delegate respondsToSelector:@selector(connectionDidStartConnecting:)]) {
            [self.delegate connectionDidStartConnecting:self];
        }
    }
    
    if(self.socket) {
        CFReadStreamRef readStream = NULL;
        CFWriteStreamRef writeStream = NULL;
        CFStreamCreatePairWithSocket(NULL, self.socket, &readStream, &writeStream);
        
        if (readStream && writeStream) {
            [self setupWithReadStream:readStream writeStream:writeStream];
        }
    }
    else {
        [self.netService resolveWithTimeout:10];
    }
}

- (void)netServiceDidResolveAddress:(NSNetService *)service {
    if(!self.netService) {
        return;
    }
    self.netService.delegate = nil;
    self.netService = nil;
    char addressBuffer[INET6_ADDRSTRLEN];
    
    for (NSData *data in service.addresses)
    {
        memset(addressBuffer, 0, INET6_ADDRSTRLEN);
        
        typedef union {
            struct sockaddr sa;
            struct sockaddr_in ipv4;
            struct sockaddr_in6 ipv6;
        } ip_socket_address;
        
        ip_socket_address *socketAddress = (ip_socket_address *)[data bytes];
        
        if (socketAddress && (socketAddress->sa.sa_family == AF_INET))// || socketAddress->sa.sa_family == AF_INET6))
        {
            const char *addressStr = inet_ntop(socketAddress->sa.sa_family,
                                               (void *)&(socketAddress->ipv4.sin_addr),
                                               addressBuffer,
                                               sizeof(addressBuffer));
            
            int port = ntohs(socketAddress->ipv4.sin_port);
            
            if (addressStr && port)
            {
                [self connectToAddress:[NSString stringWithCString:addressStr encoding:NSUTF8StringEncoding] port:port];
                break;
            }
        }
    }
}

- (void)netService:(NSNetService *)sender didNotResolve:(NSDictionary *)errorDict {
    self.connectionState = ConnectionStateErrorConnecting;
    if([self.delegate respondsToSelector:@selector(connectionDidNotConnect:reason:errorDictionary:)]) {
        [self.delegate connectionDidNotConnect:self reason:CommunicatorErrorCodeFailedToConnect errorDictionary:errorDict];
    }
}

- (void)stream:(NSStream *)aStream handleEvent:(NSStreamEvent)eventCode {
    if(eventCode == NSStreamEventEndEncountered || eventCode == NSStreamEventErrorOccurred) {
        if([self.delegate respondsToSelector:@selector(connectionDidDisconnect:)]) {
            [self.delegate connectionDidDisconnect:self];
        }
    }
    else if(eventCode == NSStreamEventHasBytesAvailable) {
        NSUInteger infoLength = [DataInfo length];
        
        uint8_t infoBuffer[infoLength];
        [self.inputStream read:infoBuffer maxLength:infoLength];
        NSData *infoData = [[NSData alloc]initWithBytes:(const void *)infoBuffer length:infoLength];
        DataInfo *dataInfo = [[DataInfo alloc]initWithData:infoData];
        if(dataInfo.dataType == CommunicationDataTypeTermination) {
            [self disconnect:NO];
            return;
        }
        
        if([self.delegate respondsToSelector:@selector(connectionDidStartReceivingData:)]) {
            dispatch_async(dispatch_get_main_queue(),^ {
                [self.delegate connectionDidStartReceivingData:self];
            });
        }
        
        NSInteger headerLength = dataInfo.headerLength;
        NSData *headerData = [self read:headerLength callback:nil];
        
        DataHeader *header = [[DataHeader alloc]initWithData:headerData];
        
        NSInteger contentLength = dataInfo.contentLength;
        NSData *contentData = [self read:contentLength callback:^(NSInteger bytesRead, NSInteger length) {
            if([self.delegate respondsToSelector:@selector(connection:didUpdateReceivingData:)]) {
                CGFloat completion = (CGFloat)(bytesRead)/(CGFloat)(length);
                dispatch_async(dispatch_get_main_queue(),^ {
                    [self.delegate connection:self didUpdateReceivingData:completion];
                });
            }
        }];
        
        DataContent *content = [[DataContent alloc]initWithData:contentData];
        
        NSInteger footerLength = dataInfo.footerLength;
        NSData *footerData = [self read:footerLength callback:nil];
        
        DataFooter *footer = [[DataFooter alloc]initWithData:footerData];
        
        CommunicationData *receivedData = [[CommunicationData alloc]initWithType:dataInfo.dataType header:header content:content footer:footer];
        dispatch_async(dispatch_get_main_queue(),^ {
            if([self.delegate respondsToSelector:@selector(connection:didReceiveData:)]) {
                [self.delegate connection:self didReceiveData:receivedData];
            }
        });
    }
}

- (NSData *)read:(NSInteger)length callback:(void(^)(NSInteger bytesRead, NSInteger length))callback {
    NSMutableData *data = [[NSMutableData alloc]init];
    if(length > 0) {
        NSInteger maxPacketSize = MIN(length, 4096);
        NSInteger bytesRead = 0;
        
        while (bytesRead < length) {
            uint8_t buffer[maxPacketSize];
            NSInteger readResult = [self.inputStream read:buffer maxLength:maxPacketSize];
            if(readResult > 0) {
                bytesRead += readResult;
                [data appendBytes:buffer length:readResult];
            }
            else {
                return nil;
            }
            if(callback) {
                callback(bytesRead, length);
            }
        }
    }
    return data;
}

- (void)sendString:(NSString *)string {
    [self sendString:string encoding:NSUTF8StringEncoding];
}

- (void)sendString:(NSString *)string encoding:(NSStringEncoding)encoding {
    [self sendCommunicationData:[CommunicationData fromString:string withEncoding:encoding]];
}

#if TARGET_OS_IPHONE

- (void)sendImage:(UIImage *)image {
    [self sendImage:image name:@"Untitled"];
}

- (void)sendImage:(UIImage *)image name:(NSString*)name {
    [self sendCommunicationData:[CommunicationData fromImage:image name:name]];
}

#elif TARGET_OS_MAC

- (void)sendImage:(NSImage *)image {
    [self sendImage:image name:@"Untitled"];
}

- (void)sendImage:(NSImage *)image name:(NSString*)name {
    [self sendCommunicationData:[CommunicationData fromImage:image name:name]];
}

#endif

- (void)sendFile:(NSString *)file {
    [self sendFile:file name:[file lastPathComponent]];
}

- (void)sendFile:(NSString *)filePath name:(NSString *)name {
    [self sendCommunicationData:[CommunicationData fromFile:filePath name:name]];
}

- (void)sendDictionary:(NSDictionary *)dictionary {
    [self sendCommunicationData:[CommunicationData fromDictionary:dictionary]];
}

- (void)sendArray:(NSArray *)array {
    [self sendCommunicationData:[CommunicationData fromArray:array]];
}

- (void)sendJSONString:(NSString *)JSON {
    NSData *data = [JSON dataUsingEncoding:NSUTF8StringEncoding];
    
    id JSONObject = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingAllowFragments error:nil];
    if(!JSONObject) {
        [self sendString:JSON];
    }
    else {
        JSONObjectType objectType = JSONObjectTypeOther;
        if([JSONObject isKindOfClass:[NSArray class]]) {
            objectType = JSONObjectTypeArray;
        }
        else if([JSONObject isKindOfClass:[NSDictionary class]]) {
            objectType = JSONObjectTypeDictionary;
        }
        [self sendCommunicationData:[CommunicationData fromJSONData:data JSONObjectType:objectType]];
    }
}

- (void)sendData:(NSData *)data {
    [self sendCommunicationData:[CommunicationData fromData:data]];
}

- (void)sendCommunicationData:(CommunicationData *)communicationData {
    if(!communicationData) {
        return;
    }
    
    [self.outputStream write:[[communicationData.dataInfo getData] bytes] maxLength:[DataInfo length]];
    if(communicationData.header.length > 0) {
        NSInteger headerBytesWritten = 0;
        NSData *headerData = [communicationData.header getData];
        while (headerData.length > headerBytesWritten )
        {
            while (!self.outputStream.hasSpaceAvailable) {
                [NSThread sleepForTimeInterval:0.05];
            }
            
            NSInteger writeResult = [self.outputStream write:[headerData bytes] + headerBytesWritten maxLength:communicationData.header.length - headerBytesWritten];
            if (writeResult != -1 ) {
                headerBytesWritten += writeResult;
            }
        }
    }
    if(communicationData.content.length > 0) {
        NSInteger dataBytesWritten = 0;
        NSData *contentData = [communicationData.content getData];
        while (contentData.length > dataBytesWritten )
        {
            while (!self.outputStream.hasSpaceAvailable) {
                [NSThread sleepForTimeInterval:0.05];
            }
            
            NSInteger writeResult = [self.outputStream write:[contentData bytes] + dataBytesWritten maxLength:communicationData.content.length - dataBytesWritten];
            if (writeResult != -1 ) {
                dataBytesWritten += writeResult;
            }
        }
    }
    if(communicationData.footer.length > 0) {
        NSInteger footerBytesWritten = 0;
        NSData *footerData = [communicationData.footer getData];
        while (footerData.length > footerBytesWritten )
        {
            while (!self.outputStream.hasSpaceAvailable) {
                [NSThread sleepForTimeInterval:0.05];
            }
            
            NSInteger writeResult = [self.outputStream write:[footerData bytes] + footerBytesWritten maxLength:communicationData.footer.length - footerBytesWritten];
            if (writeResult != -1 ) {
                footerBytesWritten += writeResult;
            }
        }
    }
    
    if([self.delegate respondsToSelector:@selector(connection:didSendData:)]) {
        [self.delegate connection:self didSendData:communicationData];
    }
}

- (void)disconnect:(BOOL)sendTerminationMessage {
    if(sendTerminationMessage) {
        [self sendCommunicationData:[CommunicationData terminationData]];
    }
    else {
        self.connectionState = ConnectionStateDisconnected;
        [self.inputStream close];
        [self.inputStream removeFromRunLoop:self.runLoop forMode: NSDefaultRunLoopMode];
        self.inputStream.delegate = nil;
        self.inputStream = nil;
        
        [self.outputStream close];
        [self.inputStream removeFromRunLoop:self.runLoop forMode: NSDefaultRunLoopMode];
        self.outputStream.delegate = nil;
        self.outputStream = nil;
        
        self.runLoop = nil;
        
        if([self.delegate respondsToSelector:@selector(connectionDidDisconnect:)]) {
            [self.delegate connectionDidDisconnect:self];
        }
    }
}

@end
