//
//  ByteHeader.m
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "ByteHeader.h"

@interface ByteHeader ()

@property (assign, readwrite, nonatomic) char byte1;
@property (assign, readwrite, nonatomic) char byte2;

@property (assign, readwrite, nonatomic) CommunicationDataType dataType;

@end

@implementation ByteHeader

- (instancetype)initWithFirstByte:(char)byte1 secondByte:(char)byte2 dataType:(CommunicationDataType)dataType{
    self = [super init];
    if(self) {
        self.byte1 = byte1;
        self.byte2 = byte2;
        self.dataType = dataType;
    }
    return self;
}
+ (ByteHeader *)stringByteHeader {
    return [[ByteHeader alloc] initWithFirstByte:0x01 secondByte:0x01 dataType:CommunicationDataTypeString];
}

+ (ByteHeader *)imageByteHeader {
    return [[ByteHeader alloc] initWithFirstByte:0x02 secondByte:0x02 dataType:CommunicationDataTypeImage];
}

+ (ByteHeader *)fileByteHeader {
    return [[ByteHeader alloc] initWithFirstByte:0x03 secondByte:0x03 dataType:CommunicationDataTypeFile];
}

+ (ByteHeader *)JSONByteHeader {
    return [[ByteHeader alloc] initWithFirstByte:0x04 secondByte:0x04 dataType:CommunicationDataTypeJSON];
}

+ (ByteHeader *)otherByteHeader {
    return [[ByteHeader alloc] initWithFirstByte:0x09 secondByte:0x09 dataType:CommunicationDataTypeOther];
}

+ (ByteHeader *)terminationByteHeader {
    return [[ByteHeader alloc] initWithFirstByte:0x00 secondByte:0x00 dataType:CommunicationDataTypeTermination];
}

- (BOOL)isEqual:(id)object {
    if(![object isKindOfClass:[ByteHeader class]]) {
        return NO;
    }
    ByteHeader *otherByterHeader = (ByteHeader *)object;
    if(otherByterHeader.byte1 == self.byte1 && otherByterHeader.byte2 == self.byte2) {
        return YES;
    }
    
    return NO;
}

- (NSData *)toData {
    NSMutableData *data = [NSMutableData data];
    char bytesToAppend[2] = { self.byte1, self.byte2 };
    [data appendBytes:bytesToAppend length:sizeof(bytesToAppend)];
    
    return [NSData dataWithData:data];
}

- (NSString *)description {
    return [NSString stringWithFormat:@"Byte Header: first byte = %c; second byte = %c; data type = %ld", self.byte1, self.byte2, (long)self.dataType];
}

@end
