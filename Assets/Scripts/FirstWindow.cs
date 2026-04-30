using System.IO;
using System.Collections;
using System.Collections.Generic;
using Serilog;
using UnityEngine;
using UnityEngine.UI;
using MinorShift._Library;

public class FirstWindow : MonoBehaviour
{
    public static void Show()
    {
        var obj = Resources.Load<GameObject>("Prefab/FirstWindow");
        obj = GameObject.Instantiate(obj);
        obj.name = "FirstWindow";
    }
    static System.Collections.IEnumerator Run(string workspace, string era)
    {
        var async = Resources.UnloadUnusedAssets();
        while(!async.isDone)
            yield return null;

        var ow = EmueraContent.instance.option_window;
        ow.gameObject.SetActive(true);
        ow.ShowGameButton(true);
        ow.ShowInProgress(true);
        yield return null;

        System.GC.Collect();
        SpriteManager.Init();

        Sys.SetWorkFolder(workspace);
        Sys.SetSourceFolder(era);
        uEmuera.Utils.ResourcePrepare();

        async = Resources.UnloadUnusedAssets();
        while(!async.isDone)
            yield return null;

        EmueraContent.instance.SetNoReady();
        var emuera = Object.FindAnyObjectByType<EmueraMain>();
        emuera.Run();
    }

    void Start()
    {
        gameObject.AddComponent<SafeArea>();
        if(!string.IsNullOrEmpty(MultiLanguage.FirstWindowTitlebar))
            titlebar.text = MultiLanguage.FirstWindowTitlebar;

        scroll_rect_ = GenericUtils.FindChildByName<ScrollRect>(gameObject, "ScrollRect");
        item_ = GenericUtils.FindChildByName(gameObject, "Item", true);
        setting_ = GenericUtils.FindChildByName(gameObject, "optionbtn", true);
        GenericUtils.SetListenerOnClick(setting_, OnOptionClick);

        GenericUtils.FindChildByName<Text>(gameObject, "version")
            .text = Application.version + " ";

#if UNITY_ANDROID && !UNITY_EDITOR
        // 安卓：先扫描内置存储（无需权限）
        GetList(Application.persistentDataPath + "/emuera");
        GetList(Application.persistentDataPath + "/era");
        GetList(Application.persistentDataPath);

        // 再尝试外部存储
        if (uEmuera.Utils.HasAndroidAllFilesAccess())
        {
            GetList("/storage/emulated/0/emuera");
            GetList("/storage/emulated/0/era");
            GetList("/storage/emulated/1/emuera");
            GetList("/storage/emulated/1/era");
            GetList("/sdcard/emuera");
            GetList("/sdcard/era");
        }
        else
        {
            uEmuera.Utils.RequestAndroidAllFilesAccess();
            // 权限请求后，下次启动时重新扫描外部存储
        }
#endif

#if UNITY_EDITOR
        var main_entry = Object.FindAnyObjectByType<MainEntry>();
        if(!string.IsNullOrEmpty(main_entry.era_path))
            GetList(main_entry.era_path);
#endif

        setting_.SetActive(true);
    }

    // Android SAF回调: treeUri||displayPath||game1||game2||...
    void OnFolderPicked(string result)
    {
        Log.ForContext("Tag", "Android").Information("OnFolderPicked: {Result}", result ?? "NULL");
        var parts = result.Split(new[] { "||" }, System.StringSplitOptions.None);
        Log.ForContext("Tag", "Android").Information("OnFolderPicked parts: {Count}", parts.Length);
        for (int i = 0; i < parts.Length && i < 5; i++)
            Log.ForContext("Tag", "Android").Information("  part[{I}] = {Val}", i, parts[i].Length > 100 ? parts[i].Substring(0, 100) : parts[i]);

        string displayPath = parts.Length > 1 ? parts[1] : result;
        UnityEngine.PlayerPrefs.SetString("last_picked_path", displayPath);
        ClearList();

        int added = 0;
        for (int i = 2; i < parts.Length; i++)
        {
            if (string.IsNullOrEmpty(parts[i]) || parts[i].StartsWith("ERROR")) continue;
            Log.ForContext("Tag", "Android").Information("AddItem: {Name} path={Path}", parts[i], displayPath);
            AddItem(parts[i], displayPath);
            added++;
        }
        Log.ForContext("Tag", "Android").Information("OnFolderPicked done: added={Added}", added);
    }

    void OnOptionClick()
    {
        var ow = EmueraContent.instance.option_window;
        ow.ShowMenu();
    }

    void AddItem(string folder, string workspace)
    {
        var rrt = item_.transform as UnityEngine.RectTransform;
        var obj = GameObject.Instantiate(item_);
        var text = GenericUtils.FindChildByName<UnityEngine.UI.Text>(obj, "name");
        text.text = folder;
        text = GenericUtils.FindChildByName<UnityEngine.UI.Text>(obj, "path");
        text.text = workspace + "/" + folder;

        GenericUtils.SetListenerOnClick(obj, () =>
        {
            scroll_rect_ = null;
            item_ = null;
            GameObject.Destroy(gameObject);
            //开始游戏
            GenericUtils.StartCoroutine(Run(workspace, folder));
        });

        var rt = obj.transform as UnityEngine.RectTransform;
        var content = scroll_rect_.content;
        rt.SetParent(content);
        rt.localScale = Vector3.one;
        rt.anchorMax = rrt.anchorMax;
        rt.anchorMin = rrt.anchorMin;
        rt.offsetMax = rrt.offsetMax;
        rt.offsetMin = rrt.offsetMin;
        rt.sizeDelta = rrt.sizeDelta;
        rt.localPosition = new Vector2(0, -rt.sizeDelta.y * itemcount_);
        itemcount_ += 1;

        var ih = rt.sizeDelta.y * itemcount_;
        if(ih > content.sizeDelta.y)
        {
            content.sizeDelta = new Vector2(content.sizeDelta.x, ih);
        }
        obj.SetActive(true);
    }

    void GetList(string workspace)
    {
        workspace = uEmuera.Utils.NormalizePath(workspace);
        if(!Directory.Exists(workspace))
            return;
        try
        {
            var paths = Directory.GetDirectories(workspace, "*", SearchOption.TopDirectoryOnly);
            System.Array.Sort(paths);
            foreach(var p in paths)
            {
                var path = uEmuera.Utils.NormalizePath(p);
                if(File.Exists(path + "/emuera.config") || Directory.Exists(path + "/ERB"))
                    AddItem(Path.GetFileName(path), workspace);
            }
        }
        catch { }
    }

    void ClearList()
    {
        if (scroll_rect_ == null) return;
        var content = scroll_rect_.content;
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            var child = content.GetChild(i);
            if (child.name != "Item")
                Destroy(child.gameObject);
        }
        itemcount_ = 0;
    }

    public Text titlebar = null;
    ScrollRect scroll_rect_ = null;
    GameObject item_ = null;
    GameObject setting_ = null;
    GameObject path_btn_ = null;
    GameObject path_input_ = null;
    int itemcount_ = 0;
}
