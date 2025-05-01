using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;               // ���� ���
    public float height = 10.0f;           // Ÿ�����κ����� ����
    public float distance = 6.0f;          // Ÿ�����κ����� �Ÿ� (����)
    public float angle = 45.0f;            // �Ʒ��� ������ ����
    public float followSpeed = 10f;        // ���󰡴� �ӵ�

    void LateUpdate()
    {
        if (target == null) return;

        // ī�޶� ��ġ�� ������ ��� (���ʿ��� ���� �̵��� ��ġ)
        Vector3 offset = Quaternion.Euler(angle, 0, 0) * new Vector3(0, 0, -distance);
        Vector3 desiredPosition = target.position + offset + Vector3.up * height;

        // �ε巯�� �̵�
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // �׻� Ÿ���� �ٶ󺸰�
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}