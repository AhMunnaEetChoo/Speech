using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatBobber : MonoBehaviour
{
    FMOD.Studio.EVENT_CALLBACK _musicFmodCallback;
    FMOD.Studio.EventInstance _musicEventInstance;
    private Animator m_animator;

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    FMOD.RESULT FMODEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, System.IntPtr instance, System.IntPtr parameterPtr)
    {
        if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
        {
            m_animator.SetTrigger("Bob");
        }

        return FMOD.RESULT.OK;
    }
    public void SetMusic(FMOD.Studio.EventInstance instance)
    {
        _musicEventInstance = instance;
        _musicFmodCallback = new FMOD.Studio.EVENT_CALLBACK(FMODEventCallback);

        _musicEventInstance.setCallback(_musicFmodCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT);

        m_animator = GetComponent<Animator>();
    }
    // Update is called once per frame
    void Update()
    {

    }
}
