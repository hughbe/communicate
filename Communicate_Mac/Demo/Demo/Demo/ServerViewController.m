//
//  ServerViewController.m
//  Demo
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "ServerViewController.h"

@implementation ServerViewController

- (void)viewDidLoad {
    int port = 10234;
    ServerInfo *serverInfo = [[ServerInfo alloc]initWithName:[UIDevice currentDevice].name port:port txtRecordList:nil];
    ProtocolInfo *protocolInfo = [[ProtocolInfo alloc]initWithProtocolName:@"ClickBoard" protocolType:TransportProtocolTypeTCP domain:nil];
    
    self.server = [[Server alloc]initWithServerInfo:serverInfo protocolInfo:protocolInfo delegate:self];
    [self.server publishAndStartListening];
}

- (void)serverDidStartPublishing:(Server *)server {
    //NSLog(@"Publishing server");
}

- (void)serverDidPublish:(Server *)server {
    NSLog(@"Server published");
}

- (void)serverDidNotPublish:(Server *)server reason:(NSError *)error {
    NSLog(@"Server did not publish. Reason: %@", error.localizedDescription);
}

- (void)serverDidUnpublish:(Server *)server {
    NSLog(@"Server unpublished");
}

- (void)serverDidStartListening:(Server *)server {
    NSLog(@"Server listening");
}

- (void)serverDidNotStartListening:(Server *)server reason:(NSError *)error {
    NSLog(@"Server did not start listening. Reason: %@", error.localizedDescription);
}

- (void)serverDidStopListening:(Server *)server {
    NSLog(@"Stopped listening");
}

- (void)server:(Server *)server didStartConnectingToClient:(ConnectedClient *)client {
    NSLog(@"Connecting to client");
}

- (void)server:(Server *)server didConnectToClient:(ConnectedClient *)client {
    NSLog(@"Connected to client");
}

- (void)server:(Server *)server didNotConnectToClient:(ConnectedClient *)client reason:(NSError *)error {
    NSLog(@"Did not connect to client, Reason: %@", error.localizedDescription);
}

- (void)server:(Server *)server didDisconnectFromClient:(ConnectedClient *)client {
    NSLog(@"Disconnected from client");
}

- (void)server:(Server *)server didReceiveData:(CommunicationData *)data {
    //NSData *dataRepresentation = [data dataValue];
    if(data.dataType == CommunicationDataTypeString) {
        self.textReceivedLabel.text = [data stringValue];
    }
    else if(data.dataType == CommunicationDataTypeImage) {
        self.imageView.image = [data imageValue];
    }
    NSLog(@"Received %ld data: %@", (long)[data dataType], [data stringValue]);
}

- (void)server:(Server *)server didSendData:(CommunicationData *)data {
    NSLog(@"Sent %ld data: %@", (long)[data dataType], [data stringValue]);
}

- (IBAction)sendText:(id)sender {
    [self.textField resignFirstResponder];
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
    [self.server sendStringToAllClients:self.textField.text];
    self.textField.text = @"";
}

- (void)imagePickerController:(UIImagePickerController *)picker didFinishPickingMediaWithInfo:(NSDictionary *)info {
    
    UIImage *image = info[UIImagePickerControllerOriginalImage];
    [self dismissViewControllerAnimated:YES completion:nil];
    [self.server sendImageToAllClients:image];
}

- (void)imagePickerControllerDidCancel:(UIImagePickerController *)picker {
    [self dismissViewControllerAnimated:YES completion:nil];
}

@end
