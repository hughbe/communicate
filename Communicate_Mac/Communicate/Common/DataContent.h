//
//  DataContent.h
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

@interface DataContent : NSObject

@property (assign, readonly, nonatomic) NSUInteger length;

- (instancetype)initWithString:(NSString *)string withEncoding:(NSStringEncoding)encoding;

#if TARGET_OS_IPHONE
- (instancetype)initWithImage:(UIImage *)image;
#elif TARGET_OS_MAC
- (instancetype)initWithImage:(NSImage *)image;
#endif

- (instancetype)initWithFilePath:(NSString *)filePath;

- (instancetype)initWithData:(NSData *)data;

- (NSData *)getData;

@end
