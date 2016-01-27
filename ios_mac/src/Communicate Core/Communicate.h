//
//  Communicate.h
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#if TARGET_OS_IPHONE
@import UIKit;
#elif TARGET_OS_MAC
@import AppKit;
#endif

//! Project version number for Communicate.
FOUNDATION_EXPORT double CommunicateVersionNumber;

//! Project version string for Communicate.
FOUNDATION_EXPORT const unsigned char CommunicateVersionString[];

// In this header, you should import all the public headers of your framework using statements like #import "PublicHeader.h"

#import "Communicator.h"
#import "ProtocolInfo.h"
#import "CommunicatorInfo.h"

#import "PublishingManager.h"
#import "ListeningManager.h"
#import "SearchingManager.h"
#import "ConnectionsManager.h"

#import "TXTRecord.h"
#import "TXTRecordList.h"

#import "CommunicationData.h"
#import "CommunicationDataType.h"
#import "DataInfo.h"
#import "DataHeader.h"
#import "DataFooter.h"
#import "DataContent.h"

#import "DataSerializers.h"


