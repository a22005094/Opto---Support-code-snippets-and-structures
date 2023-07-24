using System;
using Assets.Models.Managers;
using System.Threading.Tasks;
using UnityEngine;

public class ManagerScript : MonoBehaviour
{
    // ****************
    // *  TEST SCRIPT *
    // ****************
    // - This is "TFC Tests" project's main script, and it's being used to test Elasticsearch and Kibana.
    // - Example exercises and sessions are being generated ad-hoc or randomly, to debug service functionality.

    // ----------------------------------
    // * 1. Create new index on Elasticsearch
    // ----------------------------------
    // async void Start()
    // {
    //     Debug.Log("Generating indices...");
    //     var manager = DataManager.Instance;
    //     await Task.Run(() => manager.CreateIndexSessions());
    //     await Task.Delay(2500);
    //     // await Task.Run(() => manager.CreateIndexExercises());
    // }

    // ----------------------------------
    // * 2. Generate N Sessions with random data
    // ----------------------------------
    async void Start()
    {
        var manager = DataManager.Instance;

        // // Create [Sessions] + [Exercises] Indices (if they don't exist)
        // await Task.Run(() => manager.CreateIndexSessions());
        // await Task.Run(() => manager.CreateIndexExercises());

        // -----------------
        // ** Define here the # of random Sessions to generate ** 
        int nrSessions = 74;
        // -----------------

        // * Generate [nrSessions] Sessions *
        for (int i = 1; i <= nrSessions; i++)
        {
            Debug.Log("Sending session #" + i);
            manager.GenerateRandomSession();

            // Small delay between operations
            await Task.Delay(500);
        }

        // * (OLD) Generate 1 specific Session *
        // manager.GenerateSpecificSession();
    }

    // ----------------------------------
    // * 3. Example Use case flow
    // ----------------------------------
    // async void Start()
    // {
    //     // Load DataManager singleton instance
    //     var manager = DataManager.Instance;
    //
    //     // (EXAMPLE) Session Start
    //     manager.Start_Session();
    //
    //     // (EXAMPLE) Exercise flow 1
    //     // Initial exercise start
    //     string challengeId = "EXERCICIO_OBJETOS_ESTATICOS_1";
    //     manager.Start_Exercise(challengeId);
    //
    //     // Metrics are then stored...
    //     short duration = 15;
    //     short score = 50;
    //
    //     // Complete exercise
    //     manager.Complete_Exercise(duration, score);
    //
    //     //// * Testing a delay *
    //     // await Task.Delay(5000);
    //
    //     // (EXAMPLE) Exercise flow 2
    //     challengeId = "EXERCICIO_2";
    //     manager.Start_Exercise(challengeId);
    //     // ...
    //     duration = 65;
    //     score = 13;
    //     manager.Complete_Exercise(duration, score);
    //
    //     //// * Testing a delay *
    //     // await Task.Delay(5000);
    //
    //     // (EXAMPLE) Session completed
    //     manager.Complete_Session();
    // }

    // ----------------------------------
    // * X. Delete index from Elasticsearch
    // ----------------------------------
    // NOTE: This method DELETES ALL DATA along with the Index! Use only when strictly necessary.
    // private void Start()
    // {
    //     var esManager = ESManager.Instance;
    //     //esManager.DeleteIndex(ESManager.EXERCISES_INDEX);
    //     esManager.DeleteIndex(ESManager.SESSIONS_INDEX);
    // }

    //
}