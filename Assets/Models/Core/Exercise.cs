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

        // TODO:
        // - reassess visibility rules in attributes (private set Duration, private set Score, private set StartAt, private set IsSaved, ...)

        // * Attributes
        public string ChallengeId;              // Family/Type of exercise presented
        public short DurationSeconds;      // Time elapsed to complete the exercise        
        public short Score;                // Exercise completion score
        public DateTime StartAt;           // Timestamp when this exercise was completed.

        //
        // TODO check if this field can be still ignored from sending to Elasticsearch if Property is refactored to Attribute
        //
        [Ignore]                                        // [Ignore] means this field is not mapped to Elasticsearch.
        public bool IsSaved { get; private set; }       // Indicates if this object has been saved to ElasticSearch.


        // * Constructor(s)
        public Exercise(string challId)
        {
            // Set here the parameters that are known from the beginning of an Exercise
            ChallengeId = challId;
        }


        // * Methods
        public void Start() => StartAt = DateTime.Now;     // simple setter to mark the current date&time when an exercise begins.

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
