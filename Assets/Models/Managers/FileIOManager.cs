using System;
using System.IO;
using UnityEngine;

namespace Assets.Models.Managers
{
    // INFO
    //  - This static class provides globally-accessible methods for local, in-device data management (Read & Write).
    //  - The goal is to persist relevant data (i.e. class objects) through application runs.
    //  - Uses the JSON serialization method, meaning that the use of [Serializable] objects is required,
    //     to allow JSON (de)serialization.
    //  - Made with help of a few Internet searches.
    //  - Some background that brought this implementation:
    //
    // ---------------------------------------------------------------------------------------------- //
    // Question #1: In Unity, what types of local data persistence methods are most used?
    // ---------------------------------------------------------------------------------------------- //
    //  Some investigation was done regarding the topic of local-data storage.
    //  The most frequently mentioned names were: PlayerPrefs, JSON serialization, Binary serialization, and SQLite.
    //  (Info source: Internet)
    //
    //  *** PLAYERPREFS ***
    //  > 1MB size limit for data storage
    //  > For simple/primitive data types (string, int, float...)
    //  > Typical use case: (literaly) saving User Preferences, e.g.: game volume, selected game preferences, ...
    // 
    //   - Advantages: implementation simplicity "out of the box".
    //   - Disadvantages: limited storage size, limited support of Data Types, insecure
    //   - BOTTOM LINE:
    //     > PlayerPrefs is a potential choice, even for the purpose of object storage (by converting the object
    //        to its Json string and saving the record as the whole string), but was dropped in favor of JSON, since
    //        there is already plenty support for JSON data management with the JsonUtility class, or even the
    //        Newtonsoft.Json package.
    //     > Additionally, the path Unity chooses to manage PlayerPreferences seems a bit odd -- it uses the
    //        platform's registry location for data storage (example: on Windows, the Registry's HKCU location)
    //        (More info: https://docs.unity3d.com/ScriptReference/PlayerPrefs.html)
    //
    //   *** ALTERNATIVES ***
    //   Main alternatives found:
    //   1) Save in text (JSON) file, with JSON serialization - allows working with more complex types (e.g. Classes)
    //   2) Save in Binary files: Binary serialization (found to be too complex and error-prone for what is needed) 
    //   3) SQLite - a more "relational" approach to data storage, similar to Databases.
    //   4) Manually implementing the whole save-file & load-file system.
    //
    //   In short... 
    //      -> For simple, small-scale (<= 1MB) data storage, low security, and simple data structures: PlayerPrefs.
    //      -> For more extensive data management:
    //          >> JSON/Binary serialization (file storage);
    //          >> SQLite (databases);
    //          >> or, lastly, Cloud-based solutions (?).
    //  
    //   Possible interesting topics:
    //   > https://www.reddit.com/r/Unity3D/comments/1fanu5/how_do_i_use_playerprefs_to_save_a_game_object/
    //   > https://www.reddit.com/r/Unity3D/comments/eakh1m/unity_tip_saving_data_with_json_and_playerprefs/
    //   > https://www.youtube.com/watch?v=uD7y4T4PVk0
    // ---------------------------------------------------------------------------------------------- //
    //
    // ---------------------------------------------------------------------------------------------- //
    // Question #2: When using local data persistence, what are the ideal paths to use?
    // ---------------------------------------------------------------------------------------------- //
    // When researching the best locations for local data storage, an interesting read was taken from a link above: 
    // "
    //      If you are already writing to json, it's pretty easy just to write to a file.
    //      Application.persistentDataPath will get you a location where unity has read and write access on any platform
    //      then you can use basic file IO commands to write the json to file.
    // "
    // --------------------------------------------------------------------------------------------- //


    // TODO delete these
    [Serializable]
    public class ClasseGravar
    {
        public string playerName;
        public int playerScore;
    }

    [Serializable]
    public class UserData
    {
        public int userId;
    }
    //

    public static class FileIOManager
    {
        private const string DEFAULT_FILENAME = "saveData.json";


        public static void SaveObjectToDevice<T>(T objectToSave, string filename = DEFAULT_FILENAME)
        {
            // Saves an object into local file storage, depending on the platform's Persistent Data Directory.
            // The goal is to keep the data accessible between application runs. 
            // (For more info: https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html)
            // The object must be marked as [Serializable], so that it can be parsed into a JSON object.
            // By supporting Generics, allows to save any Class type (vs previous version, that was tied to a single Type)
            // Defaults to a generic filename if the path is not specified.

            try
            {
                // Serialize the object into JSON string
                var jsonStr = JsonUtility.ToJson(objectToSave);

                // NOTE: ".WriteAllText()" creates a new file, or overwrites if it already exists!
                var path = Path.Combine(Application.persistentDataPath, filename);
                File.WriteAllText(path, jsonStr);
            }
            catch (Exception e)
            {
                Debug.Log("An exception occurred while trying to save the file. Message: [" + e.Message + "]");
            }
        }

        public static T LoadObjectFromDevice<T>(string filename = DEFAULT_FILENAME)
        {
            // Attempts to load an Object from local file storage (JSON), according to the device's Persistent Data Directory.
            //  Follows the same rules as the SaveObject function, where the object to be fetched must be Serializable into JSON string -
            //  - which will most likely be, if it was saved successfully in first place. 
            // If, however, the file is not found or data loading fails (either because it never existed, or, somehow, got corrupted),
            //  it will be marked as NULL, informing the program that a new file should be (over)written.
            // By supporting Generics, allows reading any saved Class type (vs previous version, that was tied to a single Type)
            // Defaults to a generic loadPath if the path is not specified.


            T loadedObject = default(T);
            var path = Application.persistentDataPath + "/" + filename;

            if (File.Exists(path))
            {
                try
                {
                    // Load data
                    string objJson = File.ReadAllText(path);
                    loadedObject = JsonUtility.FromJson<T>(objJson);

                    // If data load was successful, the object will now be ready.
                    // ( From here on, use as needed... )
                }
                catch (Exception e)
                {
                    Debug.Log("An exception occurred while trying to read the file. Message: [" + e.Message + "]");
                    loadedObject = default(T);
                }
            }

            // NOTE: if load fails, object will be returned as NULL!
            return loadedObject;
        }
    }
}