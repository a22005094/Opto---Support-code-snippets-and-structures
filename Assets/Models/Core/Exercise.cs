using System;
using Nest;

namespace Assets.Models
{
    public class Exercise
    {
        // * INFO *
        // This class represents an Exercise done during a Treatment Session.
        // It is responsible for identifying the kind of Exercise being done, and saving
        // its own completion results/metrics (time elapsed, score, etc.)


        // * Attributes
        // All were kept Public to maintain full support with NEST elasticsearch client.
        public string ChallengeId { get; } // Family/Type of exercise presented
        public short DurationSeconds { get; set; } // Time elapsed to complete the exercise        
        public short Score { get; set; } // Exercise completion score
        public DateTime StartAt { get; set; } // Timestamp when this exercise was completed.

        [Ignore] // [Ignore] means this field will not be mapped to Elasticsearch.
        public bool IsSaved { get; set; } // Indicates if this object has been saved to ElasticSearch.


        // * Constructor(s)
        public Exercise(string challengeId)
        {
            // Set here the parameters that are known from the beginning of an Exercise
            ChallengeId = challengeId;
        }


        // * Methods
        public void Start() =>
            StartAt = DateTime.Now; // simple setter to mark the current date&time when an exercise begins.

        public void Complete(short duration, short finalScore)
        {
            // Fill exercise results fields
            DurationSeconds = duration;
            Score = finalScore;
        }

        public void Mark_As_Saved() => IsSaved = true;


        // * Output object data
        public override string ToString()
        {
            return $"[ ChallengeId = {ChallengeId} | Duration (s) = {DurationSeconds} |"
                   + $" Score = {Score} | StartAt = {StartAt.ToString("dd/MM/yyyy HH:mm:ss")} | Saved = {IsSaved}]";
        }
    }
}