using System;
using System.IO;
using UnityEngine;

using System.Security.Cryptography;
using System.Text;

public class TestDataScript : MonoBehaviour
{
    // TODO translate this whole file to English


    // ---------------------------------------------------------------------------------------------- //
    // Question1: what kind of local data persistence methods are most used?
    // ---------------------------------------------------------------------------------------------- //
    // Investigou-se um pouco o tema da gravação de dados.
    // Existiam já alguns nomes em mente sobre o tema: PlayerPrefs, Sqlite, JSON, ...
    //
    // *** PLAYERPREFS ***
    // Ao que tudo indica, PlayerPrefs NÃO É de todo algo recomendável, pois:
    // > Tem o limite de apenas 1MB para armazenar dados!
    // > Destina-se apenas para tipos simples de dados (strings, ints, floats, ...).
    // > O seu caso de uso típico é apenas em cenários para gravar, literalmente, player prefs (ex: volume do jogo).
    // > Ver uma discussão sobre o tema: https://www.reddit.com/r/Unity3D/comments/1fanu5/how_do_i_use_playerprefs_to_save_a_game_object/
    // 
    // - Vantagens das PlayerPrefs: simplicidade de implementação "out of the box".
    // - Desvantagens das PlayerPrefs: tamanho de dados limitado, suporte (Data Types) limitado, pouca segurança
    //
    // *** ALTERNATIVAS ***
    // Algumas alternativas encontradas foram:
    // 1) Gravar em ficheiro usando "JSON Serialization"
    // 2) Gravar em ficheiro Binário, "Binary serialization" 
    //    \ Este método aparenta oferecer complexidade excessiva para o que se pretende, e é error-prone;
    // 3) Ou então: SQLite
    //    \ Segue um padrão mais "relacional" para armazenar dados e similar a BDs.
    //    \ Poderá vir a ser solução interessante na temática da persistência de dados.
    //
    // Resumos obtidos (source: Internet)
    // -> For simple, small-scale (<= 1MB) data storage, low security, and simple data structures: PlayerPrefs.
    // -> For more extensive data management:
    //      >> JSON/Binary serialization (file storage);
    //      >> SQLite (databases);
    //      >> Cloud-based solutions (?).
    //
    // Possível leitura, também interessante:
    // https://www.reddit.com/r/Unity3D/comments/eakh1m/unity_tip_saving_data_with_json_and_playerprefs/
    // Possível vídeo interessante:
    // https://www.youtube.com/watch?v=uD7y4T4PVk0
    // ---------------------------------------------------------------------------------------------- //


    // ---------------------------------------------------------------------------------------------- //
    // Question2: when using local data persistence, what are good paths for local data storage?
    // ---------------------------------------------------------------------------------------------- //
    // Regarding paths for local data storage, an interesting read was taken from one of the above links: 
    //
    // "
    //   If you are already writing to json, it's pretty easy just to write to a file.
    //   Application.persistentDataPath will get you a location where unity has read and write access on any platform
    //   then you can use basic file IO commands to write the json to file.
    // "
    // ---------------------------------------------------------------------------------------------- //


    // *** Exemplo de implementação com JSON Serialization ***
    // 
    // NOTE1: Made with help of a few Google searches, by querying ways to save data locally on the user's device)
    //
    // NOTA2: Assume que existe um Filename pré-estabelecido, a usar para as Escritas e Leituras
    //        Aqui, utiliza o ficheiro apontado pela variável [jsonFilename].
    //
    const string jsonFilename = "saveData.json";


    // * Classe de exemplo para armazenar dados a gravar localmente.
    //   Apenas para testes.
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


    // **************************************************************************
    // v1 - limited to single-class data Load & Saving
    // **************************************************************************
    // private void TestarGravacao(ClasseGravar obj)
    // {
    //     // DEMO para gravação
    //
    //     // converter objeto em JSON
    //     string json = JsonUtility.ToJson(obj);
    //
    //     try
    //     {
    //         // " WriteAllText() creates a new file, writes the specified string to the file, and then closes the file.
    //         //   If the target file already exists, it is overwritten. "
    //         File.WriteAllText(Application.persistentDataPath + "/" + jsonFilename, json);
    //     }
    //     catch (Exception e)
    //     {
    //         Debug.Log("Exception while trying to save the file!! Message: " + e.Message);
    //     }
    // }

    // private ClasseGravar TestarCarregamento()
    // {
    //     // DEMO para carregamento do ficheiro gravado
    //
    //     // Load data
    //     ClasseGravar objetoCarregado = null;
    //     string path = Application.persistentDataPath + "/" + jsonFilename;
    //
    //     if (File.Exists(path))
    //     {
    //         string objJson = File.ReadAllText(path);
    //         objetoCarregado = JsonUtility.FromJson<ClasseGravar>(objJson);
    //
    //         // (From here on, use the loaded Object as necessary ...)
    //
    //         // TODO:
    //         // (?) Should object be NULL-Checked?
    //         // (?) If JsonUtility.FromJson cause exceptions, a TryCatch might be a good to use here...
    //         // - atenção ao retorno NULL de objetoCarregado
    //     }
    //
    //     // Nullable?
    //     return objetoCarregado;
    // }
    // **************************************************************************


    // **************************************************************************
    // v2 - An attempt to manage data Save/Load of any type, using Generics (<T>)
    // **************************************************************************
    private void SaveObjectToDevice<T>(T objectToSave)
    {
        // Saves an object into local file storage, depending on the platform's Persistent Data Directory.
        // The goal is to keep the data accessible between application runs. 
        // (For more info: https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html)
        // The object must be marked as [Serializable], so that it can be parsed into a JSON object.

        // TODO - an upgrade to allow choosing the filename as parameter to the function.

        // Serialize the object into JSON string
        string json = JsonUtility.ToJson(objectToSave);

        try
        {
            // NOTE: ".WriteAllText()" creates a new file, or overwrites if it already exists!
            string savePath = Path.Combine(Application.persistentDataPath, jsonFilename);
            File.WriteAllText(savePath, json);
        }
        catch (Exception e)
        {
            Debug.Log("An exception occurred while trying to save the file. Message: [" + e.Message + "]");
        }
    }

    private T LoadObjectFromDevice<T>()
    {
        // Attempts to load an Object from local file storage (JSON), according to the device's Persistent Data Directory.
        //  Follows the same rules as the SaveObject function, where the object to be fetched must be Serializable into JSON string -
        //  - which will most likely be, if it was saved successfully in first place. 
        // If, however, the file is not found or data loading fails (either because it never existed, or, somehow, got corrupted),
        //  it will be marked as NULL, informing the program that a new file should be (over)written.

        // TODO - an upgrade to allow finer choosing of what exactly to Load?

        // Load data
        T loadedObject = default(T);
        string path = Application.persistentDataPath + "/" + jsonFilename;

        if (File.Exists(path))
        {
            string objJson = File.ReadAllText(path);
            loadedObject = JsonUtility.FromJson<T>(objJson);

            // If data load was successful, the object will now be ready.
            // ( From here on, use as needed... )

            // TODO:
            // (?) Should object be NULL-Checked?
            // (?) If JsonUtility.FromJson cause exceptions, a TryCatch might be a good to use here...
            // - atenção ao retorno NULL de objetoCarregado
        }

        // Nullable?
        return loadedObject;
    }

    // private object LoadObjectFromDevice()
    // {
    //     // Attempts to load an Object from local file storage (JSON), according to the device's Persistent Data Directory.
    //     //  Follows the same rules as the SaveObject function, where the object to be fetched must be Serializable into JSON string -
    //     //  - which will most likely be, if it was saved successfully in first place. 
    //     // If, however, the file is not found or data loading fails (either because it never existed, or, somehow, got corrupted),
    //     //  it will be marked as NULL, informing the program that a new file should be (over)written.

    //     // TODO - an upgrade to allow finer choosing of what exactly to Load?

    //     // Load data
    //     object loadedObject = null;
    //     string path = Application.persistentDataPath + "/" + jsonFilename;

    //     if (File.Exists(path))
    //     {
    //         string objJson = File.ReadAllText(path);
    //         loadedObject = JsonUtility.FromJson<object>(objJson);

    //         // If data load was successful, the object will now be ready.
    //         // ( From here on, use as needed... )

    //         // TODO:
    //         // (?) Should object be NULL-Checked?
    //         // (?) If JsonUtility.FromJson cause exceptions, a TryCatch might be a good to use here...
    //         // - atenção ao retorno NULL de objetoCarregado
    //     }

    //     // Nullable?
    //     return loadedObject;
    // }
    // **************************************************************************

    // TODO review with RM

    private string ConvertToHexStr(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder(bytes.Length * 2);

        for (int i = 0; i < bytes.Length; i++)
        {
            sb.Append(bytes[i].ToString("x2"));  // Hex representation of this Byte
        }

        return sb.ToString();
    }

    private String GenerateNewUserID()
    {
        // Reference1: https://stackoverflow.com/questions/46194754/how-to-hex-encode-a-sha-256-hash
        // Reference2: https://dotnetfiddle.net/QbsKTc
        // - Uses a Random number between 0 and 10.000, and the current TimeStamp
        //   as data input for the random ID algorithm.
        // - The output is a SHA256 hash string representation of the data feed.

        System.Random r = new System.Random();
        String dataIn = "";
        String newUserId = "";

        // TODO acrescentar também o Timestamp atual aqui
        dataIn += "USER_" + r.Next(10001);

        // *TEST* Set a custom value if needed, for test purposes
        //dataIn = "test";

        using (var sha256 = SHA256Managed.Create())
        {
            byte[] dataHash = sha256.ComputeHash(Encoding.Default.GetBytes(dataIn));
            newUserId = ConvertToHexStr(dataHash);
        }

        return newUserId;
    }


    // Start is called before the first frame update
    void Start()
    {

        // ----------------------------------------------
        // *** Test generate new USER_ID ***
        //Debug.Log(GenerateNewUserID());
        // ----------------------------------------------


        //
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
        //

        try
        {
            // ********
            // Class #1
            // ********
            // Test Load
            // ClasseGravar objCarregar = LoadObjectFromDevice<ClasseGravar>();
            // if (objCarregar == null)
            // {
            //     Debug.Log("Oops... objeto carregado = NULL :(");

            //     // Test Save
            //     SaveObjectToDevice(new ClasseGravar { playerName = "RUI", playerScore = 15 });
            // }
            // else
            // {
            //     Debug.Log($"Dados do objeto carregados com sucesso! PlayerName=[{objCarregar.playerName}], PlayerScore=[{objCarregar.playerScore}]");
            // }

            // ********
            // Class #2
            // ********
            // Test Load
            UserData dados = LoadObjectFromDevice<UserData>();
            if (dados == null)
            {
                Debug.Log("Oops... objeto carregado = NULL :(");

                // Test Save
                SaveObjectToDevice(new UserData { userId = new System.Random().Next(16) });
            }
            else
            {
                Debug.Log($"Dados do objeto carregados com sucesso! UserID=[{dados.userId}]");
            }

        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        // ********************
        //   TEST OBJECT SAVE
        // ********************
        //ClasseGravar objGravar = new ClasseGravar();
        //objGravar.playerName = "RUI";
        //objGravar.playerScore = 13;
        //TestarGravacao(objGravar);

        // ********************
        //   TEST OBJECT LOAD
        // ********************
        //ClasseGravar objCarregar = TestarCarregamento();
        //if (objCarregar == null) Debug.Log("Oops... objeto carregado = NULL :(");
        //else Debug.Log($"Dados do objeto carregados com sucesso! PlayerName=[{objCarregar.playerName}], PlayerScore=[{objCarregar.playerScore}]");

    }

}
