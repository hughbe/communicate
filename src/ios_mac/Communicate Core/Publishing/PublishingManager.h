//
//  PublishingManager.h
//  Communicate
//
//  Created by Hugh Bellamy on 27/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "Communicator.h"
@class ProtocolInfo, CommunicatorInfo;

typedef NS_ENUM(NSInteger, PublishingState) {
    PublishingStateNotPublished,
    PublishingStatePublishing,
    PublishingStatePublished,
    PublishingStateErrorPublishing,
    PublishingStateUnpublished
};

@protocol PublishingManagerDelegate;

@interface PublishingManager : NSObject

@property (weak, nonatomic) id<PublishingManagerDelegate> delegate;

@property (assign, readonly, nonatomic) PublishingState publishingState;

@property (strong, readonly, nonatomic) ProtocolInfo *protocol;
@property (strong, readonly, nonatomic) CommunicatorInfo *communicatorInfo;

@property (strong, readonly, nonatomic) NSNetService *publishedService;

- (instancetype)initWithProtocol:(ProtocolInfo *)protocol communicatorInfo:(CommunicatorInfo *)communicatorInfo delegate:(id<PublishingManagerDelegate>)delegate;

- (void)publish;
- (void)unpublish;

@end

@protocol PublishingManagerDelegate <NSObject>
@optional
- (void)publishingManagerDidStartPublishing:(PublishingManager *)publishingManager;
- (void)publishingManagerDidPublish:(PublishingManager *)publishingManager;
- (void)publishingManagerDidNotPublish:(PublishingManager *)publishingManager reason:(CommunicatorErrorCode)errorCode errorDictionary:(NSDictionary *)errorDictionary;
- (void)publishingManagerDidUnpublish:(PublishingManager *)publishingManager;
@end
