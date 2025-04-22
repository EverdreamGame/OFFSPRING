using System;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
public class TakeScreenshot : MonoBehaviour
{
    public static void TakeScreenshotFunc(string fileName = "screenshot")
    {
        // Define the folder path relative to the Unity project root
        string relativeFolderPath = "Assets/ScreenShots/" + fileName + ".png";
        string absoluteFolderPath = Path.Combine(Application.dataPath, "ScreenShots");

        // Take the screenshot using the relative path
        ScreenCapture.CaptureScreenshot(relativeFolderPath);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
            TakeScreenshotFunc(System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }
}
#endif
