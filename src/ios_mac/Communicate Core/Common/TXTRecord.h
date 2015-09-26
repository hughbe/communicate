//
//  TXTRecord.h
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface TXTRecord : NSObject

@property (copy, readonly, nonatomic) NSString *key;
@property (copy, readonly, nonatomic) NSString *value;

- (instancetype)initWithKey:(NSString *)key value:(NSString *)value;

@end
