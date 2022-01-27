using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasManager : MonoBehaviour
{
    static CanvasManager instance;

    public Image playImage;
    public Image recordingImage;
    public Image stopImage;
    public Image pauseImage;

    public TextMeshProUGUI timeText;

    private Image currentImage;
    private Image blinkingImage;

    private string leftDisplay;
    private string rightDisplay;
    private string delimiter;

    void Awake()
    {
        if (instance)
            Debug.LogError("2 UI Managers: singleton design pattern broken");

        instance = this;

        HideImage(playImage);
        HideImage(recordingImage);
        HideImage(stopImage);
        HideImage(pauseImage);

        ShowImage(stopImage);

        SetDelimiter(" / ");
        SetLeftDisplay("....");
        SetRightDisplay("....");
    }

    public static CanvasManager GetInstance()
    {
        return instance;
    }

    void SetDelimiter(string del)
    {
        delimiter = del;
    }

    void SetLeftDisplay(string ld)
    {
        leftDisplay = ld;
        RefreshText();
    }

    void SetRightDisplay(string rd)
    {
        rightDisplay = rd;
        RefreshText();
    }

    string FloatToString(float f, int nbDigits = 2)
    {
        return f.ToString("F" + nbDigits);
    }

    string TimeToString(float time)
    {
        return FloatToString(time) + "s";
    }

    public void SetTotalTime(float totalTime)
    {
        SetRightDisplay(TimeToString(totalTime));
    }

    public void SetCurrentTime(float currentTime)
    {
        SetLeftDisplay(TimeToString(currentTime));
    }

    public void SetCurrentSize(int sizeInBytes)
    {
        string[] suffixes = { "o", "Ko", "Mo", "Go", "To" };
        int index = 0;
        double remainder = (double)sizeInBytes;
        double divider = 1024;

        while (remainder >= divider)
        {
            remainder /= divider;
            index++;
        }

        SetRightDisplay(FloatToString((float) remainder) + suffixes[index]);
    }

    // Update is called once per frame
    void Update()
    {
        if (blinkingImage != null)
        {
            Color color = blinkingImage.color;
            color.a = Mathf.Abs(Time.time * 2 % 2 - 1);
            blinkingImage.color = color;
        } 
    }

    public void StartReplaying()
    {
        BlinkImage(playImage);
    }

    public void StartRecording()
    {
        BlinkImage(recordingImage);
    }

    public void StopRecording()
    {
        ShowImage(stopImage);
    }

    public void StopReplaying()
    {
        ShowImage(stopImage);
    }

    public void PauseReplaying()
    {
        ShowImage(pauseImage);
    }

    void BlinkImage(Image img)
    {
        ShowImage(img);
        blinkingImage = img;
    }

    void ShowImage(Image img)
    {
        if (img == null)
            return;

        if (currentImage != null)
            currentImage.enabled = false;
        img.enabled = true;
        currentImage = img;
    }

    void HideImage(Image img)
    {
        if (img == null)
            return;

        img.enabled = false;
    }

    void RefreshText()
    {
        SetTimeText(leftDisplay + delimiter + rightDisplay);
    }

    void SetTimeText(string text)
    {
        timeText.SetText(text);
    }
}
