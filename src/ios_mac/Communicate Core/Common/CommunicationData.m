//
//  CommunicationData.m
//  Communicate
//
//  Created by Hugh Bellamy on 25/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "CommunicationData.h"
#import "DataInfo.h"
#import "DataContent.h"
#import "DataHeader.h"
#import "DataFooter.h"
#import "DataSerializers.h"

@interface CommunicationData ()

@property (strong, readwrite, nonatomic) DataInfo *dataInfo;
@property (strong, readwrite, nonatomic) DataHeader *header;
@property (strong, readwrite, nonatomic) DataContent *content;
@property (strong, readwrite, nonatomic) DataFooter *footer;

@end

@implementation CommunicationData

- (instancetype)initWithType:(CommunicationDataType)dataType header:(DataHeader *)header content:(DataContent *)content footer:(DataFooter *)footer {
    self = [super init];
    if(self) {
        self.dataInfo = [[DataInfo alloc]initWithDataType:dataType header:header content:content footer:footer];
        self.header = header;
        self.content = content;
        self.footer = footer;
    }
    return self;
}

+ (CommunicationData *)fromString:(NSString *)string withEncoding :(NSStringEncoding)encoding {
    DataHeader *header = [[DataHeader alloc]init];
    DataContent *content = [[DataContent alloc]initWithString:string withEncoding:encoding];
    DataFooter *footer = [[DataFooter alloc]init];
    
    return [[CommunicationData alloc]initWithType:CommunicationDataTypeString header:header content:content footer:footer];
}

#if TARGET_OS_IPHONE

+ (CommunicationData *)fromImage:(UIImage *)image name:(NSString *)name {
    DataHeader *header = [[DataHeader alloc]init];
    header.fileName = name;
    
    DataContent *content = [[DataContent alloc]initWithImage:image];
    DataFooter *footer = [[DataFooter alloc]init];
    
    return [[CommunicationData alloc]initWithType:CommunicationDataTypeImage header:header content:content footer:footer];
}

#elif TARGET_OS_MAC

+ (CommunicationData *)fromImage:(NSImage *)image name:(NSString *)name  {
    DataHeader *header = [[DataHeader alloc]init];
    header.fileName = name;
    
    DataContent *content = [[DataContent alloc]initWithImage:image];
    DataFooter *footer = [[DataFooter alloc]init];
    
    return [[CommunicationData alloc]initWithType:CommunicationDataTypeImage header:header content:content footer:footer];
}

#endif

+ (CommunicationData *)fromFile:(NSString *)filePath name:(NSString *)name {
    DataHeader *header = [[DataHeader alloc]init];
    header.fileName = name;
    
    DataContent *content = [[DataContent alloc]initWithFilePath:filePath];
    DataFooter *footer = [[DataFooter alloc]init];
    
    return [[CommunicationData alloc]initWithType:CommunicationDataTypeFile header:header content:content footer:footer];
}

+ (CommunicationData *)fromDictionary:(NSDictionary *)dictionary {
    NSData *data = [NSJSONSerialization dataWithJSONObject:dictionary options:NSJSONWritingPrettyPrinted error:nil];
    return [CommunicationData fromJSONData:data JSONObjectType:JSONObjectTypeDictionary];
}

+ (CommunicationData *)fromArray:(NSArray *)array {
    NSData *data = [NSJSONSerialization dataWithJSONObject:array options:NSJSONWritingPrettyPrinted error:nil];
    return [CommunicationData fromJSONData:data JSONObjectType:JSONObjectTypeArray];
}

+ (CommunicationData *)fromJSONData:(NSData *)data JSONObjectType:(JSONObjectType)objectType {
    DataHeader *header = [[DataHeader alloc]init];
    header.JSONObjectType = objectType;
    DataContent *content = [[DataContent alloc]initWithData:data];
    DataFooter *footer = [[DataFooter alloc]init];
    
    return [[CommunicationData alloc]initWithType:CommunicationDataTypeJSON header:header content:content footer:footer];
}

+ (CommunicationData *)fromData:(NSData *)data {
    DataHeader *header = [[DataHeader alloc]init];
    DataContent *content = [[DataContent alloc]initWithData:data];
    DataFooter *footer = [[DataFooter alloc]init];
    
    return [[CommunicationData alloc]initWithType:CommunicationDataTypeOther header:header content:content footer:footer];
}

+ (CommunicationData *)terminationData {
    return [[CommunicationData alloc]initWithType:CommunicationDataTypeTermination header:nil content:nil footer:nil];
}

- (CommunicationDataType)dataType {
    return self.dataInfo.dataType;
}

- (NSString *)stringValue {
    return [StringSerializer stringFromData:[self.content getData]];
}

#if TARGET_OS_IPHONE

- (UIImage *)imageValue {
    return [ImageSerializer imageFromData:[self.content getData]];
}

#elif TARGET_OS_MAC

- (NSImage *)imageValue {
    return [ImageSerializer imageFromData:[self.content getData]];
}

#endif

- (NSData *)dataValue {
    return [self.content getData];
}

- (id)jsonValue {
    return [NSJSONSerialization JSONObjectWithData:[self.content getData] options:NSJSONReadingAllowFragments error:nil];
}

- (NSString *)description {
    return [NSString stringWithFormat:@"Communication Data: %@; %@; %@; %@", self.dataInfo, self.header, self.content, self.footer];
}

- (DataInfo *)dataInfo {
    if(!_dataInfo) {
        _dataInfo = [[DataInfo alloc]init];
    }
    return _dataInfo;
}

- (DataHeader *)header {
    if(!_header) {
        _header = [[DataHeader alloc] init];
    }
    return _header;
}

- (DataContent *)content {
    if(!_content) {
        _content = [[DataContent alloc]init];
    }
    return _content;
}

- (DataFooter *)footer {
    if(!_footer) {
        _footer = [[DataFooter alloc]init];
    }
    return _footer;
}

@end
