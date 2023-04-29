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
        public float m_yPosition = 10.0f;
        public List<ActivePhrase> m_activePhrases = new List<ActivePhrase>();
    };

    private class ActiveBar // name?
    {
        public Vector2 m_xRange = new Vector2(-50, 50);
        public float m_currentTime = -100;
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

    // Start is called before the first frame update
    void Start()
    {
        m_musicEmitter.Play();
        m_debugSpeech.m_streams.Add(new Stream());


        // initialise the active bar
        float startY = 10.0f;
        for (int i = 0; i < m_currentSpeech.m_streams.Count; ++i)
        {
            Stream stream = m_currentSpeech.m_streams[i];
            ActiveStream activeStream = new ActiveStream();
            activeStream.m_yPosition = startY;
            m_activebar.activeStreams.Add(activeStream);

            startY += 10.0f;
        }
        m_activebar.m_currentTime = - m_currentSpeech.m_visableTime;

    }

    private void AdvanceBar()
    {
        float lastTime = m_activebar.m_currentTime;
        float lastEndTime = lastTime + m_currentSpeech.m_visableTime;

        m_activebar.m_currentTime += Time.deltaTime; // TODO do we want to use absolute start and end times?
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
                    activePhrase.m_gameObject = GameObject.Instantiate(m_phrasePrefab, new Vector3(m_activebar.m_xRange.y, activeStream.m_yPosition), Quaternion.identity);
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
            foreach (ActivePhrase activePhrase in activeStream.m_activePhrases)
            {
                float x = Mathf.Lerp(m_activebar.m_xRange.x, m_activebar.m_xRange.y, (activePhrase.m_phrase.m_time - m_activebar.m_currentTime) / m_currentSpeech.m_visableTime);
                activePhrase.m_gameObject.transform.position = new Vector3(x, activeStream.m_yPosition);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
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
