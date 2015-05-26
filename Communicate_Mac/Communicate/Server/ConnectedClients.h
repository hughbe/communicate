//
//  ConnectedClients.h
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>

@class ConnectedClient;
@interface ConnectedClients : NSObject <NSFastEnumeration>

- (ConnectedClient *)objectAtIndexedSubscript:(NSUInteger)index;

- (void)addClient:(ConnectedClient *)client;
- (void)disconnectClient:(ConnectedClient *)client;

- (void)disconnectAllClients;

@end
