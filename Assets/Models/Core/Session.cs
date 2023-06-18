using System;
using Nest;
using System.Collections.Generic;

namespace Assets.Models
{
    public class Session
    {
        // * INFO *
        // This class represents a Treatment Session, by storing important relevant data:
        // Who did it, where and when it was done, and what exercises were done during the Session.
        // Session times will be calculated from Start & End timestamps.


        // * Attributes
        // All were kept Public to maintain full support with NEST elasticsearch client.
        public string UserId { get; } // UserID this session belongs to
        public DateTime StartAt { get; set; } // Timestamp beginning of session
        public DateTime EndAt { get; set; } // Timestamp end of session

        [Nested] // [Nested] will output each exercise here as its own object
        public List<Exercise> Exercises { get; set; } // List of exercises completed during this Session

        [Ignore] // [Ignore] means this field will not be mapped to Elasticsearch.
        public bool IsSaved { get; set; } // Indicates if this object has been saved to ElasticSearch.


        // * Constructor(s)
        public Session(string userId)
        {
            // These parameters can be set from the start, since they are already known at the beginning of a Session
            UserId = userId;
            Exercises = new List<Exercise>();
        }


        // * Methods
        public void Start() => StartAt = DateTime.Now;

        public void Complete() => EndAt = DateTime.Now;

        public void Mark_As_Saved() => IsSaved = true;


        // * Output object data
        private string Print_Exercises()
        {
            if (Exercises == null || Exercises.Count == 0) return "";
            return string.Join(", ", Exercises);
        }

        public override string ToString()
        {
            return
                $"[SESSION | UserID = {UserId} | "
                + $"StartAt = {StartAt.ToString("dd/MM/yyyy HH:mm:ss")} | EndAt = {EndAt.ToString("dd/MM/yyyy HH:mm:ss")} | "
                + $"Saved = {IsSaved} | Exercises: {{ {Print_Exercises()} }}";
        }
    }
}