//
//  ServerInfo.m
//  Communicate
//
//  Created by Hugh Bellamy on 24/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "ServerInfo.h"

#import "TXTRecordList.h"

@interface ServerInfo ()

@property (copy, readwrite, nonatomic) NSString *readableName;
@property (assign, readwrite, nonatomic) int port;

@property (strong, readwrite, nonatomic) TXTRecordList *TXTRecordList;

@end

@implementation ServerInfo

- (instancetype)initWithName:(NSString *)readableName port:(int)port txtRecordList:(TXTRecordList *)txtRecordList {
    self = [super init];
    if(self) {
        self.readableName = readableName;
        self.port = port;
        self.TXTRecordList = txtRecordList;
    }
    return self;
}

- (NSString *)description {
    return [NSString stringWithFormat:@"ServerInfo: name = %@; port = %ld; TXT records = %@", self.readableName, (long)self.port, self.TXTRecordList];
}

- (NSString *)readableName {
    if(!_readableName) {
        _readableName = @"";
    }
    return _readableName;
}

- (TXTRecordList *)TXTRecordList {
    if(!_TXTRecordList) {
        _TXTRecordList = [[TXTRecordList alloc]init];
    }
    return _TXTRecordList;
}

@end
