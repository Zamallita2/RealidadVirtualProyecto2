using System.Collections.Generic;
using UnityEngine;

public class BuildingRoomVisual :
MonoBehaviour
{
    [Header("Número local")]
    public int localRoomNumber = 1;

    [Header("Spawn")]
    public Transform[] spawnPoints =
    new Transform[5];

    private readonly
    List<GameObject>
    spawnedObjects =
    new List<GameObject>();


    public void ClearVisual()
    {
        for (
        int i = 0;
        i < spawnedObjects.Count;
        i++
        )
        {
            if (
            spawnedObjects[i] != null
            )
            {
                Destroy(
                spawnedObjects[i]
                );
            }
        }

        spawnedObjects.Clear();
    }

    public void RenderRoom(
    RoomConfigData config
    )
    {
        ClearVisual();

        if (config == null)
            return;

        for (
        int i = 0;
        i < spawnPoints.Length &&
        i < config.enemyIds.Count;
        i++
        )
        {
            string enemyId =
            config.enemyIds[i];

            if (
            string.IsNullOrEmpty(
            enemyId
            )
            )
                continue;

            EnemyGachaData data =
            GachaInventoryManager
            .Instance
            .GetEnemyData(
            enemyId
            );

            if (data == null)
                continue;

            if (
            data.enemyPrefab == null
            )
                continue;

            Transform spawn =
            spawnPoints[i];

            GameObject obj =
            Instantiate(
            data.enemyPrefab,
            spawn
            );

            obj.transform
            .localPosition =
            Vector3.zero;

            obj.transform
            .localRotation =
            Quaternion.Euler(
            data.roomRotation
            );

            obj.transform
            .localScale =
            data.roomScale;

            spawnedObjects
            .Add(obj);
        }
    }
}