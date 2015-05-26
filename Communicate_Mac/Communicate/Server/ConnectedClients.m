//
//  ConnectedClients.m
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "ConnectedClients.h"
#import "ConnectedClient.h"

@interface ConnectedClients ()

@property (strong, nonatomic) NSMutableArray *connectedClients;

@end

@implementation ConnectedClients

- (ConnectedClient *)objectAtIndexedSubscript:(NSUInteger)index {
    if(index < self.connectedClients.count) {
        return self.connectedClients[index];
    }
    return nil;
}

- (void)addClient:(ConnectedClient *)client {
    if(client) {
        [self.connectedClients addObject:client];
    }
}

- (void)disconnectClient:(ConnectedClient *)client {
    [client disconnect];
    if([self.connectedClients containsObject:client]) {
        [self.connectedClients removeObject:client];
    }
}

- (void)disconnectAllClients {
    for(ConnectedClient *client in self.connectedClients) {
        [self disconnectClient:client];
    }
}

- (NSUInteger)countByEnumeratingWithState:(NSFastEnumerationState *)state objects:(__unsafe_unretained id [])buffer count:(NSUInteger)len {
    return [self.connectedClients countByEnumeratingWithState:state objects:buffer count:len];
}

- (NSString *)description {
    return [NSString stringWithFormat:@"Connected Clients: %@", self.connectedClients];
}

- (NSMutableArray *)connectedClients {
    if(!_connectedClients) {
        _connectedClients = [NSMutableArray array];
    }
    return _connectedClients;
}

@end
