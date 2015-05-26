//
//  ClientViewController.h
//  Demo
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <Communicate/Communicate.h>

@interface ClientViewController : UIViewController <UITableViewDataSource, UITableViewDelegate, UITextFieldDelegate, ClientDelegate, UIImagePickerControllerDelegate, UINavigationControllerDelegate>

@property (weak, nonatomic) IBOutlet UITextField *textField;
@property (weak, nonatomic) IBOutlet UITableView *tableView;

@property (strong, nonatomic) Client *client;

@end
