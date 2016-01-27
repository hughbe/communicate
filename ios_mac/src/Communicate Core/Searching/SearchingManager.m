

//
//  SearchingManager.m
//  Communicate
//
//  Created by Hugh Bellamy on 27/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "SearchingManager.h"
#import "ProtocolInfo.h"

@interface SearchingManager () <NSNetServiceBrowserDelegate>

@property (assign, readwrite, nonatomic) SearchingState searchingState;

@property (strong, readwrite, nonatomic) ProtocolInfo *protocol;

@property (strong, readwrite, nonatomic) NSArray *services;
@property (strong, readwrite, nonatomic) NSNetServiceBrowser *browser;

@end

@implementation SearchingManager

- (instancetype)initWithProtocol:(ProtocolInfo *)protocol delegate:(id<SearchingManagerDelegate>)delegate {
    self = [super init];
    if(self) {
        self.searchingState = SearchingStateNotSearching;
        
        self.protocol = protocol;
        self.delegate = delegate;
    }
    return self;
}

- (void)startSearching {
    if(self.searchingState == SearchingStateSearching) {
        if([self.delegate respondsToSelector:@selector(searchingManagerDidNotStartSearching:reason:errorDictionary:)]) {
            [self.delegate searchingManagerDidNotStartSearching:self reason:CommunicatorErrorCodeAlreadySearching errorDictionary:nil];
        }
        return;
    }
    self.searchingState = SearchingStateSearching;
    if([self.delegate respondsToSelector:@selector(searchingManagerDidStartSearching:)]) {
        [self.delegate searchingManagerDidStartSearching:self];
    }
    [self.browser searchForServicesOfType:[self.protocol serializeTypeWithDomain:NO] inDomain:self.protocol.domain];
}

- (void)restartSearching {
    [self.browser stop];
    self.services = [NSArray array];
    self.searchingState = SearchingStateNotSearching;
    [self startSearching];
}

- (void)netServiceBrowser:(NSNetServiceBrowser *)aNetServiceBrowser didFindService:(NSNetService *)aNetService moreComing:(BOOL)moreComing {
    if(![self.services containsObject:aNetService]) {
        BOOL addService = YES;
        for (NSNetService *netService in [self.services copy]) {
            if([netService.name isEqualToString:aNetService.name]) {
                addService = NO;
                break;
            }
        }
        if(addService && aNetService) {
            self.services = [self.services arrayByAddingObject:aNetService];
        }
    }
    if(!moreComing) {
        if([self.delegate respondsToSelector:@selector(searchingManager:didFindServices:)]) {
            [self.delegate searchingManager:self didFindServices:[self.services copy]];
        }
    }
}

- (void)netServiceBrowser:(NSNetServiceBrowser *)aNetServiceBrowser didRemoveService:(NSNetService *)aNetService moreComing:(BOOL)moreComing {
    if([self.services containsObject:aNetService]) {
        NSMutableArray *mutableServices = [self.services mutableCopy];
        [mutableServices removeObject:aNetService];
        self.services = [NSArray arrayWithArray:mutableServices];
    }
    if(!moreComing) {
        if([self.delegate respondsToSelector:@selector(searchingManager:didFindServices:)]) {
            [self.delegate searchingManager:self didFindServices:[self.services copy]];
        }
    }
}

- (void)netServiceBrowser:(NSNetServiceBrowser *)aNetServiceBrowser didNotSearch:(NSDictionary *)errorDict {
    self.searchingState = SearchingStateNotSearching;
    
    if([self.delegate respondsToSelector:@selector(searchingManagerDidNotStartSearching:reason:errorDictionary:)]) {
        [self.delegate searchingManagerDidNotStartSearching:self reason:CommunicatorErrorCodeFailedToSearch errorDictionary:errorDict];
    }
}

- (void)netServiceBrowserDidStopSearch:(NSNetServiceBrowser *)aNetServiceBrowser {
    self.searchingState = SearchingStateNotSearching;
    if([self.delegate respondsToSelector:@selector(searchingManagerDidStopSearching:)]) {
        [self.delegate searchingManagerDidStopSearching:self];
    }
}

- (void)stopSearching {
    if(self.searchingState == SearchingStateSearching) {
        self.searchingState = SearchingStateNotSearching;
        [self.browser stop];
    }
}

- (NSArray *)services {
    if(!_services) {
        _services = [NSArray array];
    }
    return _services;
}

- (NSNetServiceBrowser *)browser {
    if(!_browser) {
        _browser = [[NSNetServiceBrowser alloc]init];
        _browser.delegate = self;
    }
    return _browser;
}

@end
