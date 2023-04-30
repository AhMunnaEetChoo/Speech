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

    void Update()
    {

        if (SpeechManager.GetComponent<SpeechManager>().m_score > m_scoreDisplay)
        {
            m_scoreDisplay += 1 * (Time.deltaTime * m_scoreChangeSpeed);
        }

        if (SpeechManager.GetComponent<SpeechManager>().m_score < m_scoreDisplay)
        {
            m_scoreDisplay -= 1 * (Time.deltaTime * m_scoreChangeSpeed);
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
    }
}
