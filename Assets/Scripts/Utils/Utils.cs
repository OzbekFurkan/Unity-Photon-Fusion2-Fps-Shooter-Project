using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilitiy
{

    public static class Utility
    {
        public static Vector3 GetRandomSpawnPoint()
        {
            return new Vector3(Random.Range(-20, 20), 4, Random.Range(-20, 20));
        }

        public static void SetRenderLayerInChildren(Transform transform, int layerNumber)
        {
            foreach (Transform trans in transform.GetComponentsInChildren<Transform>(true))
            {
                if (trans.CompareTag("IgnoreLayerChange"))
                    continue;

                trans.gameObject.layer = layerNumber;
            }
        }

        public static Vector3 GetWeaponPositon()
        {
            return new Vector3(-0.1f, -0.54f, 0.53f);
        }

    }

}

