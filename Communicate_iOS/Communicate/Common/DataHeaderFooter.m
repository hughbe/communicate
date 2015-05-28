//
//  Footer.m
//  Communicate
//
//  Created by Hugh Bellamy on 25/05/2015.
//  Copyright (c) 2015 Hugh Bellamy. All rights reserved.
//

#import "DataHeaderFooter.h"

@interface DataHeaderFooter ()

@property (strong, nonatomic) NSMutableDictionary *dictionary;

@end

@implementation DataHeaderFooter

- (instancetype)initWithString:(NSString *)string {
    self = [super init];
    if(self) {
        self.dictionary = [NSJSONSerialization JSONObjectWithData:[string dataUsingEncoding:NSUTF8StringEncoding] options:NSJSONReadingMutableContainers error:nil];
    }
    return self;
}

- (instancetype)initWithDictionary:(NSDictionary *)footerDictionary {
    if(self) {
        self.dictionary = [footerDictionary mutableCopy];
    }
    return self;
}

- (instancetype)initWithData:(NSData *)data {
    NSString *dataString = [[NSString alloc]initWithData:data encoding:NSUTF8StringEncoding];
    return [self initWithString:dataString];
}

- (NSString *)getJSON {
    if(self.dictionary.allValues.count == 0) {
        return @"";
    }
    return [[NSString alloc]initWithData:[NSJSONSerialization dataWithJSONObject:self.dictionary options:0 error:nil] encoding:NSUTF8StringEncoding];
}

- (NSData *)getData {
    return [[self getJSON]dataUsingEncoding:NSUTF8StringEncoding];
}

- (NSUInteger)length {
    return [self getData].length;
}

- (NSString *)valueForKey:(NSString *)key {
    return self.dictionary[key];
}

- (void)setValue:(NSString *)value forKey:(NSString *)key {
    self.dictionary[key] = value;
}

- (NSInteger)integerForKey:(NSString *)key {
    return [[self valueForKey:key]integerValue];
}

- (void)setInteger:(NSInteger)value forKey:(NSString *)key {
    [self setValue:[@(value) stringValue] forKey:key];
}

- (BOOL)boolForKey:(NSString *)key {
    return [[self valueForKey:key] boolValue];
}

- (void)setBool:(BOOL)value forKey:(NSString *)key {
    [self setValue:[@(value) stringValue] forKey:key];
}

- (void)addFooterEntry:(NSString *)key stringValue:(NSString *)value {
    [self setValue:value forKey:key];
}

- (void)addFooterEntry:(NSString *)key integerValue:(NSInteger)value {
    [self setInteger:value forKey:key];
}

- (void)addFooterEntry:(NSString *)key boolValue:(BOOL)value {
    [self setBool:value forKey:key];
}

- (NSString *)description {
    return [NSString stringWithFormat:@"Header/Footer: entries = %@", self.dictionary];
}

- (NSMutableDictionary *)dictionary {
    if(!_dictionary) {
        _dictionary = [NSMutableDictionary dictionary];
    }
    return _dictionary;
}

@end
