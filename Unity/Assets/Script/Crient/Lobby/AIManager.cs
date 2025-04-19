using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public GameObject aiPrefab;
    private int aiSpawnCount = 0;

    private readonly string[] aiSpawnPoints = {
        "RespawnPoint_02",
        "RespawnPoint_03",
        "RespawnPoint_04"
    };

    public void SpawnAI()
    {
        if (aiSpawnCount >= aiSpawnPoints.Length)
        {
            Debug.Log("AI ĳ���ʹ� �ִ� 3������� �߰��� �� �ֽ��ϴ�.");
            return;
        }

        string pointName = aiSpawnPoints[aiSpawnCount];
        GameObject spawnPoint = GameObject.Find(pointName);

        if (spawnPoint != null && aiPrefab != null)
        {
            Instantiate(aiPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
            aiSpawnCount++;
        }
        else
        {
            Debug.LogWarning("AI ���� ��ġ �Ǵ� �������� �����ϴ�.");
        }
    }
}