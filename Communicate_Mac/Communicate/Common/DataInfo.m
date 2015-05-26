//
//  DataHeader.m
//  Communicate
//
//  Created by Hugh Bellamy on 25/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "DataInfo.h"

#import "DataContent.h"
#import "DataHeaderFooter.h"
#import "DataHeader.h"
#import "DataFooter.h"
#import "ByteHeader.h"

@interface DataInfo ()

@property (assign, readwrite, nonatomic) CommunicationDataType dataType;
@property (assign, readwrite, nonatomic) NSUInteger headerLength;
@property (assign, readwrite, nonatomic) NSUInteger contentLength;
@property (assign, readwrite, nonatomic) NSUInteger footerLength;

+ (NSArray *)byteHeaders;

@end

@implementation DataInfo

- (instancetype)initWithDataType:(CommunicationDataType)dataType header:(DataHeader *)header content:(DataContent *)content footer:(DataFooter *)footer {
    self = [super init];
    if(self) {
        self.dataType = dataType;
        self.headerLength = [header getData].length;
        self.contentLength = [content getData].length;
        self.footerLength = [footer getData].length;
    }
    return self;
}

- (instancetype)initWithData:(NSData *)data {
    self = [super init];
    if(self) {
        self.dataType = [DataInfo dataTypeFromHeaderData:data];
        self.contentLength = [DataInfo contentLengthFromHeaderData:data];
        self.footerLength = [DataInfo footerLengthFromHeaderData:data];
    }
    return self;
}

- (NSData *)getData {
    NSMutableData *header = [[NSMutableData alloc]init];
    
    NSUInteger headerLength = self.headerLength;
    NSUInteger contentLength = self.contentLength;
    NSUInteger footerLength = self.footerLength;
    
    NSData *typeData = [DataInfo typeDataFromDataType:self.dataType];
    NSData *headerLengthData = [NSData dataWithBytes: &headerLength length:4];
    NSData *contentLengthData = [NSData dataWithBytes: &contentLength length:4];
    NSData *footerLengthData = [NSData dataWithBytes: &footerLength length:4];
    
    [header appendData:typeData];
    [header appendData:headerLengthData];
    [header appendData:contentLengthData];
    [header appendData:footerLengthData];
    
    return [NSData dataWithData:header];
}

+ (NSUInteger)length {
    return 14;
}

+ (NSUInteger)headerLengthFromHeaderData:(NSData *)headerData {
    int bytes;
    [headerData getBytes:&bytes range:NSMakeRange(2, 4)];
    return bytes;
}

+ (NSUInteger)contentLengthFromHeaderData:(NSData *)headerData {
    int bytes;
    [headerData getBytes:&bytes range:NSMakeRange(6, 4)];
    return bytes;
}

+ (NSUInteger)footerLengthFromHeaderData:(NSData *)headerData {
    int bytes;
    [headerData getBytes:&bytes range:NSMakeRange(10, 4)];
    return bytes;
}

+ (CommunicationDataType)dataTypeFromHeaderData:(NSData *)headerData {
    char bytes[2];
    [headerData getBytes:&bytes length:sizeof(bytes)];
    
    ByteHeader *byteHeader = [[ByteHeader alloc]initWithFirstByte:bytes[0] secondByte:bytes[1] dataType:CommunicationDataTypeOther];
    ByteHeader *actualByteHeader = [ByteHeader otherByteHeader];
    
    for (ByteHeader *aByteHeader in [DataInfo byteHeaders]) {
        if([aByteHeader isEqual:byteHeader]) {
            actualByteHeader = aByteHeader;
            break;
        }
    }
    return actualByteHeader.dataType;
}


+ (NSData *)typeDataFromDataType:(CommunicationDataType)dataType {
    ByteHeader *byteHeader = [ByteHeader otherByteHeader];
    for(ByteHeader *dataTypeByteHeader in [DataInfo byteHeaders]) {
        if(dataTypeByteHeader.dataType == dataType) {
            byteHeader = dataTypeByteHeader;
            break;
        }
    }
    return [byteHeader toData];
}

+ (NSArray *)byteHeaders {
    static NSArray *byteHeaders;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        byteHeaders = @[[ByteHeader stringByteHeader], [ByteHeader imageByteHeader], [ByteHeader fileByteHeader], [ByteHeader JSONByteHeader], [ByteHeader otherByteHeader]];
    });
    return byteHeaders;
}

- (NSString *)description {
    return [NSString stringWithFormat:@"Data Info: data type = %ld; header length = %lu; content length = %lu; footer length = %lu", (long)self.dataType, (unsigned long)self.headerLength, (unsigned long)self.contentLength, (unsigned long)self.footerLength];
}

@end
