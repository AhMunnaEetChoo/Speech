using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroScript : MonoBehaviour
{
    private float m_timer = 0.0f;
    public string m_nextScene = "MainScene";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        m_timer += Time.deltaTime;
        if(m_timer > 1.5f && Mathf.Abs(Input.GetAxis("Vertical")) > 0f)
        {
            SceneManager.LoadScene(m_nextScene, LoadSceneMode.Single);
            SpeechManager.m_startingLevel = 0;
        }
    }
}
