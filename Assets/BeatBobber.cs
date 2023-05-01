using FMOD;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

public class BeatBobber : MonoBehaviour
{
    FMOD.Studio.EVENT_CALLBACK _musicFmodCallback;
    FMOD.Studio.EventInstance _musicEventInstance;
    private GCHandle handleStored;

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT FMODEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, System.IntPtr instance, System.IntPtr parameterPtr)
    {
        if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
        {
            FMOD.Studio.EventInstance eventinstance = new FMOD.Studio.EventInstance(instance);
            IntPtr pointer;
            eventinstance.getUserData(out pointer);
            GCHandle handle = GCHandle.FromIntPtr(pointer);
            Animator userData = handle.Target as Animator;
            userData.SetTrigger("Bob");
        }

        return FMOD.RESULT.OK;
    }
    public void SetMusic(FMOD.Studio.EventInstance instance)
    {
        _musicEventInstance = instance;
        _musicFmodCallback = new FMOD.Studio.EVENT_CALLBACK(FMODEventCallback);

        _musicEventInstance.setCallback(_musicFmodCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT);

        GCHandle handleStored = GCHandle.Alloc(GetComponent<Animator>());
        IntPtr pointer = GCHandle.ToIntPtr(handleStored);
        _musicEventInstance.setUserData(pointer);
    }
    private void OnDestroy()
    {
        if (handleStored.IsAllocated)
        {
            handleStored.Free();
        }
    }
}
