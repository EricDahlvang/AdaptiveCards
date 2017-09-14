//
//  ACOAdaptiveCards.h
//  ACOAdaptiveCards
//
//  Copyright © 2017 Microsoft. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "ACOParseResult.h"

@interface ACOAdaptiveCards:NSObject

- (instancetype)init;
+ (ACOParseResult *)FromJson:(NSString *)payload;

@end    
