using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interract
{
    public interface IHandAttachable
    {
        /// <summary>
        /// This is used for attaching left hand to item's left hand object. 
        /// A Transform variable must be assigned with the transform of the LeftHandTransform empty gameobject's
        /// transform which is child of the item that implements this interface.
        /// </summary>
        /// <returns>Returns the tranform object of the left hand</returns>
        public abstract Transform GetLeftHandTransform();

        /// <summary>
        /// This is used for attaching right hand to item's left hand object. 
        /// A Transform variable must be assigned with the transform of the RightHandTransform empty gameobject's
        /// transform which is child of the item that implements this interface.
        /// </summary>
        /// <returns>Returns the tranform object of the right hand</returns>
        public abstract Transform GetRightHandTransform();
    }
}

