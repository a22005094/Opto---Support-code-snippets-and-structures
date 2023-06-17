using Assets.Models.Managers;
using System.Threading.Tasks;
using UnityEngine;

public class ManagerScript : MonoBehaviour
{
    // **********
    // *  TEST  *
    // * SCRIPT *
    // **********
    // - This is "TFC Tests" project's main script, and it's being used to test Elasticsearch and Kibana.
    // - Example exercises and sessions are being generated ad-hoc or randomly, to debug service functionality.
    //


    // * 1. Create index on Elasticsearch
    // async void Start()
    // {
    //     var manager = DataManager.Instance;
    //     await Task.Run(() => manager.CreateIndexSessions());
    //     await Task.Delay(5000);
    //     await Task.Run(() => manager.CreateIndexExercises());
    // }


    // * 2. Generate N Sessions with random data
    // async void Start()
    // {
    //     var manager = DataManager.Instance;
    //
    //     // Create Indices
    //     await Task.Run(() => manager.CreateIndexSessions());
    //     await Task.Run(() => manager.CreateIndexExercises());
    //
    //     int numberOfSessions = 5;
    //
    //     // *** Version 1 - generates N random Sessions ***
    //     for (int i = 1; i <= numberOfSessions; i++)
    //     {
    //         // TODO Uncomment
    //         //manager.GenerateRandomSession();
    //         //Debug.Log("Sent session #" + i);
    //         await Task.Delay(500);
    //     }
    //
    //     // // *** Version 2 - generates 1 random Session ***
    //     // manager.GenerateSpecificSession();
    //     // Debug.Log("Sent session!!");
    // }


    // * 3. Example usage sequence
    // async void Start()
    // {
    //     // ** Exemplos de utilizacao da classe gestora **
    //
    //     // Load DataManager singleton instance
    //     var manager = DataManager.Instance;
    //
    //     // Session Start - example
    //     string device_id = "DEVICE_1";
    //     string user_id = "USER_1";
    //     manager.Start_Session(device_id, user_id);
    //
    //     // Exercise example
    //     string challenge_id = "EXERCICIO_OBJETOS_ESTATICOS_1";
    //     short duration = 15;    // segundos
    //     short score = 50;       // TBD quantificacao de valores para Scores
    //     manager.Start_Exercise(challenge_id);
    //     // ... ( Time passed meanwhile... user completing the exercise ) ...
    //     manager.Complete_Exercise(duration, score);
    //
    //     // Testing delay...
    //     await Task.Delay(10000);
    //
    //     // Exercise example 2
    //     manager.Start_Exercise("EXERCICIO_2");
    //     duration = 30;
    //     score = 100;
    //     // ... ( Time passed meanwhile... user completing the exercise ) ...
    //     manager.Complete_Exercise(duration, score);
    //
    //     // Testing delay...
    //     await Task.Delay(10000);
    //
    //     // Session End - example
    //     manager.Complete_Session();
    // }
}