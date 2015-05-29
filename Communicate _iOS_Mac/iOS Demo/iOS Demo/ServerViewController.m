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
    TXTRecordList *TXTRecords = [[TXTRecordList alloc]init];
    [TXTRecords addTXTRecordWithKey:"platform" value:"iOS"];

    CommunicatorInfo *communicatorInfo = [[CommunicatorInfo alloc]initWithName:[UIDevice currentDevice].name port:12345 txtRecordList:TXTRecords];
    ProtocolInfo *protocol = [[ProtocolInfo alloc]initWithProtocolName:@"Test" protocolType:TransportProtocolTypeTCP domain:nil];
    
    self.server = [[Communicator alloc]initWithProtocolInfo:protocol communicatorInfo:communicatorInfo delegate:self];
    [self.server publish];
    [self.server startListening];
}

- (void)communicatorDidStartPublishing:(Communicator *)communicator {
    
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
    NSLog(@"Connected to client");
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
        self.textReceivedLabel.text = [data stringValue];
    }
    else if(data.dataType == CommunicationDataTypeImage) {
        self.imageView.image = [data imageValue];
    }
    NSLog(@"Received %ld data: %@", (long)[data dataType], [data stringValue]);
}

- (void)communicator:(Communicator *)communicator didSendData:(CommunicationData *)data toConnection:(Connection *)connection {
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
    [self.server.connectionManager sendStringToAllConnections:self.textField.text];
    self.textField.text = @"";
}

- (void)imagePickerController:(UIImagePickerController *)picker didFinishPickingMediaWithInfo:(NSDictionary *)info {
    
    UIImage *image = info[UIImagePickerControllerOriginalImage];
    [self dismissViewControllerAnimated:YES completion:nil];
    [self.server.connectionManager sendImageToAllConnections:image];
}

- (void)imagePickerControllerDidCancel:(UIImagePickerController *)picker {
    [self dismissViewControllerAnimated:YES completion:nil];
}

@end
