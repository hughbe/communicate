
//
//  DataContent.m
//  Communicate
//
//  Created by Hugh Bellamy on 25/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "DataContent.h"
#import "DataSerializers.h"

@interface DataContent ()

@property (strong, nonatomic) NSData *innerData;

@end

@implementation DataContent

- (instancetype)initWithString:(NSString *)string withEncoding:(NSStringEncoding)encoding {
    self = [super init];
    if(self) {
        self.innerData = [StringSerializer dataFromString:string encoding:encoding];
    }
    return self;
}

#if TARGET_OS_IPHONE

- (instancetype)initWithImage:(UIImage *)image {
    self = [super init];
    if(self) {
        self.innerData = [ImageSerializer dataFromImage:image];
    }
    return self;
    
}

#elif TARGET_OS_MAC

- (instancetype)initWithImage:(NSImage *)image {
    self = [super init];
    if(self) {
        self.innerData = [ImageSerializer dataFromImage:image];
    }
    return self;
}

#endif

- (instancetype)initWithFilePath:(NSString *)filePath {
    self = [super init];
    if(self) {
        self.innerData = [FileSerializer dataFromFilePath:filePath];
    }
    return self;
}

- (instancetype)initWithData:(NSData *)data {
    self = [super init];
    if(self) {
        self.innerData = data;
    }
    return self;
}

- (NSData *)getData {
    return self.innerData;
}

- (NSUInteger)length {
    return [self getData].length;
}

- (NSString *)description {
    return [NSString stringWithFormat:@"Data Content: length = %lu", (unsigned long)self.innerData.length];
}

- (NSData *)innerData {
    if(!_innerData) {
        _innerData = [[NSData alloc]init];
    }
    return _innerData;
}

@end
