using System;
using System.Collections;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;
[Pool]
public class HitEffect : MonoBehaviour
{
    private void OnParticleSystemStopped()
    {
        this.JKGameObjectPushPool();
    }
}
