using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    [SerializeField] AudioSource footStep;
    [SerializeField] AudioClip clip;

    private void Awake()
    {
        if (footStep == null)
        {
            footStep = GetComponent<AudioSource>();
        }
    }

    private void RabbitStep()
    {
        footStep.PlayOneShot(clip);
    }
    private void TurtleSwim()
    {
        footStep.PlayOneShot(clip);
    }
}
