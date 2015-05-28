	//
//  DataHeader.h
//  Communicate
//
//  Created by Hugh Bellamy on 25/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "CommunicationDataType.h"

@class DataContent;
@class DataHeader;
@class DataFooter;

@interface DataInfo : NSObject

@property (assign, readonly, nonatomic) CommunicationDataType dataType;
@property (assign, readonly, nonatomic) NSUInteger headerLength;
@property (assign, readonly, nonatomic) NSUInteger contentLength;
@property (assign, readonly, nonatomic) NSUInteger footerLength;

+ (NSUInteger)length;

- (instancetype)initWithDataType:(CommunicationDataType)dataType header:(DataHeader *)header content:(DataContent *)content footer:(DataFooter *)footer;
- (instancetype)initWithData:(NSData *)data;

- (NSData *)getData;

@end
