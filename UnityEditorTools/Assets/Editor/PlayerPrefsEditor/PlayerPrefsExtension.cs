using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;
using PlistCS;
using UnityEditor;
using UnityEngine;

namespace ETEditor
{
    public class PlayerPrefPair
    {
        public string Key { get; set; }

        public string Value { get; set; }

        //0 int  1 float  2 string
        public int type;

        public bool focus;
    }

    public class PlayerPrefsExtension
    {
        public static PlayerPrefPair[] GetAll()
        {
            return GetAll(PlayerSettings.companyName, PlayerSettings.productName);
        }

        public static PlayerPrefPair[] GetAll(string companyName, string productName)
        {
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                // From Unity docs: On Mac OS X PlayerPrefs are stored in ~/Library/Preferences folder, in a file named unity.[company name].[product name].plist, where company and product names are the names set up in Project Settings. The same .plist file is used for both Projects run in the Editor and standalone players.

                // Construct the plist filename from the project's settings
                string plistFilename = string.Format("unity.{0}.{1}.plist", companyName, productName);
                // Now construct the fully qualified path
                string playerPrefsPath = Path.Combine(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Preferences"),
                    plistFilename);

                // Parse the player prefs file if it exists
                if (File.Exists(playerPrefsPath))
                {
                    // Parse the plist then cast it to a Dictionary
                    object plist = Plist.readPlist(playerPrefsPath);

                    Dictionary<string, object> parsed = plist as Dictionary<string, object>;

                    // Convert the dictionary data into an array of PlayerPrefPairs
                    PlayerPrefPair[] tempPlayerPrefs = new PlayerPrefPair[parsed.Count];
                    int i = 0;
                    foreach (KeyValuePair<string, object> pair in parsed)
                    {
                        int tempType = 0;
                        if (pair.Value.GetType() == typeof(double))
                        {
                            // Some float values may come back as double, so convert them back to floats
                            // tempPlayerPrefs[i] = new PlayerPrefPair() { Key = pair.Key, Value = (float) (double) pair.Value };
                            tempType = 1;
                        }
                        else if (pair.Value.GetType() == typeof(byte[]))
                        {
                            tempType = 2;
                            //tempPlayerPrefs[i] = new PlayerPrefPair() { Key = pair.Key, Value = pair.Value.ToString() };
                        }

                        tempPlayerPrefs[i] = new PlayerPrefPair
                        {
                            Key = pair.Key, Value = pair.Value.ToString(), type = tempType,
                            focus = EditorPrefs.GetBool(pair.Key, false)
                        };

                        i++;
                    }

                    // Return the results
                    return tempPlayerPrefs;
                }

                // No existing player prefs saved (which is valid), so just return an empty array
                return new PlayerPrefPair[0];
            }

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                // From Unity docs: On Windows, PlayerPrefs are stored in the registry under HKCU\Software\[company name]\[product name] key, where company and product names are the names set up in Project Settings.
#if UNITY_5_5_OR_NEWER
                // From Unity 5.5 editor player prefs moved to a specific location
                RegistryKey registryKey =
                    Registry.CurrentUser.OpenSubKey("Software\\Unity\\UnityEditor\\" + companyName + "\\" +
                                                    productName);
#else
                Microsoft.Win32.RegistryKey registryKey =
 Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\" + companyName + "\\" + productName);
#endif

                // Parse the registry if the specified registryKey exists
                if (registryKey != null)
                {
                    // Get an array of what keys (registry value names) are stored
                    string[] valueNames = registryKey.GetValueNames();

                    // Create the array of the right size to take the saved player prefs
                    PlayerPrefPair[] tempPlayerPrefs = new PlayerPrefPair[valueNames.Length];

                    // Parse and convert the registry saved player prefs into our array
                    int i = 0;
                    foreach (string valueName in valueNames)
                    {
                        string key = valueName;

                        // Remove the _h193410979 style suffix used on player pref keys in Windows registry
                        int index = key.LastIndexOf("_");
                        key = key.Remove(index, key.Length - index);

                        // Get the value from the registry
                        object ambiguousValue = registryKey.GetValue(valueName);
                        int tempType = 0;

                        // Unfortunately floats will come back as an int (at least on 64 bit) because the float is stored as
                        // 64 bit but marked as 32 bit - which confuses the GetValue() method greatly! 
                        if (ambiguousValue.GetType() == typeof(int))
                        {
                            // If the player pref is not actually an int then it must be a float, this will evaluate to true
                            // (impossible for it to be 0 and -1 at the same time)
                            if (PlayerPrefs.GetInt(key, -1) == -1 && PlayerPrefs.GetInt(key, 0) == 0)
                            {
                                // Fetch the float value from PlayerPrefs in memory
                                ambiguousValue = PlayerPrefs.GetFloat(key);
                                tempType = 1;
                            }
                        }
                        else if (ambiguousValue.GetType() == typeof(byte[]))
                        {
                            // On Unity 5 a string may be stored as binary, so convert it back to a string
                            ambiguousValue = Encoding.Default.GetString((byte[]) ambiguousValue);
                            tempType = 2;
                        }

                        // Assign the key and value into the respective record in our output array
                        tempPlayerPrefs[i] = new PlayerPrefPair
                        {
                            Key = key, Value = ambiguousValue.ToString(), type = tempType,
                            focus = EditorPrefs.GetBool(key, false)
                        };
                        i++;
                    }

                    // Return the results
                    return tempPlayerPrefs;
                }

                // No existing player prefs saved (which is valid), so just return an empty array
                return new PlayerPrefPair[0];
            }

            throw new NotSupportedException("PlayerPrefsEditor doesn't support this Unity Editor platform");
        }
    }
}