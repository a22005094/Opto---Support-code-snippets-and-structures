using System;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using Assets.Models;
using Assets.Models.Managers;

namespace Assets.Scripts
{
    public class TestDataScript : MonoBehaviour
    {
        //
        // TODO confirm where to place the functions ConvertToHex() & GenerateNewUserID()
        //

        private string ConvertToHexStr(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
            {
                // Hex representation of this Byte, in lowercase if alphabetic
                sb.Append(bytes[i].ToString("x2"));
            }

            return sb.ToString();
        }

        private string GenerateNewUserID()
        {
            // Reference1: https://stackoverflow.com/questions/46194754/how-to-hex-encode-a-sha-256-hash
            // Reference2: https://dotnetfiddle.net/QbsKTc
            // - INPUT: uses a random number between 0 and 10.000, and the current TimeStamp
            //     (in format "yyyy-mm-ddThh:mm:ss"), as "noise" for the SHA256 hash generation.
            // - OUTPUT: a Hex string representation of the new SHA256 userId.

            var r = new System.Random();
            string dataIn = "";
            string newUserId = "";

            // Example output of data input for SHA256 hash algorithm: "USER_152_2023-01-27T10h25m13s"
            dataIn += "USER_" + r.Next(10001);
            dataIn += "_" + DateTime.Now.ToString("s");

            // (Override the value if needed, for test purposes)
            // dataIn = "Test1";

            using (var sha256 = SHA256.Create())
            {
                byte[] dataHash = sha256.ComputeHash(Encoding.Default.GetBytes(dataIn));
                newUserId = ConvertToHexStr(dataHash);
            }

            return newUserId;
        }


        // Example snippets to test FileIO + new UserID generation
        void Start()
        {
            // ------------------
            // 1. Basic file Read
            // ------------------
            // ClasseGravar objCarregar = FileIOManager.LoadObjectFromDevice<ClasseGravar>();
            // if (objCarregar == null)
            // {
            // }
            // // ... 


            // ------------------
            // 2. Basic file Write
            // ------------------
            // SaveObjectToDevice(new UserData { userId = new System.Random().Next(16) });


            // ------------------
            // 3. Test generate new USER_ID
            // ------------------
            //Debug.Log(GenerateNewUserID());


            // ------------------
            // 4. Expected flow of program
            // ------------------

            // Try to load UserInfo file from local device storage.
            // Two possible outcomes exist:
            //
            // FAIL: If file reading fails (object = NULL), it is assumed that the file is missing
            //       (most likely, for being a 1st application execution), meaning that a new
            //       UserInfo instance must be generated, along with the generation of a new randomized USER_ID.
            //
            // SUCCESS: The data is retrieved from local storage, allowing the User to resume the treatment plan,
            //          knowing that any new Sessions will be associated with the existing USER_ID.

            var user = FileIOManager.LoadObjectFromDevice<UserInfo>("userData.json");

            if (user == null)
            {
                // File does not exist - create new User object with a new random User_ID
                Debug.Log("FILE MISSING: userData.json. A new one will be saved locally.");
                user = new UserInfo(GenerateNewUserID());
                FileIOManager.SaveObjectToDevice(user, "userData.json");
            }
            else
            {
                // File already exists - use the object during app runtime
                Debug.Log($"LOAD SUCCESS: userData.json. UserID = '{user.userId}'");
            }
        }
    }


// ----------------------------------------------
// TODO - place these comments where to handle: User loading & new ID generation if User unknown
// ----------------------------------------------
// INFO:
//  This function emulates the expected sequence of UserID local data load from the device.
//
//  It is expected that, on a first instance, an attempt is made to load existing User data.
//  -> if the load operation is successful, fetch and pull the data into memory, so that
//     the data can be used later to identify the USER completing the Exercises / Sessions.
//
//  -> if the load operation is NOT successful, at a prototype stage of development
//     it will be assumed that the file does NOT exist locally, meaning that a new 
//     User Data file must be stored on the device, along with a new, randomized USER_ID
//     generated and saved locally on the device running the application.
// ----------------------------------------------
}