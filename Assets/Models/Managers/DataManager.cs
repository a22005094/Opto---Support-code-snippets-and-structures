using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Models.Managers
{
    // Reference for this Singleton object implementation:
    // https://forum.unity.com/threads/singleton-vs-static.197169/

    public sealed class DataManager
    {
        // * Variables for Singleton management
        private static DataManager _instance = null;              // the instance itself.
        private static readonly object _lockObj = new object();   // lock to help ensure thread safety during singleton initialization

        // * Object to manage data pushing to Elasticsearch (Sessions, Exercises, ...)
        private ElasticsearchManager esManager { get; set; }

        // * Manage current & completed Sessions
        private Session currentSession;
        private List<Session> completedSessions;

        // * Manage current & completed Exercises
        private Exercise currentExercise;
        private List<Exercise> completedExercises;

        // * The getter of this Singleton instance.
        public static DataManager Instance
        {
            get
            {
                lock (_lockObj)
                {
                    if (_instance == null)
                    {
                        _instance = new DataManager();
                    }

                    return _instance;
                }

            }
        }

        // Private constructor ensures this Class cannot be instantiated outside of its own scope
        private DataManager()
        {
            // ** Manager initialization **
            // (Add more logic here as necessary...)

            // Clear attributes - allow data insertion
            currentSession = null;           // awaiting Session  start...
            currentExercise = null;          // awaiting Exercise start...
            completedSessions = new List<Session>();
            completedExercises = new List<Exercise>();

            // Initialize Elasticsearch NEST client
            esManager = ElasticsearchManager.Instance;
        }

        //                        //
        // * MANAGEMENT METHODS * //
        //                        //

        // ** [Elasticsearch Management] **

        // ** DEBUG
        public async void GenerateSpecificSession()
        {
            // Pushes a specific timeframe session to Elasticsearch, for debug purposes

            System.Random r = new System.Random();
            DateTime dtStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 9, 0, 0);
            DateTime dtEnd = dtStart.AddMinutes(45);

            Session s = new Session("DEVICE_" + r.Next(10000), "USER_" + r.Next(10000));
            s.StartAt = dtStart;
            s.EndAt = dtEnd;

            // Exercises will have weird, random timestamps here, different from the session, but currently that doesn't matter here
            s.Exercises = getRandomExercises();

            currentSession = s;
            await Save_Session_To_Elasticsearch();
        }

        // ** DEBUG
        public async void GenerateRandomSession()
        {
            // Pushes a randomized session to Elasticsearch, for debug purposes

            System.Random r = new System.Random();
            DateTime dtStart = new DateTime(2023, 03, 01);
            DateTime dtEnd = DateTime.Today.AddDays(-1);

            Session s = new Session("DEVICE_" + r.Next(100), "USER_" + r.Next(100));

            // Generate new date - seen @ https://stackoverflow.com/questions/1483670/whats-the-best-practice-for-getting-a-random-datetime-between-two-date-times
            TimeSpan timeSpan = dtEnd - dtStart;
            TimeSpan newSpan = new TimeSpan(0, r.Next(0, (int)timeSpan.TotalMinutes), 0);

            // This will be the randomly-generated start date             
            s.StartAt = dtStart + newSpan;
            // To generate a random duration, add a base 15m then a random time between 0 and 30
            s.EndAt = s.StartAt.AddMinutes(15).AddMinutes(r.Next(0, 31));

            // Exercises will have weird, random timestamps here, different from the session, but currently that doesn't matter here
            s.Exercises = getRandomExercises();

            //
            // OVERRIDE: define random Start and End times, depending on the week day
            if (s.StartAt.DayOfWeek.Equals(DayOfWeek.Saturday)
                || s.StartAt.DayOfWeek.Equals(DayOfWeek.Sunday))
            {
                // fim de semana
                s.StartAt = new DateTime(s.StartAt.Date.Year, s.StartAt.Date.Month, s.StartAt.Date.Day, r.Next(10, 13), 0, 0);
                s.EndAt = s.StartAt.AddMinutes(15).AddMinutes(r.Next(0, 31));
            }
            else
            {
                // dia da semana
                s.StartAt = new DateTime(s.StartAt.Date.Year, s.StartAt.Date.Month, s.StartAt.Date.Day, r.Next(18, 21), 0, 0);
                s.EndAt = s.StartAt.AddMinutes(15).AddMinutes(r.Next(0, 31));
            }
            //

            currentSession = s;
            await Save_Session_To_Elasticsearch();
        }

        // ** DEBUG
        private List<Exercise> getRandomExercises()
        {
            System.Random r = new System.Random();
            List<Exercise> lstEx = new List<Exercise>();

            for (int i = 0; i < r.Next(1, 6); i++)
            {
                Exercise e = new Exercise("CHALLENGE_" + r.Next(26));
                e.StartAt = DateTime.Now;
                e.DurationSeconds = (short)r.Next(75);
                e.Score = (short)r.Next(11);
                e.StartAt = DateTime.Now;

                lstEx.Add(e);
            }
            return lstEx;
        }

        // ** DEBUG
        public void CreateIndexSessions()
        {
            Session s = new Session("DEVICE_1", "USER_1");
            s.StartAt = new DateTime(2023, 5, 16, 9, 30, 00);
            s.EndAt = new DateTime(2023, 5, 16, 11, 30, 00);
            s.Exercises = getRandomExercises();

            esManager.CreateIndexFromObject("sessions", s);
        }

        // ** DEBUG
        public void CreateIndexExercises()
        {
            Exercise e = new Exercise("CH_1");
            e.DurationSeconds = 10;
            e.Score = 25;
            e.StartAt = DateTime.Now;

            esManager.CreateIndexFromObject("exercises", e);
        }

        private async Task<bool> Save_Session_To_Elasticsearch()
        {
            // ** TODO **
            bool result = await esManager.Push_Session_To_Elasticsearch(currentSession);
            return result;
        }

        private async Task<bool> Save_Exercise_To_Elasticsearch()
        {
            // ** TODO **
            bool result = await esManager.Push_Exercise_To_Elasticsearch(currentExercise);
            return result;
        }


        // ** [SESSION MANAGEMENT] **

        public void Start_Session(string device_id, string user_id)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(device_id))
            {
                throw new ArgumentException("Invalid DeviceID was supplied into Manager's Start_Session() method.");
            }
            if (string.IsNullOrEmpty(user_id))
            {
                throw new ArgumentException("Invalid UserID was supplied into Manager's Start_Session() method.");
            }

            currentSession = new Session(device_id, user_id);
            currentSession.Start();
        }

        public void Complete_Session()
        {
            // Completing a session means:
            // - filling relevant Session data (e.g. timestamps of Start & End)
            // - pushing object data to Elasticsearch.

            Task.Run(
                async () =>
                {
                    if (currentSession != null)
                    {
                        currentSession.Complete();

                        if (await Save_Session_To_Elasticsearch())
                        {
                            currentSession.Mark_As_Saved();
                        }

                        // Add this Session to Completed Sessions list
                        completedSessions.Add(currentSession);

                        // TODO - assert that clearing this variable does not cause issues (variable currentSession VS sessions List references)
                        currentSession = null;
                    }
                }
            );
        }

        // ** [EXERCISE MANAGEMENT] **

        public void Start_Exercise(string challengeId)
        {
            // Validate input
            if (string.IsNullOrEmpty(challengeId))
            {
                throw new ArgumentException("Invalid ChallengeID was supplied into Manager's Start_Exercise() method.");
            }

            currentExercise = new Exercise(challengeId);
            currentExercise.Start();
        }

        public void Complete_Exercise(short exerciseDuration, short exerciseScore)
        {
            // Completing an exercise means:
            // - filling Exercise object with results (e.g. score, duration, timestamps, ...)
            // - pushing object data to Elasticsearch.

            Task.Run(
                async () =>
                {
                    if (currentExercise != null)
                    {
                        currentExercise.Complete(exerciseDuration, exerciseScore);

                        if (await Save_Exercise_To_Elasticsearch())
                        {
                            currentExercise.Mark_As_Saved();

                            // *** DEBUG ***
                            Debug.Log($"Sent Exercise data: {currentExercise}");
                        }
                        else
                        {
                            // *** DEBUG ***
                            Debug.Log($"Failed to save Exercise data: {currentExercise}");
                        }

                        // Add this completed exercise to current Session's list of exercises
                        currentSession?.Exercises.Add(currentExercise);

                        completedExercises.Add(currentExercise);

                        // *** DEBUG ***
                        // TODO - assert that clearing this variable does not cause issues (variable currentExercise VS exercise List references)
                        currentExercise = null;
                    }
                }
            );
        }


    }
}
