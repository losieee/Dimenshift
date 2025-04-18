#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SmallFieldOfView))]

public class SmallFieldOfViewEditor : Editor
{
    void OnSceneGUI()
    {
        // SmallFieldOfView ��ũ��Ʈ ��������
        SmallFieldOfView fow = (SmallFieldOfView)target;
        // �þ� �ݰ� ������ ǥ��
        Handles.color = Color.white;
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, fow.smallViewRadius);
        // �þ߰� ǥ��
        Vector3 viewAngleA = fow.DirFromAngle(-fow.viewAngle / 2, false);
        Vector3 viewAngleB = fow.DirFromAngle(fow.viewAngle / 2, false);

        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleA * fow.smallViewRadius);
        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleB * fow.smallViewRadius);

        // �� �߰� �� ǥ��
        Handles.color = Color.red;
        foreach (Transform visible in fow.visibleTargets)
        {
            Handles.DrawLine(fow.transform.position, visible.transform.position);
        }
    }
}
#endif