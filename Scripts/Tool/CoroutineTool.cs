using System.Collections;
using UnityEngine;

public static class CoroutineTool
{

    public static IEnumerator GetIEnumerator(float time)
    {
        float currentTime = 0;
        while (currentTime<time)
        {
            currentTime += Time.deltaTime;
            yield return null;
        }
    }
}