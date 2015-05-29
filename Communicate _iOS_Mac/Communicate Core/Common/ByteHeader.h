//
//  ByteHeader.h
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "CommunicationDataType.h"

@interface ByteHeader : NSObject

@property (assign, readonly, nonatomic) char byte1;
@property (assign, readonly, nonatomic) char byte2;

@property (assign, readonly, nonatomic) CommunicationDataType dataType;

+ (ByteHeader *)stringByteHeader;
+ (ByteHeader *)imageByteHeader;
+ (ByteHeader *)fileByteHeader;
+ (ByteHeader *)JSONByteHeader;
+ (ByteHeader *)otherByteHeader;
+ (ByteHeader *)terminationByteHeader;

- (instancetype)initWithFirstByte:(char)byte1 secondByte:(char)byte2 dataType:(CommunicationDataType)dataType;

- (NSData *)toData;

@end
