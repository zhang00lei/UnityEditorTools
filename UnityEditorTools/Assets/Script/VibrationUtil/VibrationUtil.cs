using UnityEngine;

#if UNITY_IOS
using System.Runtime.InteropServices;
using UnityEngine.iOS;
#endif


    public enum HapticTypes
    {
        Selection,
        Success,
        Warning,
        Failure,
        LightImpact,
        MediumImpact,
        HeavyImpact
    }

public class VibrationUtil
{
    public const string VIBRATION_PREFS = "VibrationStatus";

    private static VibrationUtil instance = new VibrationUtil();

    private VibrationUtil()
    {
        iOSInitializeHaptics();
    }

    public static VibrationUtil Instance()
    {
        return instance;
    }

    /// Triggers the default Unity vibration, without any control over duration, pattern or amplitude
    public virtual void TriggerDefault()
    {
        if (PlayerPrefs.GetInt(VIBRATION_PREFS, 2) == 1)
        {
            return;
        }
#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
    }

    /// Triggers the default Vibrate method, which will result in a medium vibration on Android and a medium impact on iOS
    public virtual void TriggerVibrate()
    {
        if (PlayerPrefs.GetInt(VIBRATION_PREFS, 2) == 1)
        {
            return;
        }

        Vibrate();
    }

    /// Triggers the selection haptic feedback, a light vibration on Android, and a light impact on iOS
    public virtual void TriggerSelection()
    {
        if (PlayerPrefs.GetInt(VIBRATION_PREFS, 2) == 1)
        {
            return;
        }

        Haptic(HapticTypes.Selection);
    }

    /// Triggers the success haptic feedback, a light then heavy vibration on Android, and a success impact on iOS
    public virtual void TriggerSuccess()
    {
        if (PlayerPrefs.GetInt(VIBRATION_PREFS, 2) == 1)
        {
            return;
        }

        Haptic(HapticTypes.Success);
    }

    /// Triggers the warning haptic feedback, a heavy then medium vibration on Android, and a warning impact on iOS
    public virtual void TriggerWarning()
    {
        if (PlayerPrefs.GetInt(VIBRATION_PREFS, 2) == 1)
        {
            return;
        }

        Haptic(HapticTypes.Warning);
    }

    /// Triggers the failure haptic feedback, a medium / heavy / heavy / light vibration pattern on Android, and a failure impact on iOS
    public virtual void TriggerFailure()
    {
        if (PlayerPrefs.GetInt(VIBRATION_PREFS, 2) == 1)
        {
            return;
        }

        Haptic(HapticTypes.Failure);
    }

    /// Triggers a light impact on iOS and a short and light vibration on Android.
    public virtual void TriggerLightImpact()
    {
        if (PlayerPrefs.GetInt(VIBRATION_PREFS, 2) == 1)
        {
            return;
        }

        Haptic(HapticTypes.LightImpact);
    }

    /// Triggers a medium impact on iOS and a medium and regular vibration on Android.
    public virtual void TriggerMediumImpact()
    {
        if (PlayerPrefs.GetInt(VIBRATION_PREFS, 2) == 1)
        {
            return;
        }

        Haptic(HapticTypes.MediumImpact);
    }

    /// Triggers a heavy impact on iOS and a long and heavy vibration on Android.
    public virtual void TriggerHeavyImpact()
    {
        if (PlayerPrefs.GetInt(VIBRATION_PREFS, 2) == 1)
        {
            return;
        }

        Haptic(HapticTypes.HeavyImpact);
    }

    // INTERFACE ---------------------------------------------------------------------------------------------------------
    private static long LightDuration = 20;
    private static long MediumDuration = 40;
    private static long HeavyDuration = 80;
    private static int LightAmplitude = 40;
    private static int MediumAmplitude = 120;
    private static int HeavyAmplitude = 255;
    private static int _sdkVersion = -1;
    private static long[] _successPattern = { 0, LightDuration, LightDuration, HeavyDuration };
    private static int[] _successPatternAmplitude = { 0, LightAmplitude, 0, HeavyAmplitude };
    private static long[] _warningPattern = { 0, HeavyDuration, LightDuration, MediumDuration };
    private static int[] _warningPatternAmplitude = { 0, HeavyAmplitude, 0, MediumAmplitude };

    private static long[] _failurePattern =
    {
        0, MediumDuration, LightDuration, MediumDuration, LightDuration, HeavyDuration, LightDuration, LightDuration
    };

    private static int[] _failurePatternAmplitude = { 0, MediumAmplitude, 0, MediumAmplitude, 0, HeavyAmplitude, 0, LightAmplitude };

    /// Returns true if the current platform is Android, false otherwise.
    private static bool Android()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
				return true;
#else
        return false;
#endif
    }

    /// Returns true if the current platform is iOS, false otherwise
    private static bool iOS()
    {
#if UNITY_IOS && !UNITY_EDITOR
				return true;
#else
        return false;
#endif
    }

    /// Triggers a simple vibration
    private static void Vibrate()
    {
        if (Android())
        {
            AndroidVibrate(MediumDuration);
        }
        else if (iOS())
        {
            iOSTriggerHaptics(HapticTypes.MediumImpact);
        }
    }

    /// Triggers a haptic feedback of the specified type
    private static void Haptic(HapticTypes type)
    {
        if (Android())
        {
            switch (type)
            {
                case HapticTypes.Selection:
                    AndroidVibrate(LightDuration, LightAmplitude);
                    break;

                case HapticTypes.Success:
                    AndroidVibrate(_successPattern, _successPatternAmplitude, -1);
                    break;

                case HapticTypes.Warning:
                    AndroidVibrate(_warningPattern, _warningPatternAmplitude, -1);
                    break;

                case HapticTypes.Failure:
                    AndroidVibrate(_failurePattern, _failurePatternAmplitude, -1);
                    break;

                case HapticTypes.LightImpact:
                    AndroidVibrate(LightDuration, LightAmplitude);
                    break;

                case HapticTypes.MediumImpact:
                    AndroidVibrate(MediumDuration, MediumAmplitude);
                    break;

                case HapticTypes.HeavyImpact:
                    AndroidVibrate(HeavyDuration, HeavyAmplitude);
                    break;
            }
        }
        else if (iOS())
        {
            iOSTriggerHaptics(type);
        }
    }
    // INTERFACE END ---------------------------------------------------------------------------------------------------------

    // Android ---------------------------------------------------------------------------------------------------------
#if UNITY_ANDROID && !UNITY_EDITOR
			private static AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			private static AndroidJavaObject CurrentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			private static AndroidJavaObject AndroidVibrator = CurrentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
			private static AndroidJavaClass VibrationEffectClass;
			private static AndroidJavaObject VibrationEffect;
			private static int DefaultAmplitude;
#else
    private static AndroidJavaClass UnityPlayer;
    private static AndroidJavaObject CurrentActivity;
    private static AndroidJavaObject AndroidVibrator = null;
    private static AndroidJavaClass VibrationEffectClass = null;
    private static AndroidJavaObject VibrationEffect;
    private static int DefaultAmplitude;
#endif

    /// Requests a default vibration on Android, for the specified duration, in milliseconds
    private static void AndroidVibrate(long milliseconds)
    {
        if (!Android())
        {
            return;
        }

        AndroidVibrator.Call("vibrate", milliseconds);
    }

    /// Requests a vibration of the specified amplitude and duration. If amplitude is not supported by the device's SDK, a default vibration will be requested
    private static void AndroidVibrate(long milliseconds, int amplitude)
    {
        if (!Android())
        {
            return;
        }

        // amplitude is only supported 
        if ((AndroidSDKVersion() < 26))
        {
            AndroidVibrate(milliseconds);
        }
        else
        {
            VibrationEffectClassInitialization();
            VibrationEffect = VibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", new object[] { milliseconds, amplitude });
            AndroidVibrator.Call("vibrate", VibrationEffect);
        }
    }

    private static void AndroidVibrate(long[] pattern, int repeat)
    {
        if (!Android())
        {
            return;
        }

        if ((AndroidSDKVersion() < 26))
        {
            AndroidVibrator.Call("vibrate", pattern, repeat);
        }
        else
        {
            VibrationEffectClassInitialization();
            VibrationEffect = VibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform", new object[] { pattern, repeat });
            AndroidVibrator.Call("vibrate", VibrationEffect);
        }
    }

    /// Requests a vibration on Android for the specified pattern, amplitude and optional repeat
    private static void AndroidVibrate(long[] pattern, int[] amplitudes, int repeat)
    {
        if (!Android())
        {
            return;
        }

        if ((AndroidSDKVersion() < 26))
        {
            AndroidVibrator.Call("vibrate", pattern, repeat);
        }
        else
        {
            VibrationEffectClassInitialization();
            VibrationEffect = VibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform", new object[] { pattern, amplitudes, repeat });
            AndroidVibrator.Call("vibrate", VibrationEffect);
        }
    }

    /// Stops all Android vibrations that may be active
    private static void AndroidCancelVibrations()
    {
        if (!Android())
        {
            return;
        }

        AndroidVibrator.Call("cancel");
    }

    /// Initializes the VibrationEffectClass if needed.
    private static void VibrationEffectClassInitialization()
    {
        if (VibrationEffectClass == null)
        {
            VibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
        }
    }

    /// Returns the current Android SDK version as an int
    private static int AndroidSDKVersion()
    {
        if (_sdkVersion == -1)
        {
            int apiLevel = int.Parse(SystemInfo.operatingSystem.Substring(SystemInfo.operatingSystem.IndexOf("-") + 1, 3));
            _sdkVersion = apiLevel;
            return apiLevel;
        }
        else
        {
            return _sdkVersion;
        }
    }
    // Android End ---------------------------------------------------------------------------------------------------------

    // iOS ----------------------------------------------------------------------------------------------------------------
#if UNITY_IOS && !UNITY_EDITOR
			[DllImport ("__Internal")]
			private static extern void InstantiateFeedbackGenerators();
			[DllImport ("__Internal")]
			private static extern void ReleaseFeedbackGenerators();
			[DllImport ("__Internal")]
			private static extern void SelectionHaptic();
			[DllImport ("__Internal")]
			private static extern void SuccessHaptic();
			[DllImport ("__Internal")]
			private static extern void WarningHaptic();
			[DllImport ("__Internal")]
			private static extern void FailureHaptic();
			[DllImport ("__Internal")]
			private static extern void LightImpactHaptic();
			[DllImport ("__Internal")]
			private static extern void MediumImpactHaptic();
			[DllImport ("__Internal")]
			private static extern void HeavyImpactHaptic();
#else
    private static void InstantiateFeedbackGenerators()
    {
    }

    private static void ReleaseFeedbackGenerators()
    {
    }

    private static void SelectionHaptic()
    {
    }

    private static void SuccessHaptic()
    {
    }

    private static void WarningHaptic()
    {
    }

    private static void FailureHaptic()
    {
    }

    private static void LightImpactHaptic()
    {
    }

    private static void MediumImpactHaptic()
    {
    }

    private static void HeavyImpactHaptic()
    {
    }
#endif
    private static bool iOSHapticsInitialized = false;

    /// Call this method to initialize the haptics. If you forget to do it, Nice Vibrations will do it for you the first time you
    /// call iOSTriggerHaptics. It's better if you do it though.
    private static void iOSInitializeHaptics()
    {
        if (!iOS())
        {
            return;
        }

        InstantiateFeedbackGenerators();
        iOSHapticsInitialized = true;
    }

    /// Releases the feedback generators, usually you'll want to call this at OnDisable(); or anytime you know you won't need 
    /// vibrations anymore.
    private static void iOSReleaseHaptics()
    {
        if (!iOS())
        {
            return;
        }

        ReleaseFeedbackGenerators();
    }

    /// This methods tests the current device generation against a list of devices that don't support haptics, and returns true if haptics are supported, false otherwise.
    public static bool HapticsSupported()
    {
        bool hapticsSupported = false;
#if UNITY_IOS
			DeviceGeneration generation = Device.generation;
			if ((generation == DeviceGeneration.iPhone3G)
			|| (generation == DeviceGeneration.iPhone3GS)
			|| (generation == DeviceGeneration.iPodTouch1Gen)
			|| (generation == DeviceGeneration.iPodTouch2Gen)
			|| (generation == DeviceGeneration.iPodTouch3Gen)
			|| (generation == DeviceGeneration.iPodTouch4Gen)
			|| (generation == DeviceGeneration.iPhone4)
			|| (generation == DeviceGeneration.iPhone4S)
			|| (generation == DeviceGeneration.iPhone5)
			|| (generation == DeviceGeneration.iPhone5C)
			|| (generation == DeviceGeneration.iPhone5S)
			|| (generation == DeviceGeneration.iPhone6)
			|| (generation == DeviceGeneration.iPhone6Plus)
			|| (generation == DeviceGeneration.iPhone6S)
			|| (generation == DeviceGeneration.iPhone6SPlus))
			{
			hapticsSupported = false;
			}
			else
			{
			hapticsSupported = true;
			}
#endif
        return hapticsSupported;
    }

    /// iOS only : triggers a haptic feedback of the specified type
    private static void iOSTriggerHaptics(HapticTypes type)
    {
        if (!iOS())
        {
            return;
        }

        if (!iOSHapticsInitialized)
        {
            iOSInitializeHaptics();
        }

        if (HapticsSupported())
        {
            switch (type)
            {
                case HapticTypes.Selection:
                    SelectionHaptic();
                    break;

                case HapticTypes.Success:
                    SuccessHaptic();
                    break;

                case HapticTypes.Warning:
                    WarningHaptic();
                    break;

                case HapticTypes.Failure:
                    FailureHaptic();
                    break;

                case HapticTypes.LightImpact:
                    LightImpactHaptic();
                    break;

                case HapticTypes.MediumImpact:
                    MediumImpactHaptic();
                    break;

                case HapticTypes.HeavyImpact:
                    HeavyImpactHaptic();
                    break;
            }
        }
        else
        {
            // #if UNITY_IOS
            // 				Handheld.Vibrate();
            // #endif
        }
    }

    /// Returns a string containing iOS SDK informations
    private static string iOSSDKVersion()
    {
#if UNITY_IOS && !UNITY_EDITOR
				return Device.systemVersion;
#else
        return null;
#endif
    }

    // iOS End ----------------------------------------------------------------------------------------------------------------
}