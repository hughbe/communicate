//
//  ReceivedData.h
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

#import "CommunicationDataType.h"

@class DataHeader;
@class DataFooter;
@class DataContent;
@class DataInfo;

@interface CommunicationData : NSObject

@property (strong, readonly, nonatomic) DataInfo *dataInfo;
@property (strong, readonly, nonatomic) DataHeader *header;
@property (strong, readonly, nonatomic) DataContent *content;
@property (strong, readonly, nonatomic) DataFooter *footer;

+ (CommunicationData *)fromString:(NSString *)string withEncoding:(NSStringEncoding)encoding;
#if TARGET_OS_IPHONE
+ (CommunicationData *)fromImage:(UIImage *)image name:(NSString *)name;
#elif TARGET_OS_MAC
+ (CommunicationData *)fromImage:(NSImage *)image name:(NSString *)name;
#endif

+ (CommunicationData *)fromFile:(NSString *)filePath name:(NSString *)name;

+ (CommunicationData *)fromDictionary:(NSDictionary *)dictionary;
+ (CommunicationData *)fromArray:(NSArray *)array;
+ (CommunicationData *)fromJSONData:(NSData *)data JSONObjectType:(JSONObjectType)objectType;

+ (CommunicationData *)fromData:(NSData *)data;

+ (CommunicationData *)terminationData;

- (instancetype)initWithInfo:(DataInfo *)dataInfo header:(DataHeader *)header content:(DataContent *)content footer:(DataFooter *)footer;

@property (assign, readonly, nonatomic) CommunicationDataType dataType;

@property (copy, readonly, nonatomic) NSString *stringValue;

#if TARGET_OS_IPHONE
@property (strong, readonly, nonatomic) UIImage *imageValue;
#elif TARGET_OS_MAC
@property (strong, readonly, nonatomic) NSImage *imageValue;
#endif

@property (strong, readonly, nonatomic) NSData *dataValue;

@end
