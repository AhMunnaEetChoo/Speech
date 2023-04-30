using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreModule : MonoBehaviour
{
    public GameObject SpeechManager;
    public float m_scoreDisplay;
    public TMP_Text m_scoreDisplayText;
    public float m_scoreChangeSpeed;

    public bool m_gameEnd;
    public Animator m_manEffects;
    public GameObject m_goodMessage;
    public GameObject m_badMessage;
    public bool m_finalScore;

    public TMP_Text m_goodRankText;
    public TMP_Text m_badRankText;

    public int m_gradeAplus;
    public int m_gradeA;
    public int m_gradeB;
    public int m_gradeC;
    public int m_gradeD;


    void Update()
    {

        if (SpeechManager.GetComponent<SpeechManager>().m_score < m_scoreDisplay)
        {
            m_scoreDisplay -= 1 * (Time.deltaTime * m_scoreChangeSpeed);
        }

        if (SpeechManager.GetComponent<SpeechManager>().m_score > m_scoreDisplay)
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

        if (m_gameEnd == true)
        {
            m_manEffects.SetBool("WalkOff", true);

            if (m_manEffects.GetCurrentAnimatorStateInfo(0).IsName("WalkedOff"))
            {
                m_finalScore = true;
            }

        }

        //Final score display

        if (m_finalScore == true)
        {
            if (m_scoreDisplay > m_gradeC)
            {
                m_goodMessage.SetActive(true);
            }

            if (m_scoreDisplay <= m_gradeC)
            {
                m_badMessage.SetActive(true);
            }

            //RankDisplay

            if (m_scoreDisplay >= m_gradeAplus)
            {
                m_goodRankText.text = string.Format("A+");
                m_badRankText.text = string.Format("A+");
            }

            if (m_scoreDisplay >= m_gradeA && m_scoreDisplay < m_gradeAplus)
            {
                m_goodRankText.text = string.Format("A");
                m_badRankText.text = string.Format("A");
            }

            if (m_scoreDisplay >= m_gradeB && m_scoreDisplay < m_gradeA)
            {
                m_goodRankText.text = string.Format("B");
                m_badRankText.text = string.Format("B");
            }

            if (m_scoreDisplay >= m_gradeC && m_scoreDisplay < m_gradeB)
            {
                m_goodRankText.text = string.Format("C");
                m_badRankText.text = string.Format("C");
            }

            if (m_scoreDisplay >= m_gradeD && m_scoreDisplay < m_gradeC)
            {
                m_goodRankText.text = string.Format("D");
                m_badRankText.text = string.Format("D");
            }

            if (m_scoreDisplay < m_gradeD)
            {
                m_goodRankText.text = string.Format("F");
                m_badRankText.text = string.Format("F");
            }


        }



    }
}
