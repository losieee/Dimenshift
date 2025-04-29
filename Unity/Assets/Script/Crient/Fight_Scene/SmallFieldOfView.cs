using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallFieldOfView : MonoBehaviour
{
    // �þ� ������ �������� �þ� ����
    public float smallViewRadius;

    [Range(0, 360)]
    public float viewAngle;
    // �����ִ� �þ� ǥ��
    public float meshResolution;

    // ����ũ 2��
    public LayerMask targetMask, obstacleMask;

    // Target mask�� ray hit�� transform�� �����ϴ� ����Ʈ
    public List<Transform> visibleTargets = new List<Transform>();

    Mesh viewMesh;
    public MeshFilter viewMeshFilter;

    public int edgeResolveIterations;
    public float edgeDstThreshold;

    public float maskCutawayDst = 0.01f;

    void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        DisableEnemyChildren();

        StartCoroutine(FindTargetsWithDelay(0.2f));
    }
    void OnEnable()
    {
        if (viewMesh == null)
        {
            viewMesh = new Mesh();
            viewMesh.name = "View Mesh";
        }
        viewMeshFilter.mesh = viewMesh;  // ��Ȱ��ȭ �� �ٽ� Ȱ��ȭ�� ���� Mesh�� �Ҵ�

        StartCoroutine(FindTargetsWithDelay(0.2f));  // ��Ȱ��ȭ �� �ٽ� Ȱ��ȭ�� ���� �ڷ�ƾ �����
    }
    void OnDisable()
    {
        StopAllCoroutines();  // ��Ȱ��ȭ�� �� ���� ���� �ڷ�ƾ�� ����
    }

    void DisableEnemyChildren()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == LayerMask.NameToLayer("Enemy")) // "Enemy" Layer���� Ȯ��
            {
                if (obj.transform.childCount > 0) // ���� ������Ʈ�� �ִ��� Ȯ��
                {
                    Transform child = obj.transform.GetChild(0); // ù ��° ���� ������Ʈ ��������
                    child.gameObject.SetActive(false); // ���� ������Ʈ�� ��Ȱ��ȭ
                }
            }
        }
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, smallViewRadius, targetMask);

        List<Transform> allDetected = new List<Transform>(); // ��� ��� ����

        foreach (Collider targetCollider in targetsInViewRadius)
        {
            Transform target = targetCollider.transform;
            allDetected.Add(target);

            Vector3 dirToTarget = (target.position - transform.position).normalized;
            float dstToTarget = Vector3.Distance(transform.position, target.position);

            // �þ� ���� ���� ��ȭ
            bool isWithinView = viewAngle >= 360f || Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2;

            // ��ֹ� üũ ���� �Ǵ� �ʿ� �� ����
            // bool isNotBlocked = !Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask);

            if (isWithinView)
            {
                visibleTargets.Add(target);
            }
        }

        // ��� ������ ��� �� visibleTargets�� ���Ե� ��츸 Ȱ��ȭ
        foreach (Transform target in allDetected)
        {
            bool isVisible = visibleTargets.Contains(target);

            foreach (Transform child in target.GetComponentsInChildren<Transform>(true))
            {
                if (child != target)
                    child.gameObject.SetActive(isVisible);
            }
        }
    }

    // y�� ���Ϸ� ���� 3���� ���� ���ͷ� ��ȯ�Ѵ�.
    public Vector3 DirFromAngle(float angleDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Cos((-angleDegrees + 90) * Mathf.Deg2Rad), 0, Mathf.Sin((-angleDegrees + 90) * Mathf.Deg2Rad));
    }
    void DrawFieldOfView()
    {
        if (viewAngle <= 0) // viewAngle�� 0�����ϰ�� �޽û��� �ߴ�.
        {
            viewMesh.Clear();
            return;
        }

        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo prevViewCast = new ViewCastInfo();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            // i�� 0�̸� prevViewCast�� �ƹ� ���� ���� ���� ������ �� �� �����Ƿ� �ǳʶڴ�.
            if (i != 0)
            {
                bool edgeDstThresholdExceed = Mathf.Abs(prevViewCast.dst - newViewCast.dst) > edgeDstThreshold;

                // �� �� �� raycast�� ��ֹ��� ������ �ʾҰų� �� raycast�� ���� �ٸ� ��ֹ��� hit �� ���̶�� edgeDstThresholdExceed ���η� ���
                if (prevViewCast.hit != newViewCast.hit || (prevViewCast.hit && newViewCast.hit && edgeDstThresholdExceed))
                {
                    Edge e = FindEdge(prevViewCast, newViewCast);

                    // zero�� �ƴ� ������ �߰���
                    if (e.PointA != Vector3.zero)
                    {
                        viewPoints.Add(e.PointA);
                    }

                    if (e.PointB != Vector3.zero)
                    {
                        viewPoints.Add(e.PointB);
                    }
                }
            }

            viewPoints.Add(newViewCast.point);
            prevViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];
        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]) + Vector3.forward * maskCutawayDst;

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }
    void LateUpdate()
    {
        DrawFieldOfView();
    }
    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }
    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, smallViewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * smallViewRadius, smallViewRadius, globalAngle);
        }
    }
    public struct Edge
    {
        public Vector3 PointA, PointB;
        public Edge(Vector3 _PointA, Vector3 _PointB)
        {
            PointA = _PointA;
            PointB = _PointB;
        }
    }
    Edge FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = minAngle + (maxAngle - minAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);
            bool edgeDstThresholdExceed = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceed)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new Edge(minPoint, maxPoint);
    }
}
