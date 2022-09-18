using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toggle : MonoBehaviour
{
    public void ChangeActivation(bool value)
    {
        gameObject.SetActive(value);
    }
}
