using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogMarkers : MonoBehaviour
{
    FMOD.Studio.EVENT_CALLBACK _musicFmodCallback;
    FMOD.Studio.EventInstance _musicEventInstance;


    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT FMODEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, System.IntPtr instance, System.IntPtr parameterPtr)
    {
        if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
        {
            var parameter = (FMOD.Studio.TIMELINE_MARKER_PROPERTIES)System.Runtime.InteropServices.Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_MARKER_PROPERTIES));
            UnityEngine.Debug.LogFormat("Marker: {0}", (string)parameter.name);
            SpeechManager man = GameObject.Find("SpeechManager").GetComponent<SpeechManager>();

            man.m_debugSpeech.m_phrases.Add(new SpeechManager.Phrase { m_time = ((float)parameter.position)/1000f, m_text = (string)parameter.name, m_points = 0 });
        }

        return FMOD.RESULT.OK;
    }

    // Start is called before the first frame update
    void Start()
    {
        FMOD.Studio.EventDescription desc = FMODUnity.RuntimeManager.GetEventDescription("event:/Music Stage 2");
        desc.createInstance(out _musicEventInstance);

        _musicFmodCallback = new FMOD.Studio.EVENT_CALLBACK(FMODEventCallback);

        _musicEventInstance.setCallback(_musicFmodCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT | FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER);

        _musicEventInstance.start();
    }

    // Update is called once per frame
    void Update()
    {

    }
}