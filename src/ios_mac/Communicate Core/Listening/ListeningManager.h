//
//  ListeningManager.h
//  Communicate
//
//  Created by Hugh Bellamy on 27/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "Communicator.h"
@class CommunicatorInfo;

typedef NS_ENUM(NSInteger, ListeningState) {
    ListeningStateNotListening,
    ListeningStateListening,
    ListeningStateErrorListening,
    ListeningStateStoppedListening
};

@protocol ListeningManagerDelegate;

@interface ListeningManager : NSObject

@property (weak, nonatomic) id<ListeningManagerDelegate> delegate;

@property (assign, readonly, nonatomic) ListeningState listeningState;

@property (strong, readonly, nonatomic) CommunicatorInfo *communicatorInfo;

- (instancetype)initWithCommunicatorInfo:(CommunicatorInfo *)communicatorInfo delegate:(id<ListeningManagerDelegate>)delegate;

- (void)startListening;
- (void)stopListening;

@end

@protocol ListeningManagerDelegate <NSObject>
@optional
- (void)listeningManagerDidStartListening:(ListeningManager *)listeningManager;
- (void)listeningManager:(ListeningManager *)listeningManager didReceiveConnectionRequest:(CFSocketNativeHandle)connectionRequest;
- (void)listeningManagerDidNotStartListening:(ListeningManager *)listeningManager errorCode:(CommunicatorErrorCode)errorCode;
-(void)listeningManagerDidStopListening:(ListeningManager *)listeningManager;

@end
