using System;
using UnityEngine.Serialization;

namespace Assets.Models
{
    [Serializable]
    public class UserInfo
    {
        // * INFO *
        // - This class contains User-related information, such as fields to uniquely identify
        //     the User among others, and other types of information that should be tied.
        // - Objects will be stored locally on the User's device, allowing persistence of identification and 
        //     relevant parameters between application runs. This is crucial to identify who is performing
        //     the sessions during treatment plan.

        public string userId;             // userId running the application



        // TODO - 1 or 2 ? Review if this field can be initially NULL or must always be directly declared with the object
        // 1.
        // public UserInfo() => userId = "";
        //
        // 2.
        public UserInfo(string newUserId)
        {
            // The userId must always be known at the moment of object instantiation.
            this.userId = newUserId;
        }
    }
}