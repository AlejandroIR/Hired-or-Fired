using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(ParticleSystem))]
public class SubEmitterSound : MonoBehaviour
{
    private ParticleSystem _parentParticleSystem;

    private IDictionary<uint, ParticleSystem.Particle> _trackedParticles = new Dictionary<uint, ParticleSystem.Particle>();

    public AudioClip spawnSound;
    public AudioSource audioSource;
    void Start()
    {
        _parentParticleSystem = this.GetComponent<ParticleSystem>();
        if (_parentParticleSystem == null)
            Debug.LogError("Missing ParticleSystem!", this);
    }

    void Update()
    {
        var liveParticles       = new ParticleSystem.Particle[_parentParticleSystem.particleCount];
        _parentParticleSystem.GetParticles(liveParticles);

        var particleDelta       = GetParticleDelta(liveParticles);

        foreach (var particleAdded in particleDelta.Added)
        {
            //Todo: Play "Spawn" sound - use particleAdded.position to play at right position
            // Example: AudioSource.PlayClipAtPoint(spawnSound, particleAdded.position);

            audioSource.pitch = Random.Range(0.8f, 1.2f); // Random pitch variation
            audioSource.PlayOneShot(spawnSound);
            Debug.Log($"New particle spawned '{particleAdded.randomSeed}' at position '{particleAdded.position}'");
        }

        foreach (var particleRemoved in particleDelta.Removed)
        {
            //Todo: Play "Disappear" sound - use particleRemoved.position to play at right position
            Debug.Log($"Particle despawned '{particleRemoved.randomSeed}' at position '{particleRemoved.position}'");
        }
    }

    private ParticleDelta GetParticleDelta(ParticleSystem.Particle[] liveParticles)
    {
        var deltaResult = new ParticleDelta();

        foreach (var activeParticle in liveParticles)
        {
            ParticleSystem.Particle foundParticle;
            if(_trackedParticles.TryGetValue(activeParticle.randomSeed, out foundParticle))
            {
                _trackedParticles[activeParticle.randomSeed] = activeParticle;
            }
            else
            {
                deltaResult.Added.Add(activeParticle);
                _trackedParticles.Add(activeParticle.randomSeed, activeParticle);
            }
        }

        var updatedParticleAsDictionary = liveParticles.ToDictionary(x => x.randomSeed, x => x);
        var dictionaryKeysAsList = _trackedParticles.Keys.ToList();

        foreach (var dictionaryKey in dictionaryKeysAsList)
        {
            if (updatedParticleAsDictionary.ContainsKey(dictionaryKey) == false)
            {
                deltaResult.Removed.Add(_trackedParticles[dictionaryKey]);
                _trackedParticles.Remove(dictionaryKey);
            }
        }
        
        return deltaResult;
    }

    private class ParticleDelta
    {
        public IList<ParticleSystem.Particle> Added     { get; set; }   = new List<ParticleSystem.Particle>();
        public IList<ParticleSystem.Particle> Removed   { get; set; }   = new List<ParticleSystem.Particle>();
    }
}