//
//  DataSerializer.m
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "DataSerializers.h"
#import "CommunicationData.h"
#import "ByteHeader.h"

@implementation StringSerializer

+ (NSData *)dataFromString:(NSString *)string encoding:(NSStringEncoding)encoding {
    NSData *data = [string dataUsingEncoding:encoding];
    return data;
}

+ (NSString *)stringFromData:(NSData *)data {
    NSString *string = [[NSString alloc]initWithData:data encoding:NSUTF8StringEncoding];
    return string;
}

@end

@implementation ImageSerializer

#if TARGET_OS_IPHONE

+ (NSData *)dataFromImage:(UIImage *)image {
    NSData *data = UIImagePNGRepresentation(image);
    return data;
}

+ (UIImage *)imageFromData:(NSData *)data {
    UIImage *image = [[UIImage alloc]initWithData:data];
    return image;
}

#elif TARGET_OS_MAC

+ (NSData *)dataFromImage:(NSImage*)image {
    CGImageRef cgRef = [image CGImageForProposedRect:NULL context:nil hints:nil];
    NSBitmapImageRep *newRep = [[NSBitmapImageRep alloc] initWithCGImage:cgRef];
    newRep.size = image.size;
    NSData *data = [newRep representationUsingType:NSPNGFileType properties:nil];
    return data;
}

+ (NSImage*)imageFromData:(NSData *)data {
    NSImage *image = [[NSImage alloc]initWithData:data];
    return image;
}

#endif

@end

@implementation FileSerializer

+ (NSData *)dataFromFilePath:(NSString *)filePath {
    NSData *data = [[NSData alloc]initWithContentsOfFile:filePath];
    return data;
}

@end
