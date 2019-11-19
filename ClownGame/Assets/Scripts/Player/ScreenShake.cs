using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public float returnLerp;
    public float screenShakeDelay;

    public void Update()
    {
        transform.position = Vector3.Lerp(transform.position, transform.parent.position, Time.deltaTime * returnLerp);
        transform.rotation = Quaternion.Lerp(transform.rotation, transform.parent.rotation, Time.deltaTime * returnLerp);
    }

    public IEnumerator CallScreenShake(float time, float intensity)
    {
        while(time >= 0)
        {
            transform.Translate(new Vector3(Random.Range(-intensity, intensity), Random.Range(-intensity, intensity)) * 0.1f);
            transform.Rotate(new Vector3(Random.Range(-intensity, intensity), Random.Range(-intensity, intensity)) * 4);
            time -= screenShakeDelay;
            yield return new WaitForSeconds(screenShakeDelay);
        }
    }
}
