using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        for (int i = 0; i < 20; i++)
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
            for (int j = 0; j < pool.Count; j++)
            {
                if (!pool[j].activeSelf)
                {
                    pool[j].SetActive(true);
                    pool[j].transform.localPosition = Vector3.right * Mathf.LerpUnclamped(-6, 8, notes[i].fraction);
                    break;
                }
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
