using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreModule : MonoBehaviour
{
    public GameObject speechManager;
    public float m_scoreDisplay;
    public TMP_Text m_scoreDisplayText;
    public float m_scoreChangeSpeed;

    public bool m_gameEnd;
    public bool m_hasTriggered = false;
    public Animator m_manEffects;
    public GameObject m_goodMessageL1;
    public GameObject m_goodMessageL2;
    public GameObject m_badMessageL1;
    public GameObject m_badMessageL2;
    public bool m_finalScore;

    public TMP_Text m_goodRankTextL1;
    public TMP_Text m_badRankTextL1;
    public TMP_Text m_goodRankTextL2;
    public TMP_Text m_badRankTextL2;

    public int m_gradeAplus;
    public int m_gradeA;
    public int m_gradeB;
    public int m_gradeC;
    public int m_gradeD;

    private FMOD.Studio.EventInstance m_shuffleEventInstance;
    private FMOD.Studio.EventInstance m_scoreScreenEventInstance;

    private enum eFinalRank
    {
        F,
        D,
        C,
        B,
        A,
        Aplus
    };
    private eFinalRank m_finalRank;

    void Update()
    {

        if (speechManager.GetComponent<SpeechManager>().m_score < m_scoreDisplay)
        {
            m_scoreDisplay -= 1 * (Time.deltaTime * m_scoreChangeSpeed);
        }

        if (speechManager.GetComponent<SpeechManager>().m_score > m_scoreDisplay)
        {
            m_scoreDisplay += 1 * (Time.deltaTime * m_scoreChangeSpeed);
        }

        if (m_scoreDisplay < 0)
        {
            m_scoreDisplay = 0;
        }

        //Score display with 5 leading zeros

        if (m_scoreDisplay <= 9)
        {
            m_scoreDisplayText.text = string.Format("<mspace=0.55em>0000{0:0}</mspace>", m_scoreDisplay);
        }

        if (m_scoreDisplay <= 99 && m_scoreDisplay > 9)
        {
            m_scoreDisplayText.text = string.Format("<mspace=0.55em>000{0:0}</mspace>", m_scoreDisplay);
        }

        if (m_scoreDisplay <= 999 && m_scoreDisplay > 99)
        {
            m_scoreDisplayText.text = string.Format("<mspace=0.55em>00{0:0}</mspace>", m_scoreDisplay);
        }

        if (m_scoreDisplay <= 9999 && m_scoreDisplay > 999)
        {
            m_scoreDisplayText.text = string.Format("<mspace=0.55em>0{0:0}</mspace>", m_scoreDisplay);
        }

        if (m_scoreDisplay > 9999)
        {
            m_scoreDisplayText.text = string.Format("<mspace=0.55em>{0:0}</mspace>", m_scoreDisplay);
        }


        //Game End anim

        if (m_gameEnd == true && !m_finalScore)
        {
            if (!m_hasTriggered)
            {
                FMOD.Studio.EventDescription desc = FMODUnity.RuntimeManager.GetEventDescription("event:/Shuffling off");
                desc.createInstance(out m_shuffleEventInstance);
                m_shuffleEventInstance.start();
            }

            m_manEffects.SetBool("WalkOff", true);

            if (m_manEffects.GetCurrentAnimatorStateInfo(0).IsName("WalkedOff"))
            {
                FMOD.Studio.EventDescription desc = FMODUnity.RuntimeManager.GetEventDescription("event:/Score Screen");
                desc.createInstance(out m_scoreScreenEventInstance);
                m_scoreScreenEventInstance.start();

                m_finalScore = true;
            }

            m_hasTriggered = true;
        }

        //Final score display

        if (m_finalScore == true)
        {
            if (m_scoreDisplay > m_gradeC && SpeechManager.m_startingLevel == 0)
            {
                m_goodMessageL1.SetActive(true);
            }

            if (m_scoreDisplay <= m_gradeC && SpeechManager.m_startingLevel == 0)
            {
                m_badMessageL1.SetActive(true);
            }

            if (m_scoreDisplay > m_gradeC && SpeechManager.m_startingLevel == 1)
            {
                m_goodMessageL2.SetActive(true);
            }

            if (m_scoreDisplay <= m_gradeC && SpeechManager.m_startingLevel == 1)
            {
                m_badMessageL2.SetActive(true);
            }

            //RankDisplay

            if (m_scoreDisplay >= m_gradeAplus)
            {
                m_finalRank = eFinalRank.Aplus;
                m_goodRankTextL1.text = string.Format("A+");
                m_badRankTextL1.text = string.Format("A+");
                m_goodRankTextL2.text = string.Format("A+");
                m_badRankTextL2.text = string.Format("A+");
            }

            if (m_scoreDisplay >= m_gradeA && m_scoreDisplay < m_gradeAplus)
            {
                m_finalRank = eFinalRank.A;
                m_goodRankTextL1.text = string.Format("A");
                m_badRankTextL1.text = string.Format("A");
                m_goodRankTextL2.text = string.Format("A");
                m_badRankTextL2.text = string.Format("A");
            }

            if (m_scoreDisplay >= m_gradeB && m_scoreDisplay < m_gradeA)
            {
                m_finalRank = eFinalRank.B;
                m_goodRankTextL1.text = string.Format("B");
                m_badRankTextL1.text = string.Format("B");
                m_goodRankTextL2.text = string.Format("B");
                m_badRankTextL2.text = string.Format("B");
            }

            if (m_scoreDisplay >= m_gradeC && m_scoreDisplay < m_gradeB)
            {
                m_finalRank = eFinalRank.C;
                m_goodRankTextL1.text = string.Format("C");
                m_badRankTextL1.text = string.Format("C");
                m_goodRankTextL2.text = string.Format("C");
                m_badRankTextL2.text = string.Format("C");
            }

            if (m_scoreDisplay >= m_gradeD && m_scoreDisplay < m_gradeC)
            {
                m_finalRank = eFinalRank.D;
                m_goodRankTextL1.text = string.Format("D");
                m_badRankTextL1.text = string.Format("D");
                m_goodRankTextL2.text = string.Format("D");
                m_badRankTextL2.text = string.Format("D");
            }

            if (m_scoreDisplay < m_gradeD)
            {
                m_finalRank = eFinalRank.F;
                m_goodRankTextL1.text = string.Format("F");
                m_badRankTextL1.text = string.Format("F");
                m_goodRankTextL2.text = string.Format("F");
                m_badRankTextL2.text = string.Format("F");
            }

            int audioScoreParam = 0;
            switch(m_finalRank)
            {
                case eFinalRank.F:
                case eFinalRank.D:
                case eFinalRank.C:
                    audioScoreParam = 0;
                    break;
                case eFinalRank.B:
                case eFinalRank.A:
                    audioScoreParam = 1;
                    break;
                case eFinalRank.Aplus:
                    audioScoreParam = 2;
                    break;
            }

            FMOD.Studio.EventDescription eventDesc;
            m_scoreScreenEventInstance.getDescription(out eventDesc);
            FMOD.Studio.PARAMETER_DESCRIPTION param;
            string paramNane = "ScoreBadOkGood";
            eventDesc.getParameterDescriptionByName(paramNane, out param);
            m_scoreScreenEventInstance.setParameterByID(param.id, audioScoreParam);
        }
    }
    void OnDestroy()
    {
        if(m_scoreScreenEventInstance.isValid())
        {
            m_scoreScreenEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        if(m_shuffleEventInstance.isValid())
        {
            m_shuffleEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }
}
