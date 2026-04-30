using UnityEngine;

// 适配圆角/刘海屏：找到根 Canvas 加安全边距
public class SafeArea : MonoBehaviour
{
    public int extraPadding = 32; // 额外边距(px)，圆角屏需要比系统 safeArea 更大
    private Rect lastSafeArea;
    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        ApplySafeArea();
    }

    void Update()
    {
        if (lastSafeArea != Screen.safeArea)
            ApplySafeArea();
    }

    void ApplySafeArea()
    {
        var safe = Screen.safeArea;
        lastSafeArea = safe;

        float w = Screen.width;
        float h = Screen.height;
        float left   = Mathf.Max(safe.xMin, extraPadding) / w;
        float right  = Mathf.Min(safe.xMax, w - extraPadding) / w;
        float bottom = Mathf.Max(safe.yMin, extraPadding) / h;
        float top    = Mathf.Min(safe.yMax, h - extraPadding) / h;

        var target = canvas != null ? canvas.GetComponent<RectTransform>() : GetComponent<RectTransform>();
        target.anchorMin = new Vector2(left, bottom);
        target.anchorMax = new Vector2(right, top);
        target.offsetMin = Vector2.zero;
        target.offsetMax = Vector2.zero;
    }
}
