
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
    ClientInfo *clientInfo = [[ClientInfo alloc]initWithPort:62930];
    ProtocolInfo *protocolInfo = [[ProtocolInfo alloc]initWithProtocolName:@"ClickBoard" protocolType:TransportProtocolTypeTCP domain:nil];
    self.client = [[Client alloc]initWithClientInfo:clientInfo protocolInfo:protocolInfo delegate:self];
    [self.client search];
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
    [self.client sendString:self.textField.text];
    self.textField.text = @"";
}

- (void)imagePickerController:(UIImagePickerController *)picker didFinishPickingMediaWithInfo:(NSDictionary *)info {
    
    UIImage *image = info[UIImagePickerControllerOriginalImage];
    [self dismissViewControllerAnimated:YES completion:nil];
    [self.client sendImage:image];
}

- (void)imagePickerControllerDidCancel:(UIImagePickerController *)picker {
    [self dismissViewControllerAnimated:YES completion:nil];
}

- (void)clientDidStartSearching:(Client *)client {
    NSLog(@"Client started searching");
}

- (void)clientDidNotSearch:(Client *)client reason:(NSError *)reason {
    NSLog(@"Client failed to search. Reason: %@", reason.localizedDescription);
}

- (void)clientDidStopSearching:(Client *)client {
    NSLog(@"Client stopped searching");
}

- (void)clientDidUpdateServices:(Client *)client {
    NSLog(@"Client found services");
    [self.tableView reloadData];
}

- (void)clientDidStartConnecting:(Client *)client {
    NSLog(@"Client started connecting");
}

- (void)clientDidNotConnect:(Client *)client reason:(NSError *)reason {
    NSLog(@"Client failed to connect. Reason: %@", reason.localizedDescription);
}

- (void)clientDidDisconnect:(Client *)client {
    NSLog(@"Client did disconnect");
}

- (void)clientDidConnect:(Client *)client {
    NSLog(@"Client did connect");
}

- (void)client:(Client *)client didReceiveData:(CommunicationData *)data {
    NSLog(@"Client received data: %@", [data stringValue]);
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView {
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    return self.client.services.count;
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:@"Cell" forIndexPath:indexPath];
    if(indexPath.row < self.client.services.count) {
        NSNetService *netService = self.client.services[indexPath.row];
        cell.textLabel.text = netService.name;
    }
    return cell;
}

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath {
    if(indexPath.row < self.client.services.count) {
        NSNetService *netService = self.client.services[indexPath.row];
        [self.client connectToService:netService];
    }
}

@end
