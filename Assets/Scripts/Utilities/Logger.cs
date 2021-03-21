/*
 * Creator: Nate Smith
 * Date Created: 3/20/2021
 * Description: Debug Logger.
 * 
 * Debug.Logs messages only if the application is the UnityEditor.
 * 
 */
public static class Logger
{
    public static void Debug(string logMsg)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.Log(logMsg);
#endif
    }

    public static void Warning(string logMsg)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.LogWarning(logMsg);
#endif
    }
}
