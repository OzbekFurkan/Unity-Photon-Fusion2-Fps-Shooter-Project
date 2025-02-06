using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    /// <summary>All the items that can be held should implement this interface and follow the instructions 
    /// so we can have target points of both hands with this single interface references in any script of player.</summary>
    public interface IHandAttachable
    {
        /// <summary>The gameobject transform that refers to the point where player's left hand should be placed.
        /// You can access it from ItemReferenceGetter script in Awake</summary>
        public Transform leftHandTarget { get; set; }
        /// <summary>The gameobject transform that refers to the point where player's right hand should be placed.
        /// You can access it from ItemReferenceGetter script in Awake</summary>
        public Transform rightHandTarget { get; set; }

        /// <returns>Returns the tranform object of the left hand target.</returns>
        public abstract Transform GetLeftHandTarget();

        /// <returns>Returns the tranform object of the right hand target.</returns>
        public abstract Transform GetRightHandTarget();
    }
}

