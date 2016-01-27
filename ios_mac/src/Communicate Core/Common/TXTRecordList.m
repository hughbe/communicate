//
//  TXTRecordList.m
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "TXTRecordList.h"
#import "TXTRecord.h"

@interface TXTRecordList ()

@property (strong, nonatomic) NSMutableDictionary *innerList;

@end

@implementation TXTRecordList

- (NSString *)description {
    return [NSString stringWithFormat:@"TXTRecordList: %@", self.innerList];
}

- (void)addTXTRecord:(TXTRecord *)TXTRecord {
    if(TXTRecord) {
        [self addTXTRecordWithKey:TXTRecord.key value:TXTRecord.value];
    }
}

- (void)addTXTRecordWithKey:(NSString *)key value:(NSString *)value {
    if(key && value) {
        self.innerList[key] = value;
    }
}

- (NSData *)serialize {
    return [NSNetService dataFromTXTRecordDictionary:self.innerList];
}

- (NSMutableDictionary *)innerList {
    if(!_innerList) {
        _innerList = [NSMutableDictionary dictionary];
    }
    return _innerList;
}

@end
