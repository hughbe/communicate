//
//  ProtocolInfo.h
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>

typedef NS_ENUM(NSInteger, TransportProtocolType) {
    TransportProtocolTypeTCP
};

extern NSString* const ProtocolLocalDomainString;

@interface ProtocolInfo : NSObject

@property (copy, readonly, nonatomic) NSString *name;
@property (assign, readonly, nonatomic) TransportProtocolType protocolType;
@property (copy, readonly, nonatomic) NSString *domain;

- (instancetype)initWithProtocolName:(NSString *)protocolName protocolType:(TransportProtocolType)protocolType domain:(NSString *)domain;

- (NSString *)serializeTypeWithDomain:(BOOL)includeDomain;

@end
