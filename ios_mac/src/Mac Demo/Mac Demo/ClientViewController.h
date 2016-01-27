//
//  ClientViewController.h
//  Mac Demo
//
//  Created by Hugh Bellamy on 29/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Cocoa/Cocoa.h>
#import "Communicate.h"

@interface ClientViewController : NSViewController <CommunicatorDelegate, NSTableViewDataSource, NSTableViewDelegate>

@property (weak) IBOutlet NSTableView *tableView;

@property (strong) Communicator *client;
@property (weak) IBOutlet NSTextField *textField;
@property (weak) IBOutlet NSTextField *receivedLabel;

@end
