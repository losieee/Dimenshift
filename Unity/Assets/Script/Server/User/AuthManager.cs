using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft;
using System;
using System.Data;
using UnityEngine.UI;
public class AuthManager : MonoBehaviour
{
    // ���� URL �� PlayerPrefs Ű ��� ����
    private const string SERVER_URL = "http://localhost:4000";
    private const string ACCESS_TOKEN_PREFS_KEY = "AccessToken";
    private const string REFRESH_TOKEN_PREFS_KEY = "RefreshToken";
    private const string TOKEN_EXPIRY_PREFS_KEY = "ToeknExpiry";

    // ��ū �� ���� �ð� ���� ����
    private string accessToken;
    private string refreshToken;
    private DateTime tokenExpiryTime;

    public Text statusText;

    private AuthManager authManager;

    void Start()
    {
        LoadTokenFromPrets();
    }

    //private void OnRegisterClick()
    //{
    //    StartCoroutine(RegisteerCorouitine());
    //}

    //private IEnumerator RegisteerCorouitine()
    //{
    //    statusText.text = "ȸ������ ��...";
    //    //yield return StartCoroutine(authManager.Re)
    //}

    // PlayerPrefs���� ��ū ���� �ε�
    private void LoadTokenFromPrets()
    {
        accessToken = PlayerPrefs.GetString(ACCESS_TOKEN_PREFS_KEY, "");
        refreshToken = PlayerPrefs.GetString(REFRESH_TOKEN_PREFS_KEY, "");
        long expiryTicks = Convert.ToInt64(PlayerPrefs.GetString(TOKEN_EXPIRY_PREFS_KEY, "0"));
        tokenExpiryTime = new DateTime(expiryTicks);
    }

    // PlayerPres�� ��� ���� ����
    private void SaveTokenToPrefs(string accessToken, string refreshToken, DateTime expiryTime)
    {
        PlayerPrefs.SetString(ACCESS_TOKEN_PREFS_KEY, accessToken);
        PlayerPrefs.SetString(REFRESH_TOKEN_PREFS_KEY, refreshToken);
        PlayerPrefs.SetString(TOKEN_EXPIRY_PREFS_KEY, expiryTime.Ticks.ToString());
        PlayerPrefs.Save();

        this.accessToken = accessToken;
        this.refreshToken = refreshToken;
        this.tokenExpiryTime = expiryTime;
    }

    // �α��� ���� ������ ����
    [System.Serializable]
    public class LoginResponse
    {
        public string accessToken;
        public string refreshToken;
    }

    // ��ū ���� ���� ������ ����
    [System.Serializable]
    public class RefreshTokenResponse
    {
        public string accessToekn;
    }
}
