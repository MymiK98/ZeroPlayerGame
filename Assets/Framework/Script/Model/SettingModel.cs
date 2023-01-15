using System;
using System.IO;
using UnityEngine;

[Serializable]
public class UserCustomOption
{
    /// <summary>
    /// 마지막 로그인
    /// </summary>
    public string lastLogin = DateTime.Now.ToString();
}

public class SettingModel : Model
{
    GameModel _owner;

    #region UserSystem

    public String userSystemOs
    {
        get { return SystemInfo.operatingSystem; }
    }

    public String userSystemUid
    {
        get { return SystemInfo.deviceUniqueIdentifier; }
    }

    SystemLanguage userSystemLanguage = Application.systemLanguage;

    public SystemLanguage UserSystemLanguage
    {
        get { return userSystemLanguage; }
    }

    #endregion

    private const string optionFileName = "SayDuoUserOption.txt";

    private UserCustomOption userCustomOption = new UserCustomOption();

    public UserCustomOption UserCustomOption
    {
        get { return userCustomOption; }
    }

    public void Setup(GameModel owner)
    {
        _owner = owner;

        //설정값 파싱
        if (File.Exists($"{Application.persistentDataPath}/{optionFileName}"))
            GetSayDuoUserOption(); //설정 긁어오기
        else
            SetSayDuoUserOption(); //디폴트 세팅
    }

    public void GetSayDuoUserOption()
    {
        string path = $"{Application.persistentDataPath}/{optionFileName}";
        string data = File.ReadAllText(path);
        userCustomOption = JsonUtility.FromJson<UserCustomOption>(data);

        if (!DateTime.Parse(userCustomOption.lastLogin).Date.Equals(DateTime.Today))
        {
            //새로운 날들어왔을때 초기화
        }

        userCustomOption.lastLogin = DateTime.Now.ToString();

    }

    public void SetSayDuoUserOption()
    {
        string data = JsonUtility.ToJson(userCustomOption);
        string path = $"{Application.persistentDataPath}/{optionFileName}";
        File.WriteAllText(path, data);
    }
}