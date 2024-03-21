using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
	public MusicList musicData;

	private List <AudioClip> hiphop = new List<AudioClip>();
	private List <AudioClip> rockpunk = new List<AudioClip>();
	private List <AudioClip> random = new List<AudioClip>();
	private List <AudioClip> policeWins = new List<AudioClip>();
	private List <AudioClip> homieWins = new List<AudioClip>();

	void Awake()
	{
		DestroyOtherManagers();
	}

	private void DestroyOtherManagers()
	{
		GameObject[] musicManagers = GameObject.FindGameObjectsWithTag("Music");

		if (musicManagers.Length > 1)
			Destroy(this.gameObject);

		DontDestroyOnLoad(this.gameObject);
	}

	public void StartChoice(int musicGenre)
	{
		var sceneCameraSource = Camera.main.GetComponent<AudioSource>();
		sceneCameraSource.Stop();

		if (musicGenre == 1)
		{
			if(hiphop.Count > 0)
			{
				sceneCameraSource.clip = hiphop[0];
				hiphop.RemoveAt(0);
			}
			else
			{
				hiphop.AddRange(musicData.hiphopMusic);
				sceneCameraSource.clip = hiphop[0];
				hiphop.RemoveAt(0);
			}
		}

		else if(musicGenre == 2)
		{
			if (rockpunk.Count > 0)
			{
				sceneCameraSource.clip = rockpunk[0];
				rockpunk.RemoveAt(0);
			}
			else
			{
				rockpunk.AddRange(musicData.rockPunkMusic);
				sceneCameraSource.clip = rockpunk[0];
				rockpunk.RemoveAt(0);
			}
		}

		else if(musicGenre == 3)
		{
			if (random.Count > 0)
			{
				sceneCameraSource.clip = random[0];
				random.RemoveAt(0);
			}
			else
			{
				random.AddRange(musicData.randomMusic);
				sceneCameraSource.clip = random[0];
				random.RemoveAt(0);
			}
		}
		
		sceneCameraSource.Play();
	}

	public void NPCAVictory()
	{
		var sceneCameraSource = Camera.main.GetComponent<AudioSource>();
		sceneCameraSource.Stop();

		if (homieWins.Count > 0)
		{
			sceneCameraSource.clip = homieWins[0];
			homieWins.RemoveAt(0);
		}
		else
		{
			homieWins.AddRange(musicData.homiesWinMusic);
			sceneCameraSource.clip = homieWins[0];
			homieWins.RemoveAt(0);
		}

		sceneCameraSource.Play();
	}

	public void NPCBVictory()
	{
		var sceneCameraSource = Camera.main.GetComponent<AudioSource>();
		sceneCameraSource.Stop();

		if (policeWins.Count > 0)
		{
			sceneCameraSource.clip = policeWins[0];
			policeWins.RemoveAt(0);
		}
		else
		{
			policeWins.AddRange(musicData.policeWinMusic);
			sceneCameraSource.clip = policeWins[0];
			policeWins.RemoveAt(0);
		}

		sceneCameraSource.Play();
	}
}
