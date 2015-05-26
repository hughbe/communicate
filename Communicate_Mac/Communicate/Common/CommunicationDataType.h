//
//  DataType.h
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>

typedef NS_ENUM(NSInteger, CommunicationDataType) {
    CommunicationDataTypeString,
    CommunicationDataTypeJSON,
    CommunicationDataTypeFile,
    CommunicationDataTypeImage,
    CommunicationDataTypeOther
};

typedef NS_ENUM(NSInteger, JSONObjectType) {
    JSONObjectTypeNone,
    JSONObjectTypeDictionary,
    JSONObjectTypeArray,
    JSONObjectTypeOther
};
