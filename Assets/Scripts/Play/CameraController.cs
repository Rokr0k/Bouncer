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
                Vector3 origin = Vector3.back * (x2 - x1) * 0.625f;
                Vector3 target = Vector3.right * (x1 + x2) / 2;
                transform.position = Vector3.Lerp(transform.position, origin + target, Time.deltaTime * 2f);
            }
        }
    }
}