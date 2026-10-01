using UnityEngine;

/// <summary>
/// Quản lý con trỏ chuột tùy biến (Custom Cursor) cho toàn bộ game:
/// - Tự động nạp Cursor.png và Cursor_Pressed.png.
/// - Đảm bảo texture luôn đạt chuẩn RGBA32, isReadable = true, không có mipmap để tránh lỗi Unity.
/// - Đổi icon sang trạng thái bấm (pressed) khi đè chuột và trở về mặc định khi nhả chuột.
/// - Tự động khởi tạo ngay khi game chạy (DontDestroyOnLoad) trên mọi Scene.
/// </summary>
public class CursorManager : MonoBehaviour
{
    private static CursorManager instance;
    public static CursorManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<CursorManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("CursorManager");
                    instance = go.AddComponent<CursorManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("Cursor Textures")]
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D pressedCursor;
    [SerializeField] private Vector2 hotSpot = Vector2.zero;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

    private Texture2D preparedDefaultCursor;
    private Texture2D preparedPressedCursor;
    private bool isPressed = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        if (instance == null)
        {
            _ = Instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadTexturesIfNull();
        PrepareTextures();
        ApplyDefaultCursor();
    }

    private void Start()
    {
        ApplyDefaultCursor();
    }

    private void Update()
    {
        // Khi ấn chuột trái -> đổi sang con trỏ click
        if (Input.GetMouseButtonDown(0))
        {
            if (!isPressed)
            {
                isPressed = true;
                ApplyPressedCursor();
            }
        }
        // Khi nhả chuột trái -> đổi về con trỏ mặc định
        else if (Input.GetMouseButtonUp(0))
        {
            if (isPressed)
            {
                isPressed = false;
                ApplyDefaultCursor();
            }
        }
    }

    public void ApplyDefaultCursor()
    {
        Texture2D tex = preparedDefaultCursor != null ? preparedDefaultCursor : defaultCursor;
        if (tex != null)
        {
            try
            {
                Cursor.SetCursor(tex, hotSpot, cursorMode);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[CursorManager] SetCursor failed: " + ex.Message);
            }
        }
    }

    public void ApplyPressedCursor()
    {
        Texture2D tex = preparedPressedCursor != null ? preparedPressedCursor : preparedDefaultCursor;
        if (tex != null)
        {
            try
            {
                Cursor.SetCursor(tex, hotSpot, cursorMode);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[CursorManager] SetCursor failed: " + ex.Message);
            }
        }
    }

    public void SetCursorVisibility(bool visible)
    {
        Cursor.visible = visible;
    }

    public void SetCursorLock(CursorLockMode lockMode)
    {
        Cursor.lockState = lockMode;
    }

    private void PrepareTextures()
    {
        preparedDefaultCursor = PrepareCursorTexture(defaultCursor);
        preparedPressedCursor = PrepareCursorTexture(pressedCursor);
    }

    /// <summary>
    /// Đảm bảo texture đạt chuẩn 100% của Unity Cursor API:
    /// Format: RGBA32, isReadable = true, mipmapCount = 1, alphaIsTransparency = true.
    /// </summary>
    private Texture2D PrepareCursorTexture(Texture2D source)
    {
        if (source == null) return null;

        // Nếu texture trên đĩa đã đúng chuẩn RGBA32, readable và không có mipmap thì dùng trực tiếp
        if (source.isReadable && source.format == TextureFormat.RGBA32 && source.mipmapCount <= 1)
        {
            return source;
        }

        // Tạo bản sao qua RenderTexture để ép format về RGBA32 chuẩn chỉnh
        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(source, rt);
        RenderTexture prevActive = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D cursorTex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        cursorTex.alphaIsTransparency = true;
        cursorTex.filterMode = FilterMode.Point;
        cursorTex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        cursorTex.Apply();

        RenderTexture.active = prevActive;
        RenderTexture.ReleaseTemporary(rt);

        return cursorTex;
    }

    private void LoadTexturesIfNull()
    {
#if UNITY_EDITOR
        if (defaultCursor == null)
        {
            defaultCursor = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/Cursor.png");
        }
        if (pressedCursor == null)
        {
            pressedCursor = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/Cursor_Pressed.png");
        }
#endif
    }
}
