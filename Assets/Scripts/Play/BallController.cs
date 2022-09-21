using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bouncer
{
    namespace Play
    {
        public class BallController : MonoBehaviour
        {
            public ParticleSystem normal;
            public ParticleSystem special;
            public MusicManager musicManager;
            public int measure;
            public bool[] executed;
            private int index;

            public void Initialize()
            {
                index = -1;
                executed = new bool[musicManager.measures[measure].notes.Count];
            }

            void Update()
            {
                if (musicManager.isPlaying)
                {
                    while (index + 1 < musicManager.measures[measure].notes.Count && musicManager.measures[measure].notes[index + 1].fraction < musicManager.AudioFraction - musicManager.measures[measure].offset)
                    {
                        index++;
                    }

                    float x = Mathf.LerpUnclamped(-6, 8, musicManager.AudioFraction - musicManager.measures[measure].offset);
                    float r1, r2;
                    if (musicManager.measures[measure].notes.Count > 0)
                    {
                        if (index < 0)
                        {
                            r1 = -0.5f + musicManager.measures[measure].offset;
                            r2 = musicManager.measures[measure].notes[index + 1].fraction + musicManager.measures[measure].offset;
                        }
                        else if (index + 1 < musicManager.measures[measure].notes.Count)
                        {
                            r1 = musicManager.measures[measure].notes[index].fraction + musicManager.measures[measure].offset;
                            r2 = musicManager.measures[measure].notes[index + 1].fraction + musicManager.measures[measure].offset;
                        }
                        else if (musicManager.AudioFraction < musicManager.measures[measure].signature + musicManager.measures[measure].offset)
                        {
                            r1 = musicManager.measures[measure].notes[index].fraction + musicManager.measures[measure].offset;
                            r2 = musicManager.measures[measure].signature + musicManager.measures[measure].offset;
                        }
                        else
                        {
                            r1 = musicManager.measures[measure].signature + musicManager.measures[measure].offset;
                            r2 = 1.5f + musicManager.measures[measure].offset;
                        }
                    }
                    else
                    {
                        r1 = -0.5f + musicManager.measures[measure].offset;
                        r2 = 1.5f + musicManager.measures[measure].offset;
                    }
                    float y = -50f * CalcY(r1, r2, musicManager.AudioFraction);
                    float z = -1;

                    transform.position = new Vector3(x, y - 2.5f, z);

                    if (musicManager.AudioFraction - musicManager.measures[measure].offset > musicManager.measures[measure].signature && y < 0)
                    {
                        gameObject.SetActive(false);
                    }
                }
            }

            private float CalcY(float r1, float r2, float x)
            {
                return (x - r1) * (x - r2) / Mathf.Pow((r2 - r1) / 2, 2) * (r2 - r1) * 0.25f;
            }

            public enum Effect
            {
                Best, Good, Poor
            }

            public void PlayEffect(bool impact, Effect effect)
            {
                ParticleSystem.EmitParams param = new ParticleSystem.EmitParams();
                switch (effect)
                {
                    case Effect.Best:
                        param.startColor = Color.green;
                        break;
                    case Effect.Good:
                        param.startColor = Color.yellow;
                        break;
                    case Effect.Poor:
                        param.startColor = Color.red;
                        break;
                }
                    (impact ? special : normal).Emit(param, 1);
            }
        }
    }
}