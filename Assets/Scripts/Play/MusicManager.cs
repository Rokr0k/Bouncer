using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bouncer
{
    namespace Play
    {
        public class MusicManager : MonoBehaviour
        {
            public AudioSource audioSource;
            private double startTime;
            private double pauseTime;

            public float AudioTime
            {
                get
                {
                    return (float)(AudioSettings.dspTime - startTime) * audioSource.pitch;
                }
            }

            [System.Serializable]
            public struct Note
            {
                public float time;
                public float fraction;
                public bool impact;
                public Note(float time, float fraction, bool impact)
                {
                    this.time = time;
                    this.fraction = fraction;
                    this.impact = impact;
                }
            };
            [System.Serializable]
            public struct Measure
            {
                public List<Note> notes;
                public float signature;
                public float offset;
                public float time;
                public Measure(List<Note> notes, float signature, float offset, float time)
                {
                    this.notes = notes;
                    this.signature = signature;
                    this.offset = offset;
                    this.time = time;
                }
            }
            public List<Measure> measures = new List<Measure>();
            private int measuresIndex;

            [System.Serializable]
            public struct Speedcore
            {
                public float time;
                public float fraction;
                public float bpm;
                public Speedcore(float time, float fraction, float bpm)
                {
                    this.time = time;
                    this.fraction = fraction;
                    this.bpm = bpm;
                }
            }
            public List<Speedcore> speedcore = new List<Speedcore>();
            private int speedcoreIndex;

            public float AudioFraction
            {
                get
                {
                    while (speedcoreIndex + 1 < speedcore.Count && speedcore[speedcoreIndex + 1].time < AudioTime)
                    {
                        speedcoreIndex++;
                    }
                    return (AudioTime - speedcore[speedcoreIndex].time) * speedcore[speedcoreIndex].bpm / 240.0f + speedcore[speedcoreIndex].fraction;
                }
            }

            public GameObject ball;
            private List<BallController> balls;

            public PreviewManager up, forward;

            public static readonly float[] judge = { 0.03f, 0.07f, 0.1f };

            public CameraController cam;

            private bool first;

            public bool isPlaying;

            private void Awake()
            {
                balls = new List<BallController>();
                Initialize();
            }

            private void Start()
            {
                for (int i = 0; i < 10; i++)
                {
                    BallController b = Instantiate(ball).GetComponent<BallController>();
                    b.musicManager = this;
                    b.gameObject.SetActive(false);
                    balls.Add(b);
                }
                cam.x1 = -8;
                cam.x2 = 8;
            }

            private void Update()
            {
                if (isPlaying)
                {
                    for (int i = 0; i < balls.Count; i++)
                    {
                        if (!balls[i].gameObject.activeSelf && measuresIndex < measures.Count)
                        {
                            balls[i].measure = measuresIndex;
                            balls[i].Initialize();
                            measuresIndex++;
                            balls[i].gameObject.SetActive(true);
                        }
                    }
                    up.Clear();
                    forward.Clear();
                    float x = -1;
                    for (int i = 0; i < measures.Count; i++)
                    {
                        if (measures[i].offset < AudioFraction && AudioFraction < measures[i].offset + measures[i].signature)
                        {
                            up.Display(measures[i].notes);
                            x = Mathf.Max(measures[i].signature, x);
                        }
                        else if (i > 0 && measures[i - 1].offset < AudioFraction && AudioFraction < measures[i - 1].offset + measures[i - 1].signature || i == 0 && measures[i].offset - measures[i].signature < AudioFraction && AudioFraction < measures[i].offset)
                        {
                            forward.Display(measures[i].notes);
                            x = Mathf.Max(measures[i].signature, x);
                        }
                    }
                    cam.x1 = -8;
                    cam.x2 = x < 0 ? 8 : x * 16 - 8;
                    foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
                    {
                        if (Input.GetKeyDown(key))
                        {
                            Press();
                        }
                    }
                }
            }

            public void Initialize()
            {
                first = true;
                isPlaying = false;
                measuresIndex = 0;
                speedcoreIndex = 0;
            }

            public void Play()
            {
                startTime = AudioSettings.dspTime + 2;
                audioSource.PlayScheduled(startTime);
                isPlaying = true;
            }

            public void Pause()
            {
                audioSource.Pause();
                pauseTime = AudioSettings.dspTime;
                isPlaying = false;
            }
            public void UnPause()
            {
                if (first)
                {
                    Play();
                    first = false;
                }
                else
                {
                    startTime += AudioSettings.dspTime - pauseTime;
                    audioSource.UnPause();
                    isPlaying = true;
                }
            }

            public void Press()
            {
                int ni = -1, nj = 0;
                for (int i = 0; i < balls.Count; i++)
                {
                    if (balls[i].gameObject.activeSelf)
                    {
                        for (int j = 0; j < measures[balls[i].measure].notes.Count; j++)
                        {
                            if (!balls[i].executed[j] && (ni < 0 || measures[balls[i].measure].notes[j].time < measures[balls[ni].measure].notes[nj].time))
                            {
                                ni = i;
                                nj = j;
                            }
                        }
                    }
                }
                if (ni>=0)
                {
                    balls[ni].executed[nj] = true;
                    float diff = Mathf.Abs(measures[balls[ni].measure].notes[nj].time - AudioTime);
                    if (diff < judge[0])
                    {
                        balls[ni].PlayEffect(measures[balls[ni].measure].notes[nj].impact, BallController.Effect.Best);
                    }
                    else if (diff < judge[1])
                    {
                        balls[ni].PlayEffect(measures[balls[ni].measure].notes[nj].impact, BallController.Effect.Good);
                    }
                    else if (diff < judge[2])
                    {
                        balls[ni].PlayEffect(measures[balls[ni].measure].notes[nj].impact, BallController.Effect.Poor);
                    }
                    else
                    {
                        balls[ni].executed[nj] = false;
                    }
                }
            }
        }
    }
}