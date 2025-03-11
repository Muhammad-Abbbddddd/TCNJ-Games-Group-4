using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedItem : MonoBehaviour, IItem
{
    public static event Action<float> OnSpeedCollected;
    public float speedMultiplier = 1.5f;
    public void Collect()
    {
        //Let PlayerMovement script know
        OnSpeedCollected.Invoke(speedMultiplier);
        Destroy(gameObject);
    }
}
