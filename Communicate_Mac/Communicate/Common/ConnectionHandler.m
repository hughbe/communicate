//
//  DataHandler.m
//  Communicate
//
//  Created by Hugh Bellamy on 25/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "ConnectionHandler.h"
#import "CommunicationData.h"
#import "DataInfo.h"
#import "DataHeader.h"
#import "DataContent.h"
#import "DataFooter.h"

@interface ConnectionHandler ()

@property (strong, readwrite, nonatomic) NSInputStream *inputStream;
@property (strong, readwrite, nonatomic) NSOutputStream *outputStream;

- (void)setupWithReadStream:(CFReadStreamRef)readStream writeStream:(CFWriteStreamRef)writeStream;

@end

@implementation ConnectionHandler

- (instancetype)initWithDelegate:(id<ConnectionHandlerDelegate>)delegate {
    self = [super init];
    if(self) {
        self.delegate = delegate;
    }
    return self;
}

- (void)connectToSocket:(CFSocketRef)socket {
    
    //[self setupWithReadStream:readStream writeStream:writeStream];
}

- (void)connectToAddress:(NSString *)address port:(int)port {
    CFReadStreamRef readStream;
    CFWriteStreamRef writeStream;
    CFStreamCreatePairWithSocketToHost(NULL, (__bridge CFStringRef)(address), port, &readStream, &writeStream);
    [self setupWithReadStream:readStream writeStream:writeStream];
}

- (void)setupWithReadStream:(CFReadStreamRef)readStream writeStream:(CFWriteStreamRef)writeStream {
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

- (void)startReceiving {
    if([self.delegate respondsToSelector:@selector(connectionHandlerDidConnect:)]) {
        [self.delegate connectionHandlerDidConnect:self];
    }
}

- (void)disconnect {
    [self.inputStream close];
    [self.inputStream removeFromRunLoop:[NSRunLoop currentRunLoop] forMode: NSDefaultRunLoopMode];
    self.inputStream.delegate = nil;
    self.inputStream = nil;
    
    [self.outputStream close];
    self.outputStream.delegate = nil;
    self.outputStream = nil;
}

- (void)stream:(NSStream *)aStream handleEvent:(NSStreamEvent)eventCode {
    if(eventCode == NSStreamEventEndEncountered || eventCode == NSStreamEventErrorOccurred) {
        if([self.delegate respondsToSelector:@selector(connectionHandlerDidDisconnect:)]) {
            [self.delegate connectionHandlerDidDisconnect:self];
        }
    }
    else if(eventCode == NSStreamEventHasBytesAvailable) {
        NSUInteger infoLength = [DataInfo length];
        
        uint8_t infoBuffer[infoLength];
        [self.inputStream read:infoBuffer maxLength:infoLength];
        NSData *infoData = [[NSData alloc]initWithBytes:(const void *)infoBuffer length:infoLength];
        DataInfo *dataInfo = [[DataInfo alloc]initWithData:infoData];
        
        NSData *headerData = [NSData data];
        if(dataInfo.headerLength > 0) {
            uint8_t headerBuffer[dataInfo.headerLength];
            [self.inputStream read:headerBuffer maxLength:dataInfo.headerLength];
            headerData = [[NSData alloc]initWithBytes:(const void *)headerBuffer length:dataInfo.headerLength];
        }
        DataHeader *header = [[DataHeader alloc]initWithData:headerData];
        
        NSData *contentData = [NSData data];
        uint8_t contentBuffer[dataInfo.contentLength];
        if(dataInfo.contentLength > 0) {
            [self.inputStream read:contentBuffer maxLength:dataInfo.contentLength];
            contentData = [[NSData alloc]initWithBytes:(const void *)contentBuffer length:dataInfo.contentLength];
        }
        
        DataContent *content = [[DataContent alloc]initWithData:contentData];
        
        NSData *footerData = [NSData data];
        if(dataInfo.footerLength > 0) {
            uint8_t footerBuffer[dataInfo.footerLength];
            [self.inputStream read:footerBuffer maxLength:dataInfo.footerLength];
            footerData = [[NSData alloc]initWithBytes:(const void *)footerBuffer length:dataInfo.footerLength];
        }
        
        DataFooter *footer = [[DataFooter alloc]initWithData:footerData];
        
        CommunicationData *receivedData = [[CommunicationData alloc]initWithInfo:dataInfo header:header content:content footer:footer];
        dispatch_async(dispatch_get_main_queue(),^ {
            if([self.delegate respondsToSelector:@selector(connectionHandler:didReceiveData:)]) {
                [self.delegate connectionHandler:self didReceiveData:receivedData];
            }
        });
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
        [self sendCommunicationData:[CommunicationData fromJSONData:data JSONObjectType:JSONObjectTypeOther]];
    }
}

- (void)sendData:(NSData *)data {
    [self sendCommunicationData:[CommunicationData fromData:data]];
}

- (void)sendCommunicationData:(CommunicationData *)communicationData {
    if(!communicationData) {
        return;
    }
    
    //if(self.outputStream.hasSpaceAvailable) {
    [self.outputStream write:[[communicationData.dataInfo getData] bytes] maxLength:[DataInfo length]];
    if(communicationData.header.length > 0) {
        [self.outputStream write:[[communicationData.header getData] bytes] maxLength:communicationData.header.length];
    }
    if(communicationData.content.length > 0) {
        NSInteger bytesWritten = 0;
        NSData *completeData = [communicationData.content getData];
        while (completeData.length > bytesWritten )
        {
            while (!self.outputStream.hasSpaceAvailable) {
                [NSThread sleepForTimeInterval:0.05];
            }
            
            NSInteger writeResult = [self.outputStream write:[completeData bytes]+bytesWritten maxLength:communicationData.content.length-bytesWritten];
            if (writeResult != -1 ) {
                bytesWritten += writeResult;
            }
        }
        
        ///int written = [self.outputStream write:[[communicationData.content getData] bytes] maxLength:communicationData.content.length];
        //NSLog(@"%d", written);
    }
    if(communicationData.footer.length > 0) {
        
        [self.outputStream write:[[communicationData.footer getData] bytes] maxLength:communicationData.footer.length];
    }
    if([self.delegate respondsToSelector:@selector(connectionHandler:didSendData:)]) {
        [self.delegate connectionHandler:self didSendData:communicationData];
    }
    //}
}

@end
