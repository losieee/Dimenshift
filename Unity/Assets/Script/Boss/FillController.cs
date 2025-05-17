using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillController : MonoBehaviour
{
    public MeshRenderer fillRenderer;
    public float fillDuration = 2f;

    private Material fillMaterial;
    private float elapsed = 0f;

    private void Start()
    {
        if (fillRenderer == null)
        {
            enabled = false;
            return;
        }

        // ��Ƽ���� �ν��Ͻ�ȭ (�ٸ� ������Ʈ�� �������� �ʰ�)
        fillMaterial = fillRenderer.material;
        fillMaterial.SetFloat("_FillAmount", 0f);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float fill = Mathf.Clamp01(elapsed / fillDuration);
        fillMaterial.SetFloat("_FillAmount", fill);

        if (fill >= 1f)
        {
            // ������Ʈ ���� (BasePlane ���� WarningArea ��ü)
            Destroy(transform.root.gameObject);
        }
    }
}
