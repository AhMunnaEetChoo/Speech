using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class SpeechManager : MonoBehaviour
{
    public Speech m_currentSpeech = new Speech();
    public Speech m_debugSpeech = new Speech();

    public GameObject m_phrasePrefab;
    public FMODUnity.StudioEventEmitter m_musicEmitter;
    public GameObject m_arrowThing;
    public GameObject m_scoreText;
    public GameObject m_canvas;

    [System.Serializable]
    public class Speech
    {
        public float m_visableTime = 5.0f;
        public List<Stream> m_streams;
        public List<Sprite> m_sprites;
    };

    [System.Serializable]
    public class Phrase
    {
        public float m_time;
        public string m_text;
        public int m_points;
    };

    [System.Serializable]
    public class Stream
    {
        public List<Phrase> m_phrases = new List<Phrase>();
    };

    [System.Serializable]
    public class Sprite
    {

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
    public eTrackState m_trackSTate = eTrackState.Good;
    public int m_selectedStream = 0;
    public int m_score = 0;

    // Start is called before the first frame update
    void Start()
    {
        m_debugSpeech.m_streams.Add(new Stream());
        m_musicEmitter.Play();

        // initialise the active bar
        float startY = 150.0f;
        for (int i = 0; i < m_currentSpeech.m_streams.Count; ++i)
        {
            Stream stream = m_currentSpeech.m_streams[i];
            ActiveStream activeStream = new ActiveStream();
            activeStream.m_yPosition = startY;
            m_activebar.activeStreams.Add(activeStream);

            startY += 70.0f;
        }
        m_activebar.m_currentTime = 0;

    }

    private void AdvanceBar()
    {
        float lastTime = m_activebar.m_currentTime;
        float lastEndTime = lastTime + m_currentSpeech.m_visableTime;

        // since FMOD only works in milliseconds use the deltatime to estimate fractional time but keep us in sync with the music
        m_activebar.m_currentTime += Time.deltaTime;
        int timelinePosition;
        m_musicEmitter.EventInstance.getTimelinePosition(out timelinePosition);
        float estMillisecs = m_activebar.m_currentTime * 1000.0f;
        float estMillisecsFlr = Mathf.Floor(estMillisecs);
        if(estMillisecsFlr > (float)timelinePosition)
        {
            m_activebar.m_currentTime = (float)timelinePosition / 1000.0f;
        }

        float barEndTime = m_activebar.m_currentTime + m_currentSpeech.m_visableTime;

        Vector2 startPosition = new Vector2(0, 0);

        // Create any new phrases
        for(int i = 0 ; i < m_currentSpeech.m_streams.Count; ++i)
        {
            Stream stream = m_currentSpeech.m_streams[i];
            ActiveStream activeStream = m_activebar.activeStreams[i];

            foreach (Phrase phrase in stream.m_phrases)
            {
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
        }

        // Move all the phrases
        for (int i = 0; i < m_currentSpeech.m_streams.Count; ++i)
        {
            Stream stream = m_currentSpeech.m_streams[i];
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
                        m_scoreText.GetComponent<TextMeshPro>().text = m_score.ToString();
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
                    activePhrase.m_gameObject.GetComponent<TextMeshPro>().color = Color.white;
                    activePhrase.m_gameObject.GetComponent<DestroyAfterDelay>().Commence();
                    activeStream.m_activePhrases.RemoveAt(j);

                }
                else
                {
                    float x = Mathf.Lerp(m_activebar.m_xRange.x, m_activebar.m_xRange.y, timeDiff / m_currentSpeech.m_visableTime);

                    activePhrase.m_gameObject.transform.localPosition = new Vector3(x, activeStream.m_yPosition, -1);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // read input
        float vertical = Input.GetAxis("Vertical");
        if(vertical < 0)
        {
            if(m_selectedStream > 0)
            {
                m_selectedStream--;
            }
        }
        else if(vertical > 0)
        {
            if (m_selectedStream < m_currentSpeech.m_streams.Count - 1)
            {
                m_selectedStream++;
            }
        }

        // set arrow position
        float arrowY = m_activebar.activeStreams[m_selectedStream].m_yPosition;
        m_arrowThing.transform.localPosition = new Vector3(m_arrowThing.transform.localPosition.x, arrowY);

        AdvanceBar();
    }

    void SetTrackState(eTrackState _state)
    {
        if(m_trackSTate == _state)
            return;
        m_trackSTate = _state;

        FMOD.Studio.EventDescription eventDesc = FMODUnity.RuntimeManager.GetEventDescription(m_musicEmitter.EventReference);
        if (eventDesc.isValid())
        {
            FMOD.Studio.PARAMETER_DESCRIPTION param;
            string paramNane = "DialogGoodBad";
            eventDesc.getParameterDescriptionByName(paramNane, out param);
            m_musicEmitter.EventInstance.setParameterByID(param.id, (int)m_trackSTate);
        }
    }
}
