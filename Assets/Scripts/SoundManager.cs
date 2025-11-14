using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
	public AudioClip buttonSound;/* 
	public AudioClip fiveSecondWarningSound;
	public AudioClip myTurnSound;
	public AudioClip[] tickSounds;
	public AudioClip[] cardPickupSounds;
	public AudioClip[] cardDropSounds;
	public AudioClip[] cardSlideSounds;
	*/
	
	public AudioSource soundSource;
	public AudioSource tickSource;
	
	public static SoundManager instance;
	void Awake()
	{
		instance = this;
	}
	
	public void PlaySound(AudioClip sound, float volumeFactor = 1f)
	{
		/* if(LocalInterface.instance.menu.soundOn && (Application.isFocused || (!Application.isFocused && !LocalInterface.instance.menu.muteOnFocusLost)))
		{
			soundSource.PlayOneShot(sound, LocalInterface.instance.menu.soundVolume * volumeFactor);
		} */
	}
	/*
	public void PlayCardPickupSound()
	{
		PlaySound(cardPickupSounds[Random.Range(0, cardPickupSounds.Length)], 0.5f);
	}
	
	public void PlayCardDropSound()
	{
		PlaySound(cardDropSounds[Random.Range(0, cardDropSounds.Length)], 0.5f);
	}
	
	public void PlayCardSlideSound()
	{
		PlaySound(cardSlideSounds[Random.Range(0, cardSlideSounds.Length)], 0.5f);
	}
	
	private float lastTickSoundTime = 0;
	private int tickSoundIndex = 0;
	
	public void PlayTickSound()
	{
		if(LocalInterface.instance.menu.soundOn && (Application.isFocused || (!Application.isFocused && !LocalInterface.instance.menu.muteOnFocusLost)))
		{
			if(Time.time - lastTickSoundTime > 0.2f)
			{
				tickSoundIndex = 0;
			}
			lastTickSoundTime = Time.time;
			tickSource.pitch = 1f + 0.05f * tickSoundIndex;
			tickSource.PlayOneShot(tickSounds[Random.Range(0,tickSounds.Length)], LocalInterface.instance.menu.soundVolume * 0.5f);
			tickSoundIndex++;
		}
	}
	
	public void PlayFiveSecondWarningSound()
	{
		if(LocalInterface.instance.menu.fiveSecondWarning)
		{
			soundSource.PlayOneShot(fiveSecondWarningSound, LocalInterface.instance.menu.soundVolume);
		}
	} */
}
