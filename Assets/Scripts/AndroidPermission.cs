using Serilog;
using UnityEngine;

// Android 11+ 文件权限：引导用户到系统设置开启"管理所有文件"
public class AndroidPermission : MonoBehaviour
{
    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        RequestFilePermission();
#endif
    }

    void RequestFilePermission()
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                // Android 11+ (API 30): 检查 MANAGE_EXTERNAL_STORAGE
                if (GetAndroidApiLevel() >= 30)
                {
                    using (var environment = new AndroidJavaClass("android.os.Environment"))
                    {
                        bool hasPermission = environment.CallStatic<bool>("isExternalStorageManager");
                        if (!hasPermission)
                        {
                            // 打开系统"所有文件访问权限"设置页面
                            using (var intent = new AndroidJavaObject("android.content.Intent",
                                "android.settings.MANAGE_APP_ALL_FILES_ACCESS_PERMISSION"))
                            {
                                using (var uri = new AndroidJavaClass("android.net.Uri"))
                                {
                                    var uriObj = uri.CallStatic<AndroidJavaObject>("parse",
                                        "package:" + activity.Call<string>("getPackageName"));
                                    intent.Call<AndroidJavaObject>("setData", uriObj);
                                    activity.Call("startActivity", intent);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Android 10 及以下：普通存储权限
                    if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(
                        UnityEngine.Android.Permission.ExternalStorageWrite))
                    {
                        UnityEngine.Android.Permission.RequestUserPermission(
                            UnityEngine.Android.Permission.ExternalStorageWrite);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Log.ForContext("Tag", "Android").Warning(e, "Permission error");
        }
    }

    int GetAndroidApiLevel()
    {
        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            return version.GetStatic<int>("SDK_INT");
    }
}
