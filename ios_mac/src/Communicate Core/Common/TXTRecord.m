//
//  TXTRecord.m
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "TXTRecord.h"

@interface TXTRecord()

@property (copy, readwrite, nonatomic) NSString *key;
@property (copy, readwrite, nonatomic) NSString *value;

@end

@implementation TXTRecord

- (instancetype)initWithKey:(NSString *)key value:(NSString *)value {
    self = [super init];
    if(self) {
        self.key = key;
        self.value = value;
    }
    return self;
}

- (NSString *)description {
    return [NSString stringWithFormat:@"TXTRecord: key = %@; value = %@", self.key, self.value];
}

- (NSString *)key {
    if(!_key) {
        _key = @"";
    }
    return _key;
}

- (NSString *)value {
    if(!_value) {
        _value = @"";
    }
    return _value;
}

@end
