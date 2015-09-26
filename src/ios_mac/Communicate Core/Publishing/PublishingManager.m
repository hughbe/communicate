//
//  PublishingManager.m
//  Communicate
//
//  Created by Hugh Bellamy on 27/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "PublishingManager.h"

#import "ProtocolInfo.h"
#import "CommunicatorInfo.h"
#import "TXTRecordList.h"

@interface PublishingManager () <NSNetServiceDelegate>

@property (assign, readwrite, nonatomic) PublishingState publishingState;

@property (strong, readwrite, nonatomic) ProtocolInfo *protocol;
@property (strong, readwrite, nonatomic) CommunicatorInfo *communicatorInfo;

@property (strong, readwrite, nonatomic) NSNetService *publishedService;

@end

@implementation PublishingManager

- (instancetype)initWithProtocol:(ProtocolInfo *)protocol communicatorInfo:(CommunicatorInfo *)communicatorInfo delegate:(id<PublishingManagerDelegate>)delegate {
    self = [super init];
    if(self) {
        self.publishingState = PublishingStateNotPublished;
        
        self.protocol = protocol;
        self.communicatorInfo = communicatorInfo;
        self.delegate = delegate;
    }
    return self;
}

- (void)publish {
    if(self.publishingState == PublishingStatePublishing) {
        if([self.delegate respondsToSelector:@selector(publishingManagerDidNotPublish:reason:errorDictionary:)]) {
            [self.delegate publishingManagerDidNotPublish:self reason:CommunicatorErrorCodePublishing errorDictionary:nil];
        }
        return;
    }
    else if(self.publishingState == PublishingStatePublished) {
        if([self.delegate respondsToSelector:@selector(publishingManagerDidNotPublish:reason:errorDictionary:)]) {
            [self.delegate publishingManagerDidNotPublish:self reason:CommunicatorErrorCodeAlreadyPublished errorDictionary:nil];
        }
        return;
    }
    
    self.publishingState = PublishingStatePublishing;
    if([self.delegate respondsToSelector:@selector(publishingManagerDidStartPublishing:)]) {
        [self.delegate publishingManagerDidStartPublishing:self];
    }
    
    [self.publishedService publish];
}

- (void)netServiceDidPublish:(NSNetService *)sender {
    self.publishingState = PublishingStatePublished;
    if([self.delegate respondsToSelector:@selector(publishingManagerDidPublish:)]) {
        [self.delegate publishingManagerDidPublish:self];
    }
}

- (void)netService:(NSNetService *)sender didNotPublish:(NSDictionary *)errorDictionary {
    self.publishingState = PublishingStateErrorPublishing;
    if([self.delegate respondsToSelector:@selector(publishingManagerDidNotPublish:reason:errorDictionary:)]) {
        [self.delegate publishingManagerDidNotPublish:self reason:CommunicatorErrorCodeFailedToPublish errorDictionary:errorDictionary];
    }
}

- (void)netServiceDidStop:(NSNetService *)sender {
    self.publishingState = PublishingStateUnpublished;
    if([self.delegate respondsToSelector:@selector(publishingManagerDidUnpublish:)]) {
        [self.delegate publishingManagerDidUnpublish:self];
    }
}

- (void)unpublish {
    if(self.publishingState == PublishingStatePublishing || self.publishingState == PublishingStatePublished) {
        self.publishingState = PublishingStateUnpublished;
        [self.publishedService stop];
    }
}

- (ProtocolInfo *)protocol {
    if(!_protocol) {
        _protocol = [[ProtocolInfo alloc]init];
    }
    return _protocol;
}

- (CommunicatorInfo *)communicatorInfo {
    if(!_communicatorInfo) {
        _communicatorInfo = [[CommunicatorInfo alloc]init];
    }
    return _communicatorInfo;
}

- (NSNetService *)publishedService {
    if(!_publishedService) {
        _publishedService = [[NSNetService alloc]initWithDomain:self.protocol.domain type:[self.protocol serializeTypeWithDomain:NO] name:self.communicatorInfo.readableName port:self.communicatorInfo.port];
        _publishedService.TXTRecordData = [self.communicatorInfo.TXTRecordList serialize];
        _publishedService.delegate = self;
    }
    return _publishedService;
}

@end
