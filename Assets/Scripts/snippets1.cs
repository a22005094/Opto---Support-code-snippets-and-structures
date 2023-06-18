using UnityEngine;

namespace Assets.Scripts
{
    public class snippets1 : MonoBehaviour
    {
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
            // (...)

            // ------------------
            // 2. Basic file Write
            // ------------------
            // SaveObjectToDevice(new UserData { userId = new System.Random().Next(16) });
            // (...)

            // ------------------
            // 3. Test generate new USER_ID
            // ------------------
            // Debug.Log(GenerateNewUserID());
            // (...)

            // ------------------
            // 4. Example flow of User fetch
            // ------------------
            // (** NOTE - already moved to DataManager Constructor **)

            // placeholder
            return;
        }
    }


// ----------------------------------------------
// TODO (?) - place these comments where to handle: User loading / new UserID generation if User unknown
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