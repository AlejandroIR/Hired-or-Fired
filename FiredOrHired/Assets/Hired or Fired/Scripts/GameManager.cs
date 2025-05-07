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

    private int currentNPCIndex = 0;
    private GameObject currentNPC;

    void Start()
    {
        StartCoroutine(SpawnNextNPC());
        Debug.Log("Spawn NPC");
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
        Destroy(currentNPC, 5f);
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
        currentNPCIndex++;

        yield return new WaitForSeconds(0.5f);
        roomLight.enabled = true;
    }
}
