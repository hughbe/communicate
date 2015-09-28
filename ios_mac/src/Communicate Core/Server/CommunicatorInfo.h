//
//  ServerInfo.h
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>

@class TXTRecordList;

@interface CommunicatorInfo : NSObject

@property (copy, readonly, nonatomic) NSString *readableName;
@property (assign, readonly, nonatomic) int port;

@property (strong, readonly, nonatomic) TXTRecordList *TXTRecordList;

- (instancetype)initWithName:(NSString *)readableName port:(int)port txtRecordList:(TXTRecordList *)txtRecordList;

@end
