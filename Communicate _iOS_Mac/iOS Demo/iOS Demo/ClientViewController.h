//
//  ClientViewController.h
//  Demo
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Communicate.h"

@interface ClientViewController : UIViewController <UITableViewDataSource, UITableViewDelegate, UITextFieldDelegate, CommunicatorDelegate, UIImagePickerControllerDelegate, UINavigationControllerDelegate>

@property (weak, nonatomic) IBOutlet UITextField *textField;
@property (weak, nonatomic) IBOutlet UITableView *tableView;

@property (strong, nonatomic) Communicator *client;

@end
