//
//  ClientInfo.h
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface ClientInfo : NSObject

@property (assign, readonly, nonatomic) int port;

- (instancetype)initWithPort:(int)port;

@end
