//
//  ServerViewController.m
//  Mac Demo
//
//  Created by Hugh Bellamy on 29/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "ServerViewController.h"

@implementation ServerViewController

- (void)viewDidLoad {
    int port = 10234;
    CommunicatorInfo *communicatorInfo = [[CommunicatorInfo alloc]initWithName:@"Mac" port:port txtRecordList:nil];
    ProtocolInfo *protocol = [[ProtocolInfo alloc]initWithProtocolName:@"Test" protocolType:TransportProtocolTypeTCP domain:nil];
    
    self.server = [[Communicator alloc]initWithProtocolInfo:protocol communicatorInfo:communicatorInfo delegate:self];
    [self.server publish];
    [self.server startListening];
}

- (void)viewWillDisappear {
    [super viewWillDisappear];
    [self.server stop];
}

- (IBAction)sendText:(id)sender {
    [self.server.connectionManager sendStringToAllConnections:[self.textField stringValue]];
    self.textField.stringValue = @"";
}

- (void)communicatorDidPublish:(Communicator *)communicator {
    NSLog(@"Server published");
}

- (void)communicatorDidNotPublish:(Communicator *)communicator reason:(NSError *)reason {
    NSLog(@"Server did not publish. Reason: %@", reason.localizedDescription);
}

- (void)communicatorDidUnpublish:(Communicator *)communicator {
    NSLog(@"Server unpublished");
}

- (void)communicatorDidStartListening:(Communicator *)communicator {
    NSLog(@"Server listening");
}

- (void)communicatorDidNotStartListening:(Communicator *)communicator reason:(NSError *)reason {
    NSLog(@"Server did not start listening. Reason: %@", reason.localizedDescription);
}

- (void)communicatorDidStopListening:(Communicator *)communicator {
    NSLog(@"Stopped listening");
}

- (void)communicator:(Communicator *)communicator didStartConnecting:(Connection *)client {
    NSLog(@"Connecting to client");
}

- (void)communicator:(Communicator *)communicator didConnect:(Connection *)client {
    NSLog(@"Client did connect");
    self.receivedLabel.stringValue = @"Connected";
}

- (void)communicator:(Communicator *)communicator didNotConnect:(Connection *)client reason:(NSError *)reason {
    NSLog(@"Did not connect to client, Reason: %@", reason.localizedDescription);
}

- (void)communicator:(Communicator *)communicator didDisconnect:(Connection *)client {
    NSLog(@"Disconnected from client");
}

- (void)communicator:(Communicator *)communicator didReceiveData:(CommunicationData *)data fromConnection:(Connection *)connection {
    //NSData *dataRepresentation = [data dataValue];
    if(data.dataType == CommunicationDataTypeString) {
        self.receivedLabel.stringValue = [data stringValue];
    }
    else if(data.dataType == CommunicationDataTypeImage) {
        self.receivedImageView.image = [data imageValue];
    }
    NSLog(@"Received %ld data: %@", (long)[data dataType], [data stringValue]);
}

- (void)communicator:(Communicator *)communicator didSendData:(CommunicationData *)data toConnection:(Connection *)connection {
    self.receivedLabel.stringValue = @"Sent";
}

@end
