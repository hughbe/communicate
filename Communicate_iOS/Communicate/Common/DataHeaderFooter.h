//
//  Footer.h
//  Communicate
//
//  Created by Hugh Bellamy on 25/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface DataHeaderFooter : NSObject

@property (assign, readonly, nonatomic) NSUInteger length;

- (instancetype)initWithString:(NSString *)footerString;
- (instancetype)initWithDictionary:(NSDictionary *)footerDictionary;

- (instancetype)initWithData:(NSData *)data;

- (NSString *)getJSON;
- (NSData *)getData;

- (NSString *)valueForKey:(NSString *)key;
- (void)setValue:(NSString *)value forKey:(NSString *)key;

- (NSInteger)integerForKey:(NSString *)key;
- (void)setInteger:(NSInteger)value forKey:(NSString *)key;

- (BOOL)boolForKey:(NSString *)key;
- (void)setBool:(BOOL)value forKey:(NSString *)key;

- (void)addFooterEntry:(NSString *)key stringValue:(NSString *)value;
- (void)addFooterEntry:(NSString *)key integerValue:(NSInteger)value;
- (void)addFooterEntry:(NSString *)key boolValue:(BOOL)value;

@end
