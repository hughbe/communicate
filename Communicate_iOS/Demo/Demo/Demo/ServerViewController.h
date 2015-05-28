//
//  ServerViewController.h
//  Demo
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <Communicate/Communicate.h>

@interface ServerViewController : UIViewController <CommunicatorDelegate, UITextFieldDelegate, UIImagePickerControllerDelegate, UINavigationControllerDelegate>

@property (strong, nonatomic) Communicator *server;

@property (weak, nonatomic) IBOutlet UILabel *textReceivedLabel;
@property (weak, nonatomic) IBOutlet UITextField *textField;
@property (weak, nonatomic) IBOutlet UIImageView *imageView;

@end
