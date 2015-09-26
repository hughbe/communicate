//
//  SearchingManager.h
//  Communicate
//
//  Created by Hugh Bellamy on 27/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "Communicator.h"
@class ProtocolInfo;
@protocol SearchingManagerDelegate;

typedef NS_ENUM(NSInteger, SearchingState) {
    SearchingStateNotSearching,
    SearchingStateSearching,
    SearchingStateErrorSearching,
    SearchingStateStoppedSearching
};

@interface SearchingManager : NSObject

@property (weak, nonatomic) id<SearchingManagerDelegate> delegate;

@property (assign, readonly, nonatomic) SearchingState searchingState;

@property (strong, readonly, nonatomic) ProtocolInfo *protocol;

@property (strong, readonly, nonatomic) NSArray *services;
@property (strong, readonly, nonatomic) NSNetServiceBrowser *browser;

- (instancetype)initWithProtocol:(ProtocolInfo *)protocol delegate:(id<SearchingManagerDelegate>)delegate;

- (void)startSearching;
- (void)stopSearching;
- (void)restartSearching;

@end

@protocol SearchingManagerDelegate <NSObject>
@optional
- (void)searchingManagerDidStartSearching:(SearchingManager *)searchingManager;
- (void)searchingManager:(SearchingManager *)searchingManager didFindServices:(NSArray *)services;
- (void)searchingManagerDidNotStartSearching:(SearchingManager *)searchingManager reason:(CommunicatorErrorCode)errorCode errorDictionary:(NSDictionary *)errorDictionary;
- (void)searchingManagerDidStopSearching:(SearchingManager *)searchingManager;

@end