//
//  DataSerializer.h
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>

#if TARGET_OS_IPHONE
@import UIKit;
#elif TARGET_OS_MAC
@import AppKit;
#endif

#import "CommunicationDataType.h"

@class CommunicationData;

@interface StringSerializer : NSObject

+ (NSData *)dataFromString:(NSString *)string encoding:(NSStringEncoding)encoding;
+ (NSString *)stringFromData:(NSData *)data;

@end

@interface ImageSerializer : NSObject

#if TARGET_OS_IPHONE
+ (NSData *)dataFromImage:(UIImage *)image;
+ (UIImage *)imageFromData:(NSData *)data;
#elif TARGET_OS_MAC
+ (NSData *)dataFromImage:(NSImage*)image;
+ (NSImage*)imageFromData:(NSData *)data;
#endif

@end

@interface FileSerializer : NSObject

+ (NSData *)dataFromFilePath:(NSString *)filePath;

@end
