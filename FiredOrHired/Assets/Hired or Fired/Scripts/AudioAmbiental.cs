using UnityEngine;
using System.Collections;

public class AudioAmbientManager : MonoBehaviour
{
    [Header("Auido Ambientales")]
    public AudioClip rainClip;

    public AudioClip[] carClips;
    public AudioClip[] honkClips;

    [Header("Spawn Aleatorio")]
    public float minInterval = 32;
    public float maxInterval = 80f;
    public float spawnRadius = 10f;

    [Header("Spatial Blend")]
    public float minDistance = 5f;
    public float maxDistance = 30f;

    private AudioSource rainSource;

    void Start()
    {
        rainSource = gameObject.AddComponent<AudioSource>();
        rainSource.clip = rainClip;
        rainSource.loop = true;
        rainSource.playOnAwake = true;
        rainSource.spatialBlend = 0f;
        rainSource.volume = 0.65f;
        rainSource.Play();

        StartCoroutine(SpawnAmbient(carClips));
        StartCoroutine(SpawnAmbient(honkClips));
    }

    IEnumerator SpawnAmbient(AudioClip[] clips)
    {
        while (true)
        {
            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip == null) continue;

            Vector2 circle = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(circle.x, 0f, circle.y);

            GameObject go = new GameObject("TempAudio");
            go.transform.position = spawnPos;
            AudioSource src = go.AddComponent<AudioSource>();
            src.clip = clip;
            src.spatialBlend = 1f;
            src.rolloffMode = AudioRolloffMode.Logarithmic;
            src.minDistance = minDistance;
            src.maxDistance = maxDistance;
            src.volume = Random.Range(0.8f, 1f);
            src.pitch = Random.Range(0.95f, 1.05f);
            src.Play();

            Destroy(go, clip.length + 0.1f);
        }
    }
}
