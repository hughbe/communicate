//
//  ListeningManager.m
//  Communicate
//
//  Created by Hugh Bellamy on 27/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "ListeningManager.h"
#import "CommunicatorInfo.h"
#include <arpa/inet.h>

@interface ListeningManager ()

@property (assign, readwrite, nonatomic) ListeningState listeningState;

@property (strong, readwrite, nonatomic) CommunicatorInfo *communicatorInfo;

@property (assign, nonatomic) CFSocketRef ipv4Socket;

@end

@implementation ListeningManager

- (instancetype)initWithCommunicatorInfo:(CommunicatorInfo *)communicatorInfo delegate:(id<ListeningManagerDelegate>)delegate {
    self = [super init];
    if(self) {
        self.communicatorInfo = communicatorInfo;
        self.delegate = delegate;
        
        self.listeningState = ListeningStateNotListening;
    }
    return self;
}

- (void)startListening {
    if(self.listeningState == ListeningStateListening) {
        if([self.delegate respondsToSelector:@selector(listeningManagerDidNotStartListening:errorCode:)]) {
            [self.delegate listeningManagerDidNotStartListening:self errorCode:CommunicatorErrorCodeAlreadyListening];
        }
        return;
    }
    self.listeningState = ListeningStateListening;
    const CFSocketContext context = { 0, (__bridge void *)(self), NULL, NULL, NULL };
    
    self.ipv4Socket = CFSocketCreate(kCFAllocatorDefault, PF_INET, SOCK_STREAM, IPPROTO_TCP, kCFSocketAcceptCallBack, (CFSocketCallBack)&ServerAcceptCallBack, &context);
    
    if(self.ipv4Socket == NULL) {
        self.listeningState = ListeningStateErrorListening;
        if(self.ipv4Socket != NULL) {
            CFRelease(self.ipv4Socket);
        }
        self.ipv4Socket = NULL;
        
        if([self.delegate respondsToSelector:@selector(listeningManagerDidNotStartListening:errorCode:)])  {
            [self.delegate listeningManagerDidNotStartListening:self errorCode:CommunicatorErrorCodeNoSocketAvailable];
        }
        
        return;
    }
    int yes = 1;
    setsockopt(CFSocketGetNative(self.ipv4Socket), SOL_SOCKET, SO_REUSEADDR, (void *)&yes, sizeof(yes));
    
    struct sockaddr_in sin;
    
    memset(&sin, 0, sizeof(sin));
    sin.sin_len = sizeof(sin);
    sin.sin_family = AF_INET;
    sin.sin_port = htons(self.communicatorInfo.port);
    sin.sin_addr.s_addr= htonl(INADDR_ANY);
    
    CFDataRef sincfd = CFDataCreate(kCFAllocatorDefault, (UInt8 *)&sin, sizeof(sin));
    
    if(CFSocketSetAddress(self.ipv4Socket, sincfd) != kCFSocketSuccess) {
        self.listeningState = ListeningStateErrorListening;
        if(self.ipv4Socket != NULL) {
            CFRelease(self.ipv4Socket);
        }
        self.ipv4Socket = NULL;
        CFRelease(sincfd);
        
        if([self.delegate respondsToSelector:@selector(listeningManagerDidNotStartListening:errorCode:)])  {
            [self.delegate listeningManagerDidNotStartListening:self errorCode:CommunicatorErrorCodeCouldNotBindIPV4Address];
        }
        
        return;
    }
    
    CFRelease(sincfd);
    
    CFRunLoopSourceRef socketsource = CFSocketCreateRunLoopSource(kCFAllocatorDefault, self.ipv4Socket,0);
    CFRunLoopAddSource(CFRunLoopGetCurrent(), socketsource, kCFRunLoopDefaultMode);
    CFRelease(socketsource);
    
    if([self.delegate respondsToSelector:@selector(listeningManagerDidStartListening:)]) {
        [self.delegate listeningManagerDidStartListening:self];
    }
}

static void ServerAcceptCallBack(CFSocketRef socket, CFSocketCallBackType type, CFDataRef address, const void *data, void *info) {
    ListeningManager *listeningManager = (__bridge ListeningManager *)info;
    if (kCFSocketAcceptCallBack == type) {
        CFSocketNativeHandle nativeSocketHandle = *(CFSocketNativeHandle *)data;
        
        struct sockaddr_in peerAddress;
        socklen_t peerLen = sizeof(peerAddress);
        NSString * peer = nil;
        
        if (getpeername(nativeSocketHandle, (struct sockaddr *)&peerAddress, (socklen_t *)&peerLen) == 0) {
            peer = [NSString stringWithUTF8String:inet_ntoa(peerAddress.sin_addr)];
        } else {
            peer = @"Generic Peer";
        }
        
        
        NSLog(@"%@ has connected", peer);
        
        if([listeningManager.delegate respondsToSelector:@selector(listeningManager:didReceiveConnectionRequest:)]) {
            [listeningManager.delegate listeningManager:listeningManager didReceiveConnectionRequest:nativeSocketHandle];
        }
    }
}

- (void)stopListening {
    if(self.listeningState == ListeningStateListening) {
        self.listeningState = ListeningStateStoppedListening;
        
        CFSocketInvalidate(self.ipv4Socket);
        CFRelease(self.ipv4Socket);
        self.ipv4Socket = NULL;
        
        if([self.delegate respondsToSelector:@selector(listeningManagerDidStopListening:)]) {
            [self.delegate listeningManagerDidStopListening:self];
        }
    }
}

- (CommunicatorInfo *)communicatorInfo {
    if(!_communicatorInfo) {
        _communicatorInfo = [[CommunicatorInfo alloc]init];
    }
    return _communicatorInfo;
}

@end
