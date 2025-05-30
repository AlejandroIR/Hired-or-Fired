using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public PhoneManager phoneManager;

    [Header("NPCs")]
    public List<GameObject> npcPrefabs;

    [Header("Documents")]
    public List<GameObject> documents;

    [Header("Spawn Settings")]
    public Transform npcSpawnPoint;
    public Transform docSpawnPoint;

    [Header("Lights")]
    public List<Light> roomLights;

    [Header("Path Points")]
    public Transform npcEntrancePoint;
    public Transform npcChairPoint;
    public Transform npcExitPoint;

    [Header("Integration")]
    public NpcManager npcManager;

    public int currentNPCIndex = 0;
    public int currentDocIndex = 0;
    private GameObject currentNPC;
    private GameObject currentDoc;

    private bool npcManagerReady => npcManager != null && npcManager.IsReady;

    private bool npcReady = false;

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
        if (!npcReady) return;

        Debug.Log("Contratado");
        npcReady = false;

        Animator anim = currentNPC.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("IsSeated", false);
            anim.SetTrigger("StandUp");
        }

        if (phoneManager != null)
            phoneManager.FastReset();

        StartCoroutine(HandleHireDelay());

    }

    public void Fire()
    {
        if (!npcReady || currentNPC == null) return;

        Debug.Log("Eliminado");
        npcReady = false;

        currentNPC.GetComponent<RagdollController>().ActivateRagdoll();

        if (phoneManager != null)
            phoneManager.FastReset();

        StartCoroutine(HandleFireDelay());

    }

    public void FireByBullet(GameObject hitNpc)
    {
        if (!npcReady || hitNpc != currentNPC) return;

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
        Destroy(currentDoc);
        StartCoroutine(SpawnNextNPC());
    }

    IEnumerator HandleHireDelay()
    {

        yield return new WaitForSeconds(2.25f);

        // Ahora añadimos NpcWalker para que camine a la salida
        NpcWalker walker = currentNPC.AddComponent<NpcWalker>();
        walker.target = npcExitPoint;
        walker.playSitAnimationOnArrive = false;
        //Vector3 pos = currentNPC.transform.position;
        //pos.y += 0.3f;
        //currentNPC.transform.position = pos;
        walker.OnArrived = () =>
        {
            // Aquí destruyes y spawneas el siguiente NPC
            Destroy(currentNPC);
            Destroy(currentDoc);
            StartCoroutine(SpawnNextNPC());
        };
    }

    IEnumerator SpawnNextNPC()
    {
        npcReady = false;

        foreach (Light light in roomLights)
        {
            if (light != null)
                light.enabled = false;
        }
        yield return new WaitForSeconds(1f);

        if (currentNPC != null)
        {
            Destroy(currentNPC);
        }

        if (currentDoc != null)
        {
            Destroy(currentDoc);
        }

        if (currentNPCIndex >= npcPrefabs.Count)
        {
            Debug.Log("No hay más NPCs.");
            yield break;
        }

        GameObject npcToSpawn = npcPrefabs[currentNPCIndex];
        currentNPC = Instantiate(npcToSpawn, npcEntrancePoint.position, npcEntrancePoint.rotation);

        GameObject docToSpawn = documents[currentDocIndex];
        currentDoc = Instantiate(docToSpawn, docSpawnPoint.position, docSpawnPoint.rotation);

        // Tag the NPC for bullet collision detection
        currentNPC.tag = "NPC";

        currentNPCIndex++;
        currentDocIndex++;

        NpcWalker mover = currentNPC.AddComponent<NpcWalker>();
        mover.target = npcChairPoint;
        mover.OnArrived = () =>
        {
            StartCoroutine(CompleteNpcArrival());
        };

        IEnumerator CompleteNpcArrival()
        {
            yield return new WaitForSeconds(1f); // Esperar a que termine la animación de sentarse

            npcReady = true;

            Animator anim = currentNPC.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetBool("IsSeated", true); // Activar animación de estar sentado
            }

            foreach (Light light in roomLights)
            {
                if (light != null)
                    light.enabled = true;
            }

            if (npcManager != null)
                npcManager.ResetNpc();

            if (phoneManager != null)
                phoneManager.StartPhoneCall();
        }
    }
}
