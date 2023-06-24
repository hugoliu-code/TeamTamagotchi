using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuddyManager : MonoBehaviour
{
    [SerializeField] float happiness = 1f; // value between 0 and 1
    [SerializeField] Animator mouthAnim;
    public bool isTalking = false;
    private Emotion currentEmotion = Emotion.happy;
    enum Emotion
    {
        happy,
        neutral,
        sad
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHappiness();
        UpdateAnimation();
    }

    void UpdateAnimation()
    {


        if (isTalking)
        {
            mouthAnim.Play("Talking");
            return;
        }

        if (currentEmotion == Emotion.happy)
        {
            mouthAnim.Play("Happy");
        }
        else if (currentEmotion == Emotion.neutral)
        {
            mouthAnim.Play("Neutral");
        }
        else if (currentEmotion == Emotion.sad)
        {
            mouthAnim.Play("Sad");
        }
    }

    void UpdateHappiness()
    {
        if (happiness < 0.33f) {
            currentEmotion = Emotion.sad;
        }
        else if (happiness >= 0.33f && happiness < 0.66f)
        {
            currentEmotion = Emotion.neutral;
        }
        else if(happiness >= 0.66f)
        {
            currentEmotion = Emotion.happy;
        }

        if (happiness < 0)
        {
            happiness = 0;
        }

        if (happiness > 1)
        {
            happiness = 1;
        }
    }
}
