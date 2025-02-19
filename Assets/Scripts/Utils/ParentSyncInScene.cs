using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Utilitiy
{
    /// <summary>
    /// This empty class is used to sync parent of dropped items, otherwise it might crash.
    /// Attach it to empty gameobject in the scene. All Interractable items will be its childs.
    /// </summary>
    public class ParentSyncInScene : NetworkBehaviour
    {
        
    }
}