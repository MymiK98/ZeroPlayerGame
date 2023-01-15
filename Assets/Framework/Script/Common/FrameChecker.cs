using UnityEngine;
using System.Collections;

public class FrameChecker : MonoBehaviour
{
    [Header("Property")] [Range(1, 100)] public int fontSize = 0;
    [ColorUsage(true)] public Color guiColor;
    [Range(1.0f, 60.0f)] public float resetWorstTime = 15.0f;

    GUIStyle style;
    Rect rect;
    float deltaTime = 0.0f;
    float msec;
    float fps;
    float worstFps = float.MaxValue;
    string guiText;

    void Awake()
    {
        fontSize = fontSize == 0 ? 50 : fontSize;

        int w = Screen.width;
        int h = Screen.height;
        rect = new Rect(0, 0, w, h * 2 / 100);

        style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / fontSize;
        style.normal.textColor = guiColor;

        StartCoroutine(worstReset());
    }

    IEnumerator worstReset() //코루틴으로 15초 간격으로 최저 프레임 리셋해줌.
    {
        WaitForSecondsRealtime ws = new WaitForSecondsRealtime(resetWorstTime);
        while (true)
        {
            yield return ws;
            worstFps = 100.0f;
        }
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI() //소스로 GUI 표시.
    {
        msec = deltaTime * 1000.0f;
        fps = 1.0f / deltaTime; //초당 프레임 - 1초에

        if (fps < worstFps) //새로운 최저 fps가 나왔다면 worstFps 바꿔줌.
            worstFps = fps;

        guiText = string.Format("ms : {0:0.0} \nfps : {1:0.} \nworst : {2:0.}", msec, fps, worstFps);
        GUI.Label(rect, guiText, style);
    }
}