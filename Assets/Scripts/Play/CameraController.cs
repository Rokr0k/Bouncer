using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bouncer
{
    namespace Play
    {
        public class CameraController : MonoBehaviour
        {
            public float x1, x2;

            void Update()
            {
                transform.position = Vector3.Lerp(transform.position, new Vector3((x1 + x2) / 2, (x2 - x1) * 0.3125f - 5, (x1 - x2) * 0.625f), Time.deltaTime * 2f);
            }
        }
    }
}