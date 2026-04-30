package com.xerysherry.uEmuera;

import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import androidx.documentfile.provider.DocumentFile;
import com.unity3d.player.UnityPlayerActivity;

public class FolderPickerActivity extends UnityPlayerActivity
{
    private static final int PICK_FOLDER = 9001;
    private static FolderPickerActivity _instance;

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        _instance = this;
    }

    public static void OpenFolderPicker()
    {
        if (_instance == null) return;
        Intent intent = new Intent(Intent.ACTION_OPEN_DOCUMENT_TREE);
        _instance.startActivityForResult(intent, PICK_FOLDER);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data)
    {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode == PICK_FOLDER && resultCode == RESULT_OK && data != null)
        {
            Uri treeUri = data.getData();
            if (treeUri != null)
            {
                try {
                    getContentResolver().takePersistableUriPermission(treeUri,
                        Intent.FLAG_GRANT_READ_URI_PERMISSION);
                } catch (Exception e) {}

                // 扫描SAF目录找游戏
                StringBuilder result = new StringBuilder();
                try {
                    DocumentFile root = DocumentFile.fromTreeUri(this, treeUri);
                    DocumentFile[] children = root.listFiles();
                    if (children != null) {
                        for (DocumentFile child : children) {
                            if (!child.isDirectory()) continue;
                            String name = child.getName();
                            if (name == null) continue;
                            // 检查子目录里有没有emuera.config或ERB目录
                            DocumentFile[] subFiles = child.listFiles();
                            if (subFiles != null) {
                                boolean hasConfig = false;
                                boolean hasErb = false;
                                for (DocumentFile f : subFiles) {
                                    String fn = f.getName();
                                    if (fn == null) continue;
                                    if (fn.equalsIgnoreCase("emuera.config")) hasConfig = true;
                                    if (fn.equalsIgnoreCase("ERB") && f.isDirectory()) hasErb = true;
                                }
                                if (hasConfig || hasErb) {
                                    if (result.length() > 0) result.append("||");
                                    result.append(name);
                                }
                            }
                        }
                    }
                } catch (Exception e) {
                    result.append("ERROR:").append(e.getMessage());
                }

                // 同时传递URI字符串和扫描结果
                String displayPath = getDisplayPath(treeUri);
                String msg = treeUri.toString() + "||" + displayPath + "||" + result.toString();
                com.unity3d.player.UnityPlayer.UnitySendMessage(
                    "FirstWindow", "OnFolderPicked", msg);
            }
        }
    }

    private static String getDisplayPath(Uri uri)
    {
        String uriStr = uri.toString();
        if (uriStr.contains("primary%3A"))
        {
            String path = uriStr.substring(uriStr.indexOf("primary%3A") + 10);
            return "/storage/emulated/0/" + android.net.Uri.decode(path);
        }
        if (uriStr.contains("%3A"))
        {
            int idx = uriStr.indexOf("%3A");
            String vol = uriStr.substring(uriStr.lastIndexOf("/", idx) + 1, idx);
            String path = android.net.Uri.decode(uriStr.substring(idx + 3));
            return "/storage/" + vol + "/" + path;
        }
        return uriStr;
    }
}
