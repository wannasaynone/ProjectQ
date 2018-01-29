using UnityEngine;
using System.Collections;

public class AnimEvent : MonoBehaviour {

    [SerializeField]
    AudioSource BGM;

	public void PlayBGM()
    {
        BGM.Play();
    }
}
