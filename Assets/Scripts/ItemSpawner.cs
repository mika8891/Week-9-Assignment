using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Prefab & Spawning Settings")]
    public Item itemPrefab;
    public Transform parent;
    public float spawnRate = 1.0f;
    public float moveSpeed = 100.0f;
    public float spawnZ = 300.0f;
    public float destroyZ = -20.0f;

    [Header("Track Dimensions")]
    public float[] laneX = { -2.0f, 0.0f, 2.0f };
    public float groundY = -1.0f;

    [HideInInspector]
    public List<Item> activeItems = new List<Item>();
    
    private float spawnTimer;

    private void Update()
    {
        // 1. Spawning Timer
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnRate)
        {
            SpawnItem();
            spawnTimer = 0.0f;
        }

        // 2. Move items forward & Despawn off-screen items
        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            Item item = activeItems[i];
            if (item == null)
            {
                activeItems.RemoveAt(i);
                continue;
            }

            item.itemPosition.z -= moveSpeed * Time.deltaTime;

            if (item.itemPosition.z < destroyZ)
            {
                activeItems.RemoveAt(i);
                Destroy(item.gameObject);
            }
        }

        // 3. Hierarchy Sibling Index Sorting for Canvas/2D ordering
        activeItems.Sort((a, b) => b.itemPosition.z.CompareTo(a.itemPosition.z));
        for (int i = 0; i < activeItems.Count; i++)
        {
            if (activeItems[i] != null)
            {
                activeItems[i].transform.SetSiblingIndex(i);
            }
        }
    }

    private void SpawnItem()
    {
        if (itemPrefab == null) return;

        Transform spawnParent = (parent != null) ? parent : transform;
        Item newItem = Instantiate(itemPrefab, spawnParent);

        int randomLane = Random.Range(0, laneX.Length);
        newItem.itemPosition = new Vector3(laneX[randomLane], groundY, spawnZ);
        
        activeItems.Add(newItem);
    }
}