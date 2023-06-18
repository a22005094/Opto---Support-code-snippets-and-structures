using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nest;
using UnityEngine;

namespace Assets.Models.Managers
{
    // Reference for this Singleton object implementation:
    // https://forum.unity.com/threads/singleton-vs-static.197169/

    public sealed class DataManager
    {
        // Singleton instance to this manager
        private static DataManager _instance = null; // the instance itself.

        // Lock object is being used to help ensure thread safety during singleton initialization
        private static readonly object _lockObj = new object();

        // * ES singleton manager allows data pushing (Sessions, Exercises, ...) to the Elasticsearch back-end Server
        private readonly ESManager _esManager;


        // ********** Application data **********

        // * User information
        private UserInfo _currentUser;

        // * Sessions - current & completed
        private Session _currentSession;
        private List<Session> _completedSessions;

        // * Exercises - current & completed
        private Exercise _currentExercise;
        private List<Exercise> _completedExercises;

        // **************************************


        // * Getter of this Singleton instance.
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


        // * Constructor. Being [private] ensures this Class
        // cannot be instantiated outside of its own scope (allowing Singleton implementation)
        private DataManager()
        {
            // * User data
            // Attempt to load User Data object from local device storage.
            //
            // 2 possible outcomes exist:
            //
            // > FAIL: If file reading fails (object = NULL), it is assumed that the User file is missing
            //         (most likely on a 1st application execution), meaning that a new
            //         UserInfo instance must be generated, along with the generation of a new randomized USER_ID.
            //
            // > OK:   The data is retrieved from local storage, allowing the User to resume the treatment plan,
            //         knowing that any new Sessions will be associated with the existing USER_ID.

            // If it doesn't exist, a new User object will be generated, along with a new USER_ID.
            _currentUser = FileIOManager.LoadObjectFromDevice<UserInfo>(FileKind.User);
            if (_currentUser == null)
            {
                // File does not exist - create new User object with a new random User_ID
                Debug.Log("FILE MISSING: userData.json. A new one will be saved locally.");
                _currentUser = new UserInfo(GenerateNewUserID());
                FileIOManager.SaveObjectToDevice(_currentUser, FileKind.User);
            }
            else
            {
                // ( TODO remove this block )
                // File exists! User object loaded into memory.
                Debug.Log($"LOAD SUCCESS: userData.json. UserID = '{_currentUser.userId}'");
            }

            // * Sessions & Exercises
            // Reset attributes and await for data insertion
            _currentSession = null; // awaiting Session start...
            _currentExercise = null; // awaiting Exercise start...
            _completedSessions = new List<Session>();
            _completedExercises = new List<Exercise>();

            // Initialize/Fetch singleton instance for ElasticSearch manager
            _esManager = ESManager.Instance;
        }


        //    UTILITY     //
        // * MANAGEMENT * //
        //    METHODS     //

        private string ConvertToHexStr(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);

            foreach (var currByte in bytes)
            {
                // Hex representation of this Byte, in lowercase if alphabetic
                sb.Append(currByte.ToString("x2"));
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

        // public string getUserId() => _currentUser.userId;


        //      DATA      //
        // * MANAGEMENT * //
        //    METHODS     //

        // **************************************
        //   ElasticSearch MANAGEMENT FUNCTIONS
        // **************************************

        public void CreateIndexSessions()
        {
            var s = new Session(_currentUser.userId)
            {
                StartAt = new DateTime(2023, 5, 16, 9, 30, 00),
                EndAt = new DateTime(2023, 5, 16, 11, 30, 00),
                Exercises = GenerateRandomExercises()
            };

            _esManager.CreateIndexFromObject("sessions", s);
        }

        public void CreateIndexExercises()
        {
            var e = new Exercise("CH_1")
            {
                DurationSeconds = 10,
                Score = 25,
                StartAt = DateTime.Now
            };

            _esManager.CreateIndexFromObject("exercises", e);
        }

        // *******************************
        //   EXERCISES - DEBUG FUNCTIONS
        // *******************************

        private List<Exercise> GenerateRandomExercises()
        {
            var r = new System.Random();
            var lstEx = new List<Exercise>();

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


        // ******************************
        //   SESSIONS - DEBUG FUNCTIONS
        // ******************************

        public async void GenerateAdHocSession()
        {
            // Pushes a specific, on-demand Session for debugging purposes.
            var dtStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 9, 30, 0);
            var dtEnd = dtStart.AddMinutes(30);

            var s = new Session(_currentUser.userId);
            s.StartAt = dtStart;
            s.EndAt = dtEnd;

            // (note: exercises will have random timestamps here, different from the session!)
            s.Exercises = GenerateRandomExercises();

            _currentSession = s;
            await Save_Session_To_Elasticsearch();
        }

        public async void GenerateRandomSession()
        {
            // Pushes a "randomized" Session for debugging purposes.
            // > Date - random, between [last 3 months] and [yesterday].
            // > Time - also random, depending on the random TimeDiff obtained.
            // > Duration - random value between [15-45] minutes.

            var rand = new System.Random();
            var dtStart = DateTime.Now.AddMonths(-3);
            var dtEnd = DateTime.Today.AddDays(-1);

            var s = new Session(_currentUser.userId);
            // (note: Exercises will have random timestamps here, different from the session)
            s.Exercises = GenerateRandomExercises();

            // Generate new random Date and Time.
            // Based from: https://stackoverflow.com/questions/1483670/whats-the-best-practice-for-getting-a-random-datetime-between-two-date-times
            // > Get the timespan elapsed between the intended timeframe (Start and End dates)
            TimeSpan tsDiff = dtEnd - dtStart;
            // > Generate random time elapsed between both dates
            TimeSpan tsRandom = new TimeSpan(0, rand.Next(0, (int)tsDiff.TotalMinutes), 0);
            // > Add the random timespan to the Start date             
            s.StartAt = dtStart + tsRandom;

            // > Generate a random start time, according to Weekday
            //   Test scenario:
            //    >> Mon - Fri : between 18 o'clock and 20 o'clock
            //    >> Sat & Sun : between 10 o'clock and 12 o'clock
            var sessionDay = s.StartAt.DayOfWeek;

            if (sessionDay.Equals(DayOfWeek.Saturday) || sessionDay.Equals(DayOfWeek.Sunday))
            {
                // Sat / Sunday
                s.StartAt = new DateTime(s.StartAt.Date.Year, s.StartAt.Date.Month, s.StartAt.Date.Day,
                    rand.Next(10, 13), 0, 0);
            }
            else
            {
                // Mon - Fri
                s.StartAt = new DateTime(s.StartAt.Date.Year, s.StartAt.Date.Month, s.StartAt.Date.Day,
                    rand.Next(18, 21), 0, 0);
            }

            // > Generate a random duration: add base 15 minutes, then again a random time between 0m - 30m
            s.EndAt = s.StartAt.AddMinutes(15).AddMinutes(rand.Next(0, 31));

            _currentSession = s;
            await Save_Session_To_Elasticsearch();
        }

        // --- End of Debug functions ---

        // * Internal ES communication functions
        private async Task<bool> Save_Session_To_Elasticsearch()
        {
            // TODO review
            bool result = await _esManager.Push_Session_To_Elasticsearch(_currentSession);
            return result;
        }

        private async Task<bool> Save_Exercise_To_Elasticsearch()
        {
            // TODO review
            bool result = await _esManager.Push_Exercise_To_Elasticsearch(_currentExercise);
            return result;
        }

        // ----------------------------
        // * Publicly available methods
        // ----------------------------

        // Sessions
        public void Start_Session()
        {
            // Assert that a UserId exists
            if (string.IsNullOrEmpty(_currentUser.userId))
                throw new Exception("ERROR - could not start a Session because a valid UserID has not been supplied.");

            _currentSession = new Session(_currentUser.userId);
            _currentSession.Start();
        }

        public void Complete_Session()
        {
            // Completing a Session means:
            //  > filling relevant Session data (e.g.: relevant timestamps);
            //  > pushing Session object data to Elasticsearch.

            // This operation will run in background (asynchronously), allowing the app
            // to continue its normal flow of execution without blocking the UI in the Main Thread,
            // while the Session is completed & sent to Elasticsearch server.

            if (_currentSession != null)
            {
                Task.Run
                (
                    async () =>
                    {
                        _currentSession.Complete();

                        // Attempt to send the Session data to Elasticsearch backend server.
                        if (await Save_Session_To_Elasticsearch())
                        {
                            // - If successful, marks the object as Saved (to the Server).

                            // - If it fails, another routine is expected to look for pending objects later,
                            //   and retrying the send operation.

                            _currentSession.Mark_As_Saved();
                        }

                        // Add current Session to the List of completed Sessions
                        _completedSessions.Add(_currentSession);

                        //
                        // TODO - Retest if nullifying the variable is not causing unintended secondary effects
                        //
                        _currentSession = null;
                    }
                );
            }
        }

        // Exercises

        public void Start_Exercise(string challengeId)
        {
            // Validate input
            if (string.IsNullOrEmpty(challengeId))
                throw new ArgumentException("Invalid ChallengeID was supplied into Manager's Start_Exercise() method.");

            _currentExercise = new Exercise(challengeId);
            _currentExercise.Start();
        }

        public void Complete_Exercise(short exerciseDuration, short exerciseScore)
        {
            // Completing an Exercise means:
            //  > filling Exercise object with results (e.g. Score, Duration, Timestamps, ...);
            //  > pushing Exercise object data to Elasticsearch.

            // Similarly to Sessions, this operation will run in background (asynchronously), allowing the app
            // to continue its normal flow of execution without blocking the UI in the Main Thread,
            // while the Exercise is completed & sent to Elasticsearch server.

            if (_currentExercise != null)
            {
                Task.Run
                (
                    async () =>
                    {
                        _currentExercise.Complete(exerciseDuration, exerciseScore);

                        // Attempt to send the Exercise data to Elasticsearch backend server.
                        if (await Save_Exercise_To_Elasticsearch())
                        {
                            // - If successful, marks the object as Saved (to the Server).

                            // - If it fails, another routine is expected to look for pending objects later,
                            //   and retrying the send operation.

                            _currentExercise.Mark_As_Saved();

                            // (DEBUG)
                            Debug.Log($"Sent Exercise data: {_currentExercise}");
                        }

                        // Add this completed exercise to current Session's List of completed Exercises
                        _currentSession?.Exercises.Add(_currentExercise);

                        // Add this completed exercise to the standalone List of completed Exercises
                        _completedExercises.Add(_currentExercise);

                        //
                        // TODO - Retest if nullifying the variable is not causing unintended secondary effects
                        //
                        _currentExercise = null;
                    }
                );
            }
        }
    }
}