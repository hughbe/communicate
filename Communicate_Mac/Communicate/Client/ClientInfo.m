//
//  ClientInfo.m
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "ClientInfo.h"

@interface ClientInfo ()

@property (assign, readwrite, nonatomic) int port;

@end

@implementation ClientInfo

- (instancetype)initWithPort:(int)port {
    self = [super init];
    if(self) {
        self.port = port;
    }
    return self;
}

- (NSString *)description {
    return [NSString stringWithFormat:@"ClientInfo: port = %ld", (long)self.port];
}

@end
