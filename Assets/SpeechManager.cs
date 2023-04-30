using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class SpeechManager : MonoBehaviour
{
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

    [System.Serializable]
    public class Speech
    {
        public float m_visableTime = 5.0f;
        public List<Phrase> m_phrases = new List<Phrase>();
    };

    [System.Serializable]
    public class Phrase
    {
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

    // Start is called before the first frame update
    void Start()
    {
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
            if (phrase.m_time >= lastEndTime && phrase.m_time < barEndTime)
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
        for (int i = 0; i < m_activebar.activeStreams.Count; ++i)
        {
            ActiveStream activeStream = m_activebar.activeStreams[i];

            for (int j = activeStream.m_activePhrases.Count - 1; j >= 0; j--)
            {
                ActivePhrase activePhrase = activeStream.m_activePhrases[j];
                float timeDiff = activePhrase.m_phrase.m_time - m_activebar.m_currentTime;
                if(timeDiff < 0.0f)
                {
                    // scoring
                    bool thisStreamSelected = i == m_selectedStream;
                    if (thisStreamSelected)
                    {
                        m_score += activePhrase.m_phrase.m_points;
                    }

                    if(activePhrase.m_phrase.m_points > 0)
                    {
                        if(thisStreamSelected)
                        {
                            SetTrackState(eTrackState.Good);
                        }
                        else
                        {
                            SetTrackState(eTrackState.None);
                        }
                    }
                    else
                    {
                        if (thisStreamSelected)
                        {
                            SetTrackState(eTrackState.Bad);
                        }
                        else
                        {
                            SetTrackState(eTrackState.Good);
                        }
                    }
                    activePhrase.m_gameObject.GetComponent<DestroyAfterDelay>().Commence();
                    activeStream.m_activePhrases.RemoveAt(j);

                    if(activePhrase.m_phrase.m_sprite.Length > 0)
                    {
                        m_powerPoint.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/" + activePhrase.m_phrase.m_sprite);
                    }
                }
                else
                {
                    float x = Mathf.Lerp(m_activebar.m_xRange.x, m_activebar.m_xRange.y, timeDiff / m_currentSpeech.m_visableTime);

                    activePhrase.m_gameObject.transform.localPosition = new Vector3(x, activeStream.m_yPosition, -1);
                }
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
            if (m_videoPlayer.isPrepared)
            {
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
        if(Mathf.Abs((float)m_videoPlayer.time - musicTime) > 0.05f)
        {
            Debug.Log("music / vid desync");
            m_videoPlayer.time = (double)musicTime;
        }

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
                break;
            }
            case eTrackState.Good:
            {
                m_scoreAnimator.SetTrigger("ScoreUp");
                break;
            }
            case eTrackState.Bad:
            {
                m_manEffects.SetTrigger("Impact");
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
}
