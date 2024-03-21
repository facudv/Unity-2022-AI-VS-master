using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HideManager : MonoBehaviour
{
	public NpcsTargetManager AIManager;
	public CameraController cameraController;
	public CameraZoom cameraZoom;
	private MusicManager musicManager;

	public GameObject controllerHUD;
	public GameObject musicHUD;
	public GameObject victoryBG;

	public Image homiesWin;
	public Image policeWin;
	public Image hiphopMarked;
	public Image rockMarked;
	public Image randomMarked;

	private bool onVictory;
	private bool musicAlreadySet;

	void Start()
	{
		Cursor.visible = false;
		hiphopMarked.enabled = false;
		rockMarked.enabled = false;
		randomMarked.enabled = false;
		musicManager = FindObjectOfType<MusicManager>(); //No encontramos una mejor manera de hacerlo, el objeto no se destruye en cambio de escena.
	}

	void Update()
	{
		ShowHideUI();
		MusicSelection();
	}

	private void MusicSelection()
	{
		if (musicAlreadySet) return;

		if(Input.GetKeyDown(KeyCode.A))
		{
			musicManager.StartChoice(1);
			hiphopMarked.enabled = true;
			musicAlreadySet = true;
			StartCoroutine(StartEverything());
		}

		else if (Input.GetKeyDown(KeyCode.S))
		{
			musicManager.StartChoice(2);
			rockMarked.enabled = true;
			musicAlreadySet = true;
			StartCoroutine(StartEverything());
		}

		else if (Input.GetKeyDown(KeyCode.D))
		{
			musicManager.StartChoice(3);
			randomMarked.enabled = true;
			musicAlreadySet = true;
			StartCoroutine(StartEverything());
		}

		else if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	private void ShowHideUI()
	{
		if (onVictory || !musicAlreadySet) return;

		if (Input.GetKeyDown(KeyCode.X))
		{
			if (controllerHUD.activeInHierarchy)
				controllerHUD.SetActive(false);
			else
				controllerHUD.SetActive(true);
		}
	}

	public void NPCAVictory()
	{
		onVictory = true;
		controllerHUD.SetActive(false);
		victoryBG.SetActive(true);
		homiesWin.enabled = true;
	}

	public void NPCBVictory()
	{
		onVictory = true;
		controllerHUD.SetActive(false);
		victoryBG.SetActive(true);
		policeWin.enabled = true;
	}

	IEnumerator StartEverything()
	{
		yield return new WaitForSeconds(0.5f);
	
		musicHUD.SetActive(false);
		AIManager.StarSimulation();
		cameraZoom.StartSimulation();
		cameraController.StartSimulation();
	}
}
