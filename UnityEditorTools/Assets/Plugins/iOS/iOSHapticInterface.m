// This iOS haptic interface is a pretty straightforward implementation of UIKit's framework :
// You can learn more about these methods at https://developer.apple.com/documentation/uikit/animation_and_haptics
// DO NOT remove this from your project, or iOS vibrations won't work anymore!

#import <Foundation/Foundation.h>

UISelectionFeedbackGenerator* SelectionFeedbackGenerator;
UINotificationFeedbackGenerator* NotificationFeedbackGenerator;
UIImpactFeedbackGenerator* LightImpactFeedbackGenerator;
UIImpactFeedbackGenerator* MediumImpactFeedbackGenerator;
UIImpactFeedbackGenerator* HeavyImpactFeedbackGenerator;

// INIT METHOD ---------------------------------------------------------------------------

void InstantiateFeedbackGenerators()
{
    SelectionFeedbackGenerator = [[UISelectionFeedbackGenerator alloc] init];
    NotificationFeedbackGenerator = [[UINotificationFeedbackGenerator alloc] init];
    LightImpactFeedbackGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
    MediumImpactFeedbackGenerator =  [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
    HeavyImpactFeedbackGenerator =  [[UIImpactFeedbackGenerator alloc] initWithStyle:
                                     UIImpactFeedbackStyleHeavy];
}

// RELEASE METHOD ---------------------------------------------------------------------------

void ReleaseFeedbackGenerators ()
{
    SelectionFeedbackGenerator = nil;
    NotificationFeedbackGenerator = nil;
    LightImpactFeedbackGenerator = nil;
    MediumImpactFeedbackGenerator = nil;
    HeavyImpactFeedbackGenerator = nil;
}

// PREPARATION METHODS ----------------------------------------------------------------------

void PrepareSelectionFeedbackGenerator()
{
    [SelectionFeedbackGenerator prepare];
}

void PrepareNotificationFeedbackGenerator()
{
    [NotificationFeedbackGenerator prepare];
}

void PrepareLightImpactFeedbackGenerator()
{
    [LightImpactFeedbackGenerator prepare];
}

void PrepareMediumImpactFeedbackGenerator()
{
    [MediumImpactFeedbackGenerator prepare];
}

void PrepareHeavyImpactFeedbackGenerator()
{
    [HeavyImpactFeedbackGenerator prepare];
}

// FEEDBACK TRIGGER METHODS -------------------------------------------------------------------------

void SelectionHaptic()
{
    [SelectionFeedbackGenerator prepare];
    [SelectionFeedbackGenerator selectionChanged];
}

void SuccessHaptic()
{
    [NotificationFeedbackGenerator prepare];
    [NotificationFeedbackGenerator notificationOccurred:UINotificationFeedbackTypeSuccess];
}

void WarningHaptic()
{
    [NotificationFeedbackGenerator prepare];
    [NotificationFeedbackGenerator notificationOccurred:UINotificationFeedbackTypeWarning];
}

void FailureHaptic()
{
    [NotificationFeedbackGenerator prepare];
    [NotificationFeedbackGenerator notificationOccurred:UINotificationFeedbackTypeError];
}

void LightImpactHaptic()
{
    [LightImpactFeedbackGenerator prepare];
    [LightImpactFeedbackGenerator impactOccurred];
}

void MediumImpactHaptic()
{
    [MediumImpactFeedbackGenerator prepare];
    [MediumImpactFeedbackGenerator impactOccurred];
}

void HeavyImpactHaptic()
{
    [HeavyImpactFeedbackGenerator prepare];
    [HeavyImpactFeedbackGenerator impactOccurred];
}
