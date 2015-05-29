//
//  DataHeader.m
//  Communicate
//
//  Created by Hugh Bellamy on 25/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "DataHeader.h"

@interface DataHeader ()

+ (NSString *)fileNameKey;
+ (NSString *)JSONObjectTypeKey;

@end

@implementation DataHeader

- (NSString *)fileName {
    return [self valueForKey:[DataHeader fileNameKey]];
}

- (void)setFileName:(NSString *)fileName {
    [self setValue:fileName forKey:[DataHeader fileNameKey]];
}

- (JSONObjectType)JSONObjectType {
    return [self integerForKey:[DataHeader JSONObjectTypeKey]];
}

- (void)setJSONObjectType:(JSONObjectType)JSONObjectType {
    [self setInteger:JSONObjectType forKey:[DataHeader JSONObjectTypeKey]];
}

+ (NSString *)fileNameKey {
    return @"FileName";
}

+ (NSString *)JSONObjectTypeKey {
    return @"JSONObjectType";
}


@end
