using System;
using System.Collections.Generic;

using UnityEngine;

public class RecordingMetadata
{
    private long ticksSinceDateBegin;
    public long GetTicks() { return ticksSinceDateBegin; }

    private DateTime dateTime;
    public DateTime GetDateTime() { return dateTime; }
    private string sceneName;
    public string GetSceneName() { return sceneName; }

    private XREchoConfig config;

    public RecordingMetadata()
    {
        config = XREchoConfig.GetInstance();

        sceneName = config.GetCurrentScene();

        dateTime = DateTime.Now;
        ticksSinceDateBegin = Utils.GetTicksSinceDateBegin(dateTime);

        //Debug.Log("now = " + dateTime + ", elapsedTicks (100ns) = " + ticksSinceDateBegin);
    }

    public string GetDate()
    {
        return dateTime.ToString(config.dateFormat);
    }
}
