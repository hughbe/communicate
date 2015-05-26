//
//  TXTRecordList.h
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>
@class TXTRecord;

@interface TXTRecordList : NSObject

- (void)addTXTRecord:(TXTRecord*)TXTRecord;
- (void)addTXTRecordWithKey:(NSString *)key value:(NSString *)value;

- (NSData *)serialize;

@end
