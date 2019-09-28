using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coloring : MonoBehaviour
{

    void Awake()
    {
        LeanTween.color(gameObject, Color.black, 0.0f);
    }
}
