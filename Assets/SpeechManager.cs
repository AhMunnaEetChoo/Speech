using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Video;
using static System.Net.WebRequestMethods;

public class SpeechManager : MonoBehaviour
{
    public string m_jsonURL = "";
    private string m_jsonData = "";

    public static int m_startingLevel = 0;
    public GameData m_gameData = new GameData();
    public Speech m_currentSpeech = new Speech();
    public Speech m_debugSpeech = new Speech();

    public GameObject m_phrasePrefab;

    private FMOD.Studio.EventInstance m_musicEventInstance;
    public FMODUnity.StudioEventEmitter m_eughEmitter;

    public GameObject m_pointHigh;
    public GameObject m_pointLow;
    public GameObject m_highBar;
    public GameObject m_lowBar;
    public GameObject m_powerPoint;
    public GameObject m_canvas;
    public GameObject m_scoreObject;

    public Animator m_scoreAnimator;
    public Animator m_manEffects;
    public Animator m_manBobber;

    public VideoPlayer m_videoPlayer;
    public VideoPlayer m_videoPlayerBad;
    public VideoPlayer m_videoPlayerBlank;

    public Material m_manMaterialGood;
    public Material m_manMaterialBad;
    public Material m_manMaterialBlank;

    public static float s_bufferTime = 0.10f;

    [System.Serializable]
    public class ScoreData
    {
        public int scored = 300;
        public int scorec = 400;
        public int scoreb = 500;
        public int scorea = 700;
        public int scoreaplus = 800;
    }

    [System.Serializable]
    public class GameData
    {
        public List<Speech> m_speechs = new List<Speech>();
    };

    [System.Serializable]
    public class Speech
    {
        public float m_visableTime = 5.0f;
        public string m_trackName = "event:/Music Stage 1";
        public List<Phrase> m_phrases = new List<Phrase>();
        public ScoreData m_scoreData = new ScoreData();
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
    private bool m_showingScore = false;

    public GameObject[] m_destroyL1Objects;
    public GameObject[] m_destroyL2Objects;

    // Start is called before the first frame update
    void Start()
    {
        //change graphics depending on level
        GameObject[] hideThese = m_destroyL1Objects;
        GameObject[] showThese = m_destroyL2Objects;
        if (SpeechManager.m_startingLevel == 0)
        {
            hideThese = m_destroyL1Objects;
            showThese = m_destroyL2Objects;
        }
        if (SpeechManager.m_startingLevel == 1)
        {
            m_videoPlayer.url = "https://ahmunnaeetchoo.github.io/Speech/Video/ManHead_Court_Good.mp4";
            m_videoPlayerBad.url = "https://ahmunnaeetchoo.github.io/Speech/Video/ManHead_Court_Bad.mp4";
            m_videoPlayerBlank.url = "https://ahmunnaeetchoo.github.io/Speech/Video/ManHead_Business_Blank.mp4";
            hideThese = m_destroyL2Objects;
            showThese = m_destroyL1Objects;
        }

        foreach (GameObject hideObject in hideThese)
        {
            hideObject.SetActive(false);
        }
        foreach (GameObject showObject in showThese)
        {
            showObject.SetActive(true);
        }

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

        float startY = m_lowBar.transform.localPosition.y + 5f;
        for (int i = 0; i < maxStreamNo; ++i)
        {
            ActiveStream activeStream = new ActiveStream();
            activeStream.m_yPosition = startY;
            m_activebar.activeStreams.Add(activeStream);

            startY = m_highBar.transform.localPosition.y + 5f;
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
        m_musicEventInstance.getTimelinePosition(out timelinePosition);
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
        List<ActivePhrase> toSwitchAudioThisFrame = new List<ActivePhrase>();
        for (int i = 0; i < m_activebar.activeStreams.Count; ++i)
        {
            ActiveStream activeStream = m_activebar.activeStreams[i];

            for (int j = activeStream.m_activePhrases.Count - 1; j >= 0; j--)
            {
                ActivePhrase activePhrase = activeStream.m_activePhrases[j];
                float timeDiff = activePhrase.m_phrase.GetTime() - m_activebar.m_currentTime;
                float audioTimeDiff = activePhrase.m_phrase.m_time - m_activebar.m_currentTime;
                if(audioTimeDiff < 0.0f)
                {
                    toSwitchAudioThisFrame.Add(activePhrase);
                }

                if (timeDiff < 0.0f)
                {
                    toTriggerThisFrame.Add(activePhrase);
                }
                else
                {
                    float x = Mathf.LerpUnclamped(m_activebar.m_xRange.x, m_activebar.m_xRange.y, audioTimeDiff / m_currentSpeech.m_visableTime);

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
                    activePhrase.m_gameObject.GetComponent<DestroyAfterDelay>().Commence();
                }
                else
                {
                    missedGood = true;
                    GameObject.Destroy(activePhrase.m_gameObject);
                }
            }
            else
            {
                if (thisStreamSelected)
                {
                    hitBad = true;
                    activePhrase.m_gameObject.GetComponent<DestroyAfterDelay>().Commence();
                }
                else
                {
                    missedBad = true;
                    GameObject.Destroy(activePhrase.m_gameObject);
                }
            }

            m_activebar.activeStreams[activePhrase.m_phrase.m_stream - 1].m_activePhrases.Remove(activePhrase);

            if (activePhrase.m_phrase.m_sprite.Length > 0)
            {
                m_powerPoint.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/" + activePhrase.m_phrase.m_sprite);
            }
        }
        if (toTriggerThisFrame.Count > 0)
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

        bool aboutToHitBad = false;
        bool aboutToHitGood = false;
        bool aboutToMissedBad = false;
        bool aboutToMissedGood = false;
        foreach (ActivePhrase activePhrase in toSwitchAudioThisFrame)
        {
            bool thisStreamSelected = activePhrase.m_phrase.m_stream - 1 == m_selectedStream;
            if (activePhrase.m_phrase.m_points > 0)
            {
                if (thisStreamSelected)
                {
                    aboutToHitGood = true;
                }
                else
                {
                    aboutToMissedGood = true;
                }
            }
            else
            {
                if (thisStreamSelected)
                {
                    aboutToHitBad = true;
                }
                else
                {
                    aboutToMissedBad = true;
                }
            }
        }

        if (toSwitchAudioThisFrame.Count > 0)
        {
            if (aboutToHitGood || aboutToMissedBad)
            {
                SetAudioTrackState(eTrackState.Good);
            }
            else if (aboutToHitBad)
            {
                SetAudioTrackState(eTrackState.Bad);
            }
            else if (aboutToMissedGood)
            {
                SetAudioTrackState(eTrackState.None);
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


    void StartSpeech(Speech _speech)
    {
        m_currentSpeech = _speech;

        // initialise this level's score targets
        ScoreModule scoreModule = m_scoreObject.GetComponent<ScoreModule>();
        scoreModule.m_gradeD = _speech.m_scoreData.scored;
        scoreModule.m_gradeC = _speech.m_scoreData.scorec;
        scoreModule.m_gradeB = _speech.m_scoreData.scoreb;
        scoreModule.m_gradeA = _speech.m_scoreData.scorea;
        scoreModule.m_gradeAplus = _speech.m_scoreData.scoreaplus;

        FMOD.Studio.EventDescription desc = FMODUnity.RuntimeManager.GetEventDescription(m_currentSpeech.m_trackName);
        desc.createInstance(out m_musicEventInstance);
        m_musicEventInstance.start();
        SetTrackState(eTrackState.Good);
        
        m_hasStarted = true;
        m_showingScore = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_showingScore)
        {
            if (Mathf.Abs(Input.GetAxis("Vertical")) > 0f)
            {
                SpeechManager.m_startingLevel = 1;
                SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
            }

            // wait for score to be shown
            return;
        }

        // wait for the streamed video to be ready
        if(!m_hasStarted)
        {
            if (m_videoPlayer.isPrepared && m_videoPlayerBad.isPrepared && m_videoPlayerBlank.isPrepared
                && m_jsonData.Length > 0)
            {
#if !UNITY_EDITOR
                m_gameData = JsonUtility.FromJson<GameData>(m_jsonData);
#endif
                StartSpeech(m_gameData.m_speechs[SpeechManager.m_startingLevel]);
            }
            else
            {
                return;
            }
        }

        // keep videos in sync with music
        int timelinePosition;
        m_musicEventInstance.getTimelinePosition(out timelinePosition);
        float musicTime = (float)timelinePosition / 1000.0f;
        if(m_lastResyncTime > 2f)
        {
            if(Mathf.Abs((float)m_videoPlayer.time - musicTime) > 0.2f)
            {
                m_videoPlayer.time = (double)musicTime;
                m_lastResyncTime = 0f;
                Debug.Log("music / vid desync");
            }
            if (Mathf.Abs((float)m_videoPlayerBad.time - musicTime) > 0.2f)
            {
                m_videoPlayerBad.time = (double)musicTime;
                m_lastResyncTime = 0f;
                Debug.Log("music / vid desync");
            }
            if (Mathf.Abs((float)m_videoPlayerBlank.time - musicTime) > 0.2f)
            {
                m_videoPlayerBlank.time = (double)musicTime;
                m_lastResyncTime = 0f;
                Debug.Log("music / vid desync");
            }
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

        FMOD.Studio.EventDescription musicEventDescription;
        m_musicEventInstance.getDescription(out musicEventDescription);
        int songLength;
        musicEventDescription.getLength(out songLength);
        if(songLength - timelinePosition < (int)(1000f * 3.0f))
        {
            // we have reached the end of the song
            m_scoreObject.GetComponent<ScoreModule>().m_gameEnd = true;
            m_showingScore = true;
        }
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
        m_trackState = _state;

        SetAudioTrackState(m_trackState);
    }

    void SetAudioTrackState(eTrackState _state)
    {
        FMOD.Studio.EventDescription eventDesc;
        m_musicEventInstance.getDescription(out eventDesc);
        if (eventDesc.isValid())
        {
            FMOD.Studio.PARAMETER_DESCRIPTION param;
            string paramNane = "DialogGoodBad";
            eventDesc.getParameterDescriptionByName(paramNane, out param);
            m_musicEventInstance.setParameterByID(param.id, (int)_state);
        }
    }

    public void SetTime(float _newTime)
    {
        int timelinePosition = (int)(_newTime * 1000.0f);
        m_musicEventInstance.setTimelinePosition(timelinePosition);
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
