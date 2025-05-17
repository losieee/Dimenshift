using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [System.Serializable]
    public class WarningConfig
    {
        public Vector3 position;
        public Vector3 scale = new Vector3(3, 1, 3);
        public Vector3 rotationEuler;
        public float delay = 0f; // �� ����� ���� �ð�
    }

    public GameObject warningAreaPrefab;
    public WarningConfig[] warningConfigs; // ���� ���� ������ ���� �迭
    public float fillDuration = 2f;

    private void Start()
    {
        foreach (WarningConfig config in warningConfigs)
        {
            StartCoroutine(SpawnWarningAreaWithDelay(config));
        }
    }

    private IEnumerator SpawnWarningAreaWithDelay(WarningConfig config)
    {
        yield return new WaitForSeconds(config.delay);

        GameObject instance = Instantiate(
            warningAreaPrefab,
            config.position,
            Quaternion.Euler(config.rotationEuler)
        );

        instance.transform.localScale = config.scale;

        // FillController ����
        FillController controller = instance.GetComponent<FillController>();
        if (controller != null)
        {
            controller.fillDuration = fillDuration;
            controller.enabled = true;
        }
    }
}
