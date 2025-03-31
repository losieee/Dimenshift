using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Respawn : MonoBehaviour
{
    public TextMeshProUGUI countdownText; // ī��Ʈ�ٿ� UI �ؽ�Ʈ
    private float countdownTime = 10f; // 10�� ī��Ʈ�ٿ�
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Countdown());
    }
    IEnumerator Countdown()
    {
        while (countdownTime > 0)
        {
            countdownText.text =countdownTime.ToString("F0"); // �Ҽ��� ����
            yield return new WaitForSeconds(1.0f);
            countdownTime--;
        }

        countdownText.text = "Start!"; // ī��Ʈ�ٿ� ������ ǥ��
        yield return new WaitForSeconds(1.0f); // 1�� ��� �� �� �̵�
        SceneManager.LoadScene("SampleScene");
    }
}
