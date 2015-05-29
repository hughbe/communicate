//
//  ClientViewController.m
//  Mac Demo
//
//  Created by Hugh Bellamy on 29/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "ClientViewController.h"

@implementation ClientViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    ProtocolInfo *protocol = [[ProtocolInfo alloc]initWithProtocolName:@"Test" protocolType:TransportProtocolTypeTCP domain:nil];
    CommunicatorInfo *communicatorInfo = [[CommunicatorInfo alloc]initWithName:@"Mac" port:54321 txtRecordList:nil];
    self.client = [[Communicator alloc]initWithProtocolInfo:protocol communicatorInfo:communicatorInfo delegate:self];
    [self.client startSearching];
}

- (void)viewWillDisappear {
    [super viewWillDisappear];
    [self.client stop];
}

- (IBAction)sendText:(id)sender {
    [self.client.connectionManager sendStringToAllConnections:self.textField.stringValue];
    self.textField.stringValue = @"";
}

- (void)communicatorDidStartSearching:(Communicator *)communicator {
    NSLog(@"Client started searching");
}

- (void)communicatorDidNotStartSearching:(Communicator *)communicator reason:(NSError *)reason {
    NSLog(@"Client failed to search. Reason: %@", reason.localizedDescription);
}

- (void)communicatorDidStopSearching:(Communicator *)communicator {
    NSLog(@"Client stopped searching");
}

- (void)communicator:(Communicator *)communicator didDiscoverServices:(NSArray *)services {
    NSLog(@"Client found services");
    [self.tableView reloadData];
}

- (void)communicator:(Communicator *)communicator didStartConnecting:(Connection *)client {
    NSLog(@"Client started connecting");
}

- (void)communicator:(Communicator *)communicator didNotConnect:(Connection *)client reason:(NSError *)reason {
    NSLog(@"Client failed to connect. Reason: %@", reason.localizedDescription);
}

- (void)communicator:(Communicator *)communicator didConnect:(Connection *)client {
    NSLog(@"Client did connect");
    self.receivedLabel.stringValue = @"Connected";
}

- (void)communicator:(Communicator *)communicator didDisconnect:(Connection *)client {
    NSLog(@"Client did disconnect");
}

- (void)communicator:(Communicator *)communicator didReceiveData:(CommunicationData *)data fromConnection:(Connection *)connection {
    if(data.dataType == CommunicationDataTypeString) {
        self.receivedLabel.stringValue = [data stringValue];
    }
    NSLog(@"Client received data: %@", [data stringValue]);
}

- (void)communicator:(Communicator *)communicator didSendData:(CommunicationData *)data toConnection:(Connection *)connection {
    self.receivedLabel.stringValue = @"Sent";
}

- (NSInteger)numberOfRowsInTableView:(NSTableView *)tableView {
    return self.client.searchingManager.services.count;
}

- (id)tableView:(NSTableView *)tableView objectValueForTableColumn:(NSTableColumn *)tableColumn row:(NSInteger)row {
    NSNetService *service = self.client.searchingManager.services[row];
    return service.name;
}

- (BOOL)tableView:(NSTableView *)tableView shouldSelectRow:(NSInteger)row {
    NSNetService *service = self.client.searchingManager.services[row];
    [self.client connectToService:service];
    return YES;
}

@end
