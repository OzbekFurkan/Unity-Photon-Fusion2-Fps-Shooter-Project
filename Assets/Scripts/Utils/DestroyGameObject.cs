using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilitiy
{
    public class DestroyGameObject : MonoBehaviour
    {
        public float lifeTime = 1.5f;

        // Start is called before the first frame update
        IEnumerator Start()
        {
            yield return new WaitForSeconds(lifeTime);

            Destroy(gameObject);
        }
    }
}