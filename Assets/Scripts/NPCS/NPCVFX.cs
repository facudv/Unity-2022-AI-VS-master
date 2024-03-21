using UnityEngine;

public class NPCVFX : MonoBehaviour
{
    public ParticleSystem[] allMyParticles;
    public ParticleSystem floorBloodParticle;

    public void OnDamageVFX()
    {
        int random = Random.Range(0, allMyParticles.Length);

        floorBloodParticle.Stop();
        floorBloodParticle.Play();

        allMyParticles[random].Stop();
        allMyParticles[random].Play();
    }
}
