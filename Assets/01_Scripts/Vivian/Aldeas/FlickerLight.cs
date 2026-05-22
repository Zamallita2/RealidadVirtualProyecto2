using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlickerLight : MonoBehaviour
{
    public Light targetLight;
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float speed = 2f;

    private float offset;

    private void Start()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();

        offset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        if (targetLight == null) return;

        float noise = Mathf.PerlinNoise(Time.time * speed, offset);
        targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}