using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CustomGUIDropdown
{
    public int SelectedId;
    public bool DropdownIsDisplayed;

    public int MaxHeight;
    public int ListLength;
    public List<string> DropdownList;

    private int ddButtonHeight;
    private Rect dropdownRect;
    private Vector2 scrollPosition;
    private GUIStyle masterButtonStyle;
    private GUIStyle blockedMasterButtonStyle;
    private GUIStyle ddButtonStyle;
    private GUIStyle ddBoxStyle;

    public CustomGUIDropdown(List<string> _dropdownList, int _maxHeight = 200, int defaultId = 0)
    {
        SelectedId = 0;
        DropdownIsDisplayed = false;

        MaxHeight = _maxHeight;
        DropdownList = _dropdownList;
        ListLength = DropdownList.Count;
        if (ListLength == 0)
        {
            DropdownList = new List<string> { "--- empty list ---" };
            ListLength = DropdownList.Count;
        }
        if (defaultId >= 0 && defaultId < ListLength)
        {
            SelectedId = defaultId;
        }

        InitStyles();
    }


    // getters
    public int GetId(string listEntry)
    {
        return DropdownList.IndexOf(listEntry);
    }

    public string GetCurrentEntry()
    {
        return DropdownList[SelectedId];
    }


    // setters
    public bool SetSelectedEntry(string listEntry)
    {
        int id = GetId(listEntry);
        if (id >= 0)
        {
            SelectedId = id;
            return true;
        }
        return false;
    }

    
    // should be called inside the parent onGUI method
    // return the currently selected ID
    public int OnGUI(bool allowDropingMenu)
    {
        if (!allowDropingMenu)
        {
            GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            DropdownIsDisplayed = false;
        }
        if (GUILayout.Button(DropdownList[SelectedId], allowDropingMenu ? masterButtonStyle : blockedMasterButtonStyle))
        {
            if (DropdownIsDisplayed || allowDropingMenu)
            {
                DropdownIsDisplayed = !DropdownIsDisplayed;
            }
        }
        if (Event.current.type == EventType.Repaint)
        {
            dropdownRect = GUILayoutUtility.GetLastRect();
            float siz = dropdownRect.height;
            GUI.Box(new Rect(dropdownRect.x + dropdownRect.width - siz - 5, dropdownRect.y, siz, siz), MakeMasterButtonTexture((int)(siz * 4.0f)), GUIStyle.none);

            dropdownRect.y += dropdownRect.height;
            dropdownRect.height = ddButtonHeight * DropdownList.Count + 2;
        }
        GUI.color = Color.white;

        DropdownOnGUI(); // display a first time under other part of the gui to get click event first

        return SelectedId;
    }
    
    // should be called at the end of the parent onGUI method, so that the dropdown menu appears above
    // the method is called 2 times per parent's onGUI method : once at the right position to capture mouse events (call present in the above OnGUI method), and once at the end so that the dropdown menu apprears on top of the gui
    public void DropdownOnGUI()
    {
        if (!DropdownIsDisplayed)
        {
            return;
        }
        GUILayout.BeginArea(dropdownRect);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, ddBoxStyle, GUILayout.Width(dropdownRect.width), GUILayout.Height(MaxHeight));
        GUILayout.BeginVertical(GUIStyle.none);
        for (int id = 0; id < DropdownList.Count; id++)
        {
            if (GUILayout.Button(DropdownList[id], ddButtonStyle))
            {
                SelectedId = id;
                DropdownIsDisplayed = false;
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }



    // -----------------------------------------------------------------------
    //                        private methods
    // -----------------------------------------------------------------------
    private void InitStyles()
    {
        int padd = 4;
        ddButtonHeight = (int)GUI.skin.button.lineHeight + padd * 2;

        float grayVal = 0.6f;
        Color gray = new Color(grayVal, grayVal, grayVal, 1.0f);
        Color select = new Color(grayVal, grayVal, 1.0f, 1.0f);

        // creating styles
        masterButtonStyle = new GUIStyle("button");
        int marg = GUI.skin.label.margin.top;
        masterButtonStyle.margin = new RectOffset(0, 0, marg, marg);
        masterButtonStyle.alignment = TextAnchor.MiddleLeft;

        blockedMasterButtonStyle = new GUIStyle(masterButtonStyle);
        blockedMasterButtonStyle.hover.background = blockedMasterButtonStyle.normal.background;
        blockedMasterButtonStyle.active.background = blockedMasterButtonStyle.normal.background;

        ddButtonStyle = new GUIStyle("box");
        ddButtonStyle.padding = new RectOffset(0, 0, padd, padd);
        ddButtonStyle.margin = new RectOffset(0, 0, 0, 0);
        ddButtonStyle.fixedHeight = ddButtonHeight;
        ddButtonStyle.normal.background = GUIStylesManager.MakeBackgroundTexture(gray);
        ddButtonStyle.normal.textColor = Color.white;
        ddButtonStyle.hover.background = GUIStylesManager.MakeBackgroundTexture(select);

        ddBoxStyle = new GUIStyle("box");
        ddBoxStyle.margin = new RectOffset(0, 0, 0, 0);
        ddBoxStyle.padding = new RectOffset(1, 1, 1, 1);
    }

    private static Texture2D MakeMasterButtonTexture(int size)
    {
        float trans = 0.7f;
        Color gray = new Color(1, 1, 1, trans);
        Color black = new Color(0, 0, 0, 0);

        Color[] pix = new Color[size * size];
        int i = 0;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (y < size / 3 || y > size * 2 / 3)
                {
                    pix[i] = black;
                }
                else if (size * 2 / 3 - y + Math.Abs(x - size / 2) <= size / 3)
                {
                    pix[i] = gray;
                }
                else
                {
                    pix[i] = black;
                }

                i++;
            }
        }
        Texture2D result = new Texture2D(size, size);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

}
