using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    public interface IShootable
    {
        public void Fire(Vector3 aimVector);
    }
}

