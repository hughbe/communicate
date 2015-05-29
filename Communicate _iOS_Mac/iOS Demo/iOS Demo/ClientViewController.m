
//
//  ClientViewController.m
//  Demo
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "ClientViewController.h"

@implementation ClientViewController

- (void)viewDidLoad {
    ProtocolInfo *protocol = [[ProtocolInfo alloc]initWithProtocolName:@"Test" protocolType:TransportProtocolTypeTCP domain:nil];
    CommunicatorInfo *communicatorInfo = [[CommunicatorInfo alloc]initWithName:[UIDevice currentDevice].name port:5432 txtRecordList:nil];
    self.client = [[Communicator alloc]initWithProtocolInfo:protocol communicatorInfo:communicatorInfo delegate:self];
    [self.client startSearching];
}

- (void)viewWillDisappear:(BOOL)animated {
    [super viewWillDisappear:animated];
    [self.client stop];
}

- (IBAction)sendText:(id)sender {
    [self sendText];
}

- (BOOL)textFieldShouldReturn:(UITextField *)textField {
    [self sendText];
    return YES;
}

- (IBAction)sendImage:(id)sender {
    UIImagePickerController *imagePickerController = [[UIImagePickerController alloc]init];
    imagePickerController.delegate = self;
    [self presentViewController:imagePickerController animated:YES completion:nil];
}

- (void)sendText {
    [self.client.connectionManager sendStringToAllConnections:self.textField.text];
    self.textField.text = @"";
}

- (void)imagePickerController:(UIImagePickerController *)picker didFinishPickingMediaWithInfo:(NSDictionary *)info {
    
    UIImage *image = info[UIImagePickerControllerOriginalImage];
    [self dismissViewControllerAnimated:YES completion:nil];
    [self.client.connectionManager sendImageToAllConnections:image];
}

- (void)imagePickerControllerDidCancel:(UIImagePickerController *)picker {
    [self dismissViewControllerAnimated:YES completion:nil];
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
}

- (void)communicator:(Communicator *)communicator didDisconnect:(Connection *)client {
    NSLog(@"Client did disconnect");
}

- (void)communicator:(Communicator *)communicator didReceiveData:(CommunicationData *)data fromConnection:(Connection *)connection {
    NSLog(@"Client received data: %@", [data stringValue]);
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView {
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    return self.client.searchingManager.services.count;
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:@"Cell" forIndexPath:indexPath];
    if(indexPath.row < self.client.searchingManager.services.count) {
        NSNetService *netService = self.client.searchingManager.services[indexPath.row];
        cell.textLabel.text = netService.name;
    }
    return cell;
}

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath {
    if(indexPath.row < self.client.searchingManager.services.count) {
        NSNetService *netService = self.client.searchingManager.services[indexPath.row];
        [self.client connectToService:netService];
    }
}

@end
