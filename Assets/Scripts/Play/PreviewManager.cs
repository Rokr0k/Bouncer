using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bouncer
{
    namespace Play
    {
        public class PreviewManager : MonoBehaviour
        {
            public GameObject note;
            public List<GameObject> pool;

            private void Awake()
            {
                pool = new List<GameObject>();
            }

            private void Start()
            {
                for (int i = 0; i < 50; i++)
                {
                    GameObject aa = Instantiate(note, transform);
                    aa.SetActive(false);
                    pool.Add(aa);
                }
            }

            public void Display(List<MusicManager.Note> notes)
            {
                for (int i = 0; i < notes.Count; i++)
                {
                    bool newFlag = true;
                    for (int j = 0; j < pool.Count; j++)
                    {
                        if (!pool[j].activeSelf)
                        {
                            pool[j].SetActive(true);
                            pool[j].transform.localPosition = Vector3.right * Mathf.LerpUnclamped(-8, 8, notes[i].fraction);
                            newFlag = false;
                            break;
                        }
                    }
                    if (newFlag)
                    {
                        GameObject aa = Instantiate(note, transform);
                        aa.transform.localPosition = Vector3.right * Mathf.LerpUnclamped(-8, 8, notes[i].fraction);
                        pool.Add(aa);
                    }
                }
            }

            public void Clear()
            {
                for (int j = 0; j < pool.Count; j++)
                {
                    pool[j].SetActive(false);
                }
            }
        }
    }
}