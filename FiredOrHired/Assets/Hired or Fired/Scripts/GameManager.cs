using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("NPCs")]
    public List<GameObject> npcPrefabs;

    [Header("Spawn Settings")]
    public Transform npcSpawnPoint;
    public Light roomLight;

    [Header("Integration")]
    public NpcManager npcManager;

    public int currentNPCIndex = 0;
    private GameObject currentNPC;

    void Start()
    {
        StartCoroutine(SpawnNextNPC());
        Debug.Log("Spawn NPC");
        
        // Find NpcManager if not assigned
        if (npcManager == null)
            npcManager = FindObjectOfType<NpcManager>();
    }

    public void Hire()
    {
        Debug.Log("Contratado");

        Destroy(currentNPC);
        StartCoroutine(SpawnNextNPC());
    }

    public void Fire()
    {
        Debug.Log("Eliminado");

        currentNPC.GetComponent<RagdollController>().ActivateRagdoll();
        StartCoroutine(HandleFireDelay());
    }

    public void FireByBullet(GameObject hitNpc)
    {
        Debug.Log("NPC shot by bullet");
        
        // Make sure this is our current NPC
        if (hitNpc != currentNPC)
            return;
            
        // Trigger the regular fire logic
        Fire();
        
        // Sync with NpcManager if available
        if (npcManager != null)
        {
            npcManager.NextNpc();
        }
    }

    IEnumerator HandleFireDelay()
    {
        yield return new WaitForSeconds(2f);

        Destroy(currentNPC);
        StartCoroutine(SpawnNextNPC());
    }

    IEnumerator SpawnNextNPC()
    {
        roomLight.enabled = false;
        yield return new WaitForSeconds(1f);

        if (currentNPC != null)
        {
            Destroy(currentNPC);
        }

        if (currentNPCIndex >= npcPrefabs.Count)
        {
            Debug.Log("No hay más NPCs.");
            yield break;
        }

        GameObject npcToSpawn = npcPrefabs[currentNPCIndex];
        currentNPC = Instantiate(npcToSpawn, npcSpawnPoint.position, npcSpawnPoint.rotation);
        
        // Tag the NPC for bullet collision detection
        currentNPC.tag = "NPC";
        
        currentNPCIndex++;

        yield return new WaitForSeconds(0.5f);
        roomLight.enabled = true;
        
        // Sync with NpcManager if available
        if (npcManager != null && npcManager.currentNpcIndex != currentNPCIndex - 1)
        {
            npcManager.ChangeNpc(currentNPCIndex - 1);
        }
    }
}
