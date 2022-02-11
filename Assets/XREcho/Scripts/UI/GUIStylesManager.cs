using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GUIStylesManager : MonoBehaviour
{
    private bool needGUIInit;

    // all needed sprites
    [Header("GUI icons")]
    public Sprite recSprite;
    public Sprite playSprite;
    public Sprite pauseSprite;
    public Sprite forwardSprite;
    public Sprite cameraSprite;
    public Sprite topviewSprite;
    public Sprite stopSprite;
    public Sprite screenshotSprite;
    public Sprite loadingSprite;

    // all styles needed
    [HideInInspector]
    public GUIStyle boxStyle;
    [HideInInspector]
    public GUIStyle uppedMargedBoxStyle;
    [HideInInspector]
    public GUIStyle tabButtonsStyle;
    [HideInInspector]
    public GUIStyle tabButtonsStyleBlocked;
    [HideInInspector]
    public GUIStyle tabButtonsStyleSelected;
    [HideInInspector]
    public GUIStyle tabStyle;
    [HideInInspector]
    public GUIStyle imageButtonStyle;
    [HideInInspector]
    public GUIStyle blockedImageButtonStyle;
    [HideInInspector]
    public GUIStyle screenshotButtonStyle;
    [HideInInspector]
    public GUIStyle bigImageButtonStyle;
    [HideInInspector]
    public GUIStyle labelStyle;
    [HideInInspector]
    public GUIStyle bigLabelStyle;
    [HideInInspector]
    public GUIStyle recLabelStyle;
    [HideInInspector]
    public GUIStyle missingLabelStyle;
    [HideInInspector]
    public GUIStyle noHoverStyle;
    [HideInInspector]
    public GUIStyle toggleStyle;

    // Start is called before the first frame update
    void Start()
    {
        needGUIInit = true;
    }

    private void OnGUI()
    {
        if (needGUIInit)
        {
            initGUIStyles();
            needGUIInit = false;
        }
    }

#if UNITY_EDITOR
    private Sprite autoInitSprite(Sprite sprite, string name)
    {
        if (sprite == null)
        {
            return (Sprite)AssetDatabase.LoadAssetAtPath("Assets/XREcho/UI/" + name + ".png", typeof(Sprite));
        }
        return sprite;
    }

    private void OnValidate() 
    {
        recSprite = autoInitSprite(recSprite, "rec_button");
        playSprite = autoInitSprite(playSprite, "play_button");
        pauseSprite = autoInitSprite(pauseSprite, "pause_button");
        forwardSprite = autoInitSprite(forwardSprite, "fastforward_button");
        cameraSprite = autoInitSprite(cameraSprite, "camera_button");
        topviewSprite = autoInitSprite(topviewSprite, "topview_button");
        stopSprite = autoInitSprite(stopSprite, "stop_button");
        screenshotSprite = autoInitSprite(screenshotSprite, "screenshot_button");
        loadingSprite = autoInitSprite(loadingSprite, "loading_icon");
    }
#endif

    public string LabeledTextField(string title, string thumbnail)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(title, labelStyle);
        string toReturn = GUILayout.TextField(thumbnail);
        GUILayout.EndHorizontal();
        return toReturn;
    }

    public static Texture2D MakeBackgroundTexture(Color col)
    {
        int siz = 2;
        Color[] pix = new Color[siz * siz];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(siz, siz);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void initGUIStyles()
    {
        float bgAlpha = 0.4f;
        float bgWhite = 0.8f;
        float bgHover = bgWhite * 0.65f;
        Color tabHover = new Color(bgHover, bgHover, bgHover, bgAlpha);
        Color tabSelected = new Color(bgWhite, bgWhite, bgWhite, bgAlpha);
        Color tabBlack = new Color(0, 0, 0, bgAlpha);
        Color tabRed = new Color(0.8f, 0, 0, bgAlpha);
        Color tabGrayText = new Color(bgHover, bgHover, bgHover, 1.0f);

        boxStyle = new GUIStyle("box");
        boxStyle.padding = new RectOffset(1, 1, 1, 1);

        uppedMargedBoxStyle = new GUIStyle(boxStyle);
        uppedMargedBoxStyle.margin.top = 8;

        tabButtonsStyle = new GUIStyle("box");
        tabButtonsStyle.margin = new RectOffset(0, 0, 0, 0);
        tabButtonsStyle.normal.background = MakeBackgroundTexture(tabBlack);
        tabButtonsStyle.hover.background = MakeBackgroundTexture(tabHover);

        tabButtonsStyleBlocked = new GUIStyle(tabButtonsStyle);
        tabButtonsStyleBlocked.normal.textColor = tabGrayText;
        tabButtonsStyleBlocked.hover.textColor = tabGrayText;
        tabButtonsStyleBlocked.hover.background = MakeBackgroundTexture(tabBlack);
        tabButtonsStyleBlocked.active.background = MakeBackgroundTexture(tabRed);

        tabButtonsStyleSelected = new GUIStyle(tabButtonsStyle);
        tabButtonsStyleSelected.normal.background = MakeBackgroundTexture(tabSelected);
        tabButtonsStyleSelected.normal.textColor = tabButtonsStyleSelected.hover.textColor;
        tabButtonsStyleSelected.hover.background = MakeBackgroundTexture(tabSelected);


        tabStyle = new GUIStyle("box");
        tabStyle.normal.background = MakeBackgroundTexture(tabSelected);
        tabStyle.margin = new RectOffset(0, 0, 0, 0);
        tabStyle.padding.top = 8;

        imageButtonStyle = new GUIStyle("button");
        int butPadding = 5;
        imageButtonStyle.fixedHeight = imageButtonStyle.lineHeight * 1.1f + 2 * butPadding;
        imageButtonStyle.fixedWidth = imageButtonStyle.fixedHeight * 1.3f;
        imageButtonStyle.normal.textColor = Color.black;
        imageButtonStyle.hover.textColor = Color.black;
        imageButtonStyle.padding = new RectOffset(butPadding, butPadding, butPadding, butPadding);

        blockedImageButtonStyle = new GUIStyle(imageButtonStyle);
        blockedImageButtonStyle.hover.background = blockedImageButtonStyle.normal.background;
        blockedImageButtonStyle.active.background = blockedImageButtonStyle.normal.background;

        screenshotButtonStyle = new GUIStyle(imageButtonStyle);
        screenshotButtonStyle.fixedWidth = 0;

        bigImageButtonStyle = new GUIStyle("button");
        butPadding = 8;
        bigImageButtonStyle.fixedHeight = bigImageButtonStyle.lineHeight * 1.5f + 2 * butPadding;
        bigImageButtonStyle.fixedWidth = bigImageButtonStyle.fixedHeight * 1.5f;
        bigImageButtonStyle.padding = new RectOffset(butPadding, butPadding, butPadding, butPadding);

        noHoverStyle = new GUIStyle("button");
        noHoverStyle.hover.background = noHoverStyle.normal.background;

        toggleStyle = new GUIStyle("toggle");
        toggleStyle.normal.textColor = Color.black;
        toggleStyle.hover.textColor = Color.black;
        toggleStyle.active.textColor = Color.black;

        labelStyle = new GUIStyle("label");
        labelStyle.stretchWidth = false;

        bigLabelStyle = new GUIStyle(labelStyle);
        bigLabelStyle.margin = new RectOffset(10, 10, 6, 6);
        bigLabelStyle.fontSize = 15;

        recLabelStyle = new GUIStyle(labelStyle);
        recLabelStyle.richText = true;
        recLabelStyle.alignment = TextAnchor.MiddleCenter;
        recLabelStyle.margin = new RectOffset(16, 16, 5, 5);

        missingLabelStyle = new GUIStyle(labelStyle);
        int marg = 10;
        missingLabelStyle.normal.textColor = new Color(0.7f, 0, 0, 1.0f);
        missingLabelStyle.margin = new RectOffset(marg, marg, marg, marg);
    }
}
