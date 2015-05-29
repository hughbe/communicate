//
//  ServerViewController.h
//  Mac Demo
//
//  Created by Hugh Bellamy on 29/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Cocoa/Cocoa.h>
#import "Communicate.h"

@interface ServerViewController : NSViewController <CommunicatorDelegate>

@property (weak) IBOutlet NSTextField *textField;
@property (weak) IBOutlet NSTextField *receivedLabel;
@property (weak) IBOutlet NSImageView *receivedImageView;

@property (strong) Communicator *server;

@end
