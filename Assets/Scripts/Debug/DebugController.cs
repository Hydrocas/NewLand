using UnityEngine;
using UnityEngine.UI;

public class DebugController : MonoBehaviour
{
    private bool isCheatConsoleShowed;
    private string inputText = "baane";

    private float y = 0;
    private float historyLength = 200;
    private float lineHeight = 20;
    private float inputFieldHeight = 10;
    private float verticalSpacing = 30;
    private float padding = 5;

    private void Start()
    {
        isCheatConsoleShowed = false;
    }

    private void Update()
    {
        if (Input.GetButtonDown("ShowCheatConsole"))
            isCheatConsoleShowed = !isCheatConsoleShowed;
    }

    private void OnGUI()
    {
        if (!isCheatConsoleShowed)
            return;

        GUI.Box(new Rect(0, y, Screen.width, historyLength), "");
        Rect viewport = new Rect(0, 0, Screen.width - verticalSpacing, lineHeight);// * commandList.Count);
        //scroll = GUI.BeginScrollView(new Rect(0, y + padding, Screen.width, historyLength - inputFieldHeight), scroll, viewport);

        GUI.backgroundColor = new Color(0, 0, 0, 0);
        //inputText = GUI.TextField(new Rect(inputFieldHeight, y + padding, Screen.width - lineHeight, lineHeight), inputText);
        inputText = GUILayout.TextField(inputText);

        Debug.Log(inputText);

        if (Event.current.type == EventType.KeyDown && Event.current.character == '\n')
        {
            inputText = "";
        }
    }
}
