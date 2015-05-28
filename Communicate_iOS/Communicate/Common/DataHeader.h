//
//  DataHeader.h
//  Communicate
//
//  Created by Hugh Bellamy on 25/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "DataHeaderFooter.h"
#import "CommunicationDataType.h"

@interface DataHeader : DataHeaderFooter

@property (strong, nonatomic) NSString *fileName;
@property (assign, nonatomic) JSONObjectType JSONObjectType;

@end
