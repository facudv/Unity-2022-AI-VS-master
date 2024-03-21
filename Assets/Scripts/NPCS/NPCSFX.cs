using System.Collections.Generic;
using UnityEngine;

public class NPCSFX : MonoBehaviour
{
	public AudioSource src;
	public List<AudioClip> audioClips;

	private List<AudioClip> mySources = new List<AudioClip>();

	public void DamageSound()
	{
		if (mySources.Count > 0)
		{
			PlaySoundPlease();
			mySources.RemoveAt(0);
		}

		else
		{
			mySources.Clear(); //por las dudas
			mySources.AddRange(audioClips);
			RandomizeThisListPlease(mySources);

			PlaySoundPlease();
			mySources.RemoveAt(0);
		}
	}

	private void PlaySoundPlease()
	{
		AudioClip sound = mySources[0];
		src.Stop();
		src.clip = sound;
		src.Play();
	}

	private void RandomizeThisListPlease(List<AudioClip> list)
	{
		System.Random random = new System.Random();
		int listLength = list.Count;
		while (listLength > 1)
		{
			listLength--;

			int k = random.Next(listLength + 1);
			AudioClip value = list[k];
			list[k] = list[listLength];
			list[listLength] = value;
		}
	}
}
