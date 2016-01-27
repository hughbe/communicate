//
//  ProtocolInfo.m
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "ProtocolInfo.h"

NSString* const ProtocolLocalDomainString = @"local";

@interface ProtocolInfo ()

@property (copy, readwrite, nonatomic) NSString *name;
@property (assign, readwrite, nonatomic) TransportProtocolType protocolType;
@property (copy, readwrite, nonatomic) NSString *domain;

- (NSString *)protocolTypeString;

@end

@implementation ProtocolInfo

- (instancetype)initWithProtocolName:(NSString *)protocolName protocolType:(TransportProtocolType)protocolType domain:(NSString *)domain {
    self = [super init];
    if(self) {
        self.name = protocolName;
        self.protocolType = protocolType;
        self.domain = domain;
    }
    return self;
}

- (NSString *)description {
    return [NSString stringWithFormat:@"ProtocolInfo: %@", [self serializeTypeWithDomain:YES]];
}

- (NSString *)serializeTypeWithDomain:(BOOL)includeDomain {
    NSString *type = [NSString stringWithFormat:@"_%@._%@.", self.name, [self protocolTypeString]];
    if(includeDomain) {
        type = [type stringByAppendingFormat:@"%@.", self.domain];
    }
    return type;
}

- (NSString *)name {
    if(!_name) {
        _name = @"";
    }
    return _name;
}

- (NSString *)domain {
    if(!_domain || _domain.length == 0) {
        _domain = ProtocolLocalDomainString;
    }
    return _domain;
}

- (NSString *)protocolTypeString {
    NSString *toReturn = @"";
    if (self.protocolType == TransportProtocolTypeTCP) {
            toReturn = @"tcp";
    }
    return toReturn;
}

@end