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
    if([self.delegate respondsToSelector:@selector(connectionDidConnect:)]) {
        [self.delegate connectionDidConnect:self];
    }
    
    self.inputStream = (__bridge NSInputStream *)readStream;
    self.outputStream = (__bridge NSOutputStream *)writeStream;
    
    self.inputStream.delegate = self;
    self.outputStream.delegate = self;
    
    [self.inputStream scheduleInRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
    [self.inputStream open];
    
    //[self.outputStream scheduleInRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
    [self.outputStream open];
    
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
        [self.netService resolveWithTimeout:30];
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
        @try {
        NSUInteger infoLength = [DataInfo length];
        
        uint8_t infoBuffer[infoLength];
        [self.inputStream read:infoBuffer maxLength:infoLength];
        NSData *infoData = [[NSData alloc]initWithBytes:(const void *)infoBuffer length:infoLength];
        DataInfo *dataInfo = [[DataInfo alloc]initWithData:infoData];
        
        NSData *headerData = [NSData data];
        if(dataInfo.headerLength > 0) {
            uint8_t headerBuffer[dataInfo.headerLength];
            
            NSInteger headerBytesRead = 0;
            while (headerBytesRead < dataInfo.headerLength) {
                NSInteger readResult = [self.inputStream read:headerBuffer + headerBytesRead maxLength:dataInfo.headerLength - headerBytesRead];
                if(readResult != -1) {
                    headerBytesRead += readResult;
                }
            }
            
            headerData = [[NSData alloc]initWithBytes:(const void *)headerBuffer length:dataInfo.headerLength];
        }
        DataHeader *header = [[DataHeader alloc]initWithData:headerData];
        
        NSData *contentData = [NSData data];
        if(dataInfo.contentLength > 0) {
            uint8_t contentBuffer[dataInfo.contentLength];
            
            NSInteger dataBytesRead = 0;
            while (dataBytesRead < dataInfo.contentLength) {
                NSInteger readResult = [self.inputStream read:contentBuffer + dataBytesRead maxLength:dataInfo.contentLength - dataBytesRead];
                if(readResult != -1) {
                    dataBytesRead += readResult;
                }
            }
            
            contentData = [[NSData alloc]initWithBytes:(const void *)contentBuffer length:dataInfo.contentLength];
        }
        
        DataContent *content = [[DataContent alloc]initWithData:contentData];
        
        NSData *footerData = [NSData data];
        if(dataInfo.footerLength > 0) {
            uint8_t footerBuffer[dataInfo.footerLength];
            
            NSInteger footerBytesRead = 0;
            while (footerBytesRead < dataInfo.footerLength) {
                NSInteger readResult = [self.inputStream read:footerBuffer + footerBytesRead maxLength:dataInfo.contentLength - footerBytesRead];
                if(readResult != -1) {
                    footerBytesRead += readResult;
                }
            }
            
            footerData = [[NSData alloc]initWithBytes:(const void *)footerBuffer length:dataInfo.footerLength];
        }
        
        DataFooter *footer = [[DataFooter alloc]initWithData:footerData];
        
        CommunicationData *receivedData = [[CommunicationData alloc]initWithInfo:dataInfo header:header content:content footer:footer];
        dispatch_async(dispatch_get_main_queue(),^ {
            if(dataInfo.dataType == CommunicationDataTypeTermination) {
                [self disconnect:NO];
            }
            else if([self.delegate respondsToSelector:@selector(connection:didReceiveData:)]) {
                [self.delegate connection:self didReceiveData:receivedData];
            }
        });
        }
        @catch (NSException *exception) {
            NSLog(@"%@", exception);
        }
    }
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
        [self.inputStream removeFromRunLoop:[NSRunLoop currentRunLoop] forMode: NSDefaultRunLoopMode];
        self.inputStream.delegate = nil;
        self.inputStream = nil;
    
        [self.outputStream close];
        self.outputStream.delegate = nil;
        self.outputStream = nil;
    }
}

@end
