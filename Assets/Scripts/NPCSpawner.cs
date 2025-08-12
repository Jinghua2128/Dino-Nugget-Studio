using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Prefabs")]
    [SerializeField] private GameObject normalShopperPrefab;
    [SerializeField] private GameObject shoplifterPrefab;
    [SerializeField] private GameObject distractorPrefab;

    [Header("Spawn Amounts")]
    [SerializeField] private int numNormalShoppers = 5;
    [SerializeField] private int numShoplifters = 3;
    [SerializeField] private int numDistractors = 2;

    [Header("Spawn Points")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform stallCenter; // Center of grocery stall

    void Start()
    {
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        SpawnNPCs(normalShopperPrefab, numNormalShoppers, AssignNormalShopperReferences);
        SpawnNPCs(shoplifterPrefab, numShoplifters, AssignShoplifterReferences);
        SpawnNPCs(distractorPrefab, numDistractors, AssignDistractorReferences);
    }

    private void SpawnNPCs(GameObject prefab, int count, System.Action<GameObject> assignReferences)
    {
        if (prefab == null) return;

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = spawnPoint.position + Random.insideUnitSphere * 2f;
            spawnPos.y = spawnPoint.position.y;
            GameObject npc = Instantiate(prefab, spawnPos, Quaternion.identity);
            assignReferences(npc);
        }
    }

    private void AssignNormalShopperReferences(GameObject npc)
    {
        NormalShopperNPC shopper = npc.GetComponent<NormalShopperNPC>();
        if (shopper != null)
        {
            shopper.stallCenter = stallCenter;
        }
    }

    private void AssignShoplifterReferences(GameObject npc)
    {
        ShoplifterNPC shoplifter = npc.GetComponent<ShoplifterNPC>();
        if (shoplifter != null)
        {
            shoplifter.stallCenter = stallCenter;
        }
    }

    private void AssignDistractorReferences(GameObject npc)
    {
        DistractorNPC distractor = npc.GetComponent<DistractorNPC>();
        if (distractor != null)
        {
            distractor.stallCenter = stallCenter;
            ShoplifterNPC[] shoplifters = FindObjectsOfType<ShoplifterNPC>();
            if (shoplifters.Length > 0)
            {
                distractor.partner = shoplifters[Random.Range(0, shoplifters.Length)];
            }
        }
    }
}