using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Playables;
using UnityEngine.Video;

public class SpeechManager : MonoBehaviour
{
    public string m_jsonURL = "";
    public string m_jsonData = "";
    public Speech m_currentSpeech = new Speech();
    public Speech m_debugSpeech = new Speech();

    public GameObject m_phrasePrefab;

    public FMODUnity.StudioEventEmitter m_musicEmitter;
    public FMODUnity.StudioEventEmitter m_eughEmitter;

    public GameObject m_pointHigh;
    public GameObject m_pointLow;
    public GameObject m_highBar;
    public GameObject m_lowBar;
    public GameObject m_powerPoint;
    public GameObject m_canvas;

    public Animator m_scoreAnimator;
    public Animator m_manEffects;
    public Animator m_manBobber;

    public VideoPlayer m_videoPlayer;
    public VideoPlayer m_videoPlayerBad;
    public VideoPlayer m_videoPlayerBlank;

    public Material m_manMaterialGood;
    public Material m_manMaterialBad;
    public Material m_manMaterialBlank;

    public static float s_bufferTime = 0.15f;

    [System.Serializable]
    public class Speech
    {
        public float m_visableTime = 5.0f;
        public List<Phrase> m_phrases = new List<Phrase>();
    };

    [System.Serializable]
    public class Phrase
    {
        public float GetTime()
        {
            return m_time + s_bufferTime;
        }
        public float m_time;
        public string m_text;
        public int m_points;
        public string m_sprite;
        public int m_stream;
    };

    private class ActivePhrase
    {
        public Phrase m_phrase;
        public GameObject m_gameObject;
    };
    private class ActiveStream
    {
        public float m_yPosition = 150.0f;
        public List<ActivePhrase> m_activePhrases = new List<ActivePhrase>();
    };

    private class ActiveBar // name?
    {
        public Vector2 m_xRange = new Vector2(-475, 500);
        public float m_currentTime = 0;
        public List <ActiveStream> activeStreams = new List<ActiveStream>();
    };

    private ActiveBar m_activebar = new ActiveBar();

    public enum eTrackState
    {
        None,
        Bad,
        Good,
    }
    public eTrackState m_trackState = eTrackState.Good;
    public int m_selectedStream = 0;
    public int m_score = 0;
    public bool m_hasStarted = false;
    private float m_lastResyncTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetRequest(m_jsonURL, false));

        // initialise the active bar
        int maxStreamNo = 0;
        foreach(Phrase phrase in m_currentSpeech.m_phrases)
        {
            if(phrase.m_stream > maxStreamNo)
            {
                maxStreamNo = phrase.m_stream;
            }
        }

        float startY = 150.0f;
        for (int i = 0; i < maxStreamNo; ++i)
        {
            ActiveStream activeStream = new ActiveStream();
            activeStream.m_yPosition = startY;
            m_activebar.activeStreams.Add(activeStream);

            startY += 70.0f;
        }
        m_activebar.m_currentTime = 0;

        SetActiveStream(0);
    }

    private void AdvanceBar()
    {
        float lastTime = m_activebar.m_currentTime;
        float lastEndTime = lastTime + m_currentSpeech.m_visableTime;

        // since FMOD only works in milliseconds use the deltatime to estimate fractional time but keep us in sync with the music
        m_activebar.m_currentTime += Time.deltaTime;
        int timelinePosition;
        m_musicEmitter.EventInstance.getTimelinePosition(out timelinePosition);
        float musicTime = (float)timelinePosition / 1000.0f;
        float diff = musicTime - m_activebar.m_currentTime;
        m_activebar.m_currentTime += diff * 0.2f;

        float barEndTime = m_activebar.m_currentTime + m_currentSpeech.m_visableTime;

        Vector2 startPosition = new Vector2(0, 0);

        // Create any new phrases
        foreach (Phrase phrase in m_currentSpeech.m_phrases)
        {
            ActiveStream activeStream = m_activebar.activeStreams[phrase.m_stream-1];
            if (phrase.GetTime() >= lastEndTime && phrase.GetTime() < barEndTime)
            {
                // create a new phrase
                ActivePhrase activePhrase = new ActivePhrase();
                activePhrase.m_phrase = phrase;
                activePhrase.m_gameObject = GameObject.Instantiate(m_phrasePrefab, new Vector3(m_activebar.m_xRange.y, activeStream.m_yPosition), Quaternion.identity, m_canvas.transform);
                TMP_Text newText = activePhrase.m_gameObject.GetComponent<TMP_Text>();
                newText.text = phrase.m_text;

                activeStream.m_activePhrases.Add(activePhrase);
            }
        }


        // Move all the phrases
        List<ActivePhrase> toTriggerThisFrame = new List<ActivePhrase>();
        for (int i = 0; i < m_activebar.activeStreams.Count; ++i)
        {
            ActiveStream activeStream = m_activebar.activeStreams[i];

            for (int j = activeStream.m_activePhrases.Count - 1; j >= 0; j--)
            {
                ActivePhrase activePhrase = activeStream.m_activePhrases[j];
                float timeDiff = activePhrase.m_phrase.GetTime() - m_activebar.m_currentTime;
                if(timeDiff < 0.0f)
                {
                    toTriggerThisFrame.Add(activePhrase);
                }
                else
                {
                    float x = Mathf.Lerp(m_activebar.m_xRange.x, m_activebar.m_xRange.y, timeDiff / m_currentSpeech.m_visableTime);

                    activePhrase.m_gameObject.transform.localPosition = new Vector3(x, activeStream.m_yPosition, -1);
                }
            }
        }

        bool hitBad = false;
        bool hitGood = false;
        bool missedBad = false;
        bool missedGood = false; 
        foreach (ActivePhrase activePhrase in toTriggerThisFrame)
        {
            // scoring
            bool thisStreamSelected = activePhrase.m_phrase.m_stream-1 == m_selectedStream;
            if (thisStreamSelected)
            {
                m_score += activePhrase.m_phrase.m_points;
            }


            if (activePhrase.m_phrase.m_points > 0)
            {
                if (thisStreamSelected)
                {
                    hitGood = true;
                }
                else
                {
                    missedGood = true;
                }
            }
            else
            {
                if (thisStreamSelected)
                {
                    hitBad = true;
                }
                else
                {
                    missedBad = true;
                }
            }

            activePhrase.m_gameObject.GetComponent<DestroyAfterDelay>().Commence();
            m_activebar.activeStreams[activePhrase.m_phrase.m_stream - 1].m_activePhrases.Remove(activePhrase);

            if (activePhrase.m_phrase.m_sprite.Length > 0)
            {
                m_powerPoint.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/" + activePhrase.m_phrase.m_sprite);
            }
        }

        if(toTriggerThisFrame.Count > 0)
        {
            if (hitGood || missedBad)
            {
                SetTrackState(eTrackState.Good);
            }
            else if (hitBad)
            {
                SetTrackState(eTrackState.Bad);
            }
            else if (missedGood)
            {
                SetTrackState(eTrackState.None);
            }
        }

    }


    void SetActiveStream(int _activeStream)
    {
        m_selectedStream = _activeStream;
        Color transparentWhite = Color.white;
        transparentWhite.a = 0.2f;

        if (_activeStream == 0)
        {
            m_lowBar.GetComponent<SpriteRenderer>().color = Color.white;
            m_highBar.GetComponent<SpriteRenderer>().color = transparentWhite;

            m_pointLow.SetActive(true);
            m_pointHigh.SetActive(false);
        }
        else
        {
            m_lowBar.GetComponent<SpriteRenderer>().color = transparentWhite;
            m_highBar.GetComponent<SpriteRenderer>().color = Color.white;

            m_pointLow.SetActive(false);
            m_pointHigh.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // wait for the streamed video to be ready
        if(!m_hasStarted)
        {
            if (m_videoPlayer.isPrepared && m_videoPlayerBad.isPrepared && m_videoPlayerBlank.isPrepared
                && m_jsonData.Length > 0)
            {
#if !UNITY_EDITOR
                m_currentSpeech = JsonUtility.FromJson<Speech>(m_jsonData);
#endif
                m_hasStarted = true;
                m_musicEmitter.Play();
            }
            else
            {
                return;
            }
        }

        // keep video in sync with music
        int timelinePosition;
        m_musicEmitter.EventInstance.getTimelinePosition(out timelinePosition);
        float musicTime = (float)timelinePosition / 1000.0f;
        if(m_lastResyncTime > 3f && Mathf.Abs((float)m_videoPlayer.time - musicTime) > 0.2f)
        {
            Debug.Log("music / vid desync");
            m_videoPlayer.time = (double)musicTime;
            m_videoPlayerBad.time = (double)musicTime;
            m_videoPlayerBlank.time = (double)musicTime;
            m_lastResyncTime = 0f;
        }
        m_lastResyncTime += Time.deltaTime;

        // read input
        float vertical = Input.GetAxis("Vertical");
        if(vertical < 0)
        {
            if(m_selectedStream > 0)
            {
                SetActiveStream(0);
            }
        }
        else if(vertical > 0)
        {
            if (m_selectedStream < m_activebar.activeStreams.Count - 1)
            {
                SetActiveStream(1);
            }
        }

        AdvanceBar();
    }

    void SetTrackState(eTrackState _state)
    {
        switch(_state)
        {
            case eTrackState.None:
            {
                m_eughEmitter.Play();
                m_manEffects.SetTrigger("Sweat");
                m_videoPlayer.gameObject.GetComponent<MeshRenderer>().material = m_manMaterialBlank;
                break;
            }
            case eTrackState.Good:
            {
                m_scoreAnimator.SetTrigger("ScoreUp");
                m_videoPlayer.gameObject.GetComponent<MeshRenderer>().material = m_manMaterialGood;
                break;
            }
            case eTrackState.Bad:
            {
                m_manEffects.SetTrigger("Impact");
                m_videoPlayer.gameObject.GetComponent<MeshRenderer>().material = m_manMaterialBad;
                break;
            }
        }

        // don't change audio if state is the same
        if (m_trackState == _state)
            return;
        m_trackState = _state;


        FMOD.Studio.EventDescription eventDesc = FMODUnity.RuntimeManager.GetEventDescription(m_musicEmitter.EventReference);
        if (eventDesc.isValid())
        {
            FMOD.Studio.PARAMETER_DESCRIPTION param;
            string paramNane = "DialogGoodBad";
            eventDesc.getParameterDescriptionByName(paramNane, out param);
            m_musicEmitter.EventInstance.setParameterByID(param.id, (int)m_trackState);
        }
    }


    IEnumerator GetRequest(string uri, bool _extraGame)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    m_jsonData = webRequest.downloadHandler.text;
                    break;
            }
        }
    }
}
