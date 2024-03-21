using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class NpcsTargetManager : MonoBehaviour
{
	private enum NPCS { A, B };
	private NPCS winner;

	private MusicManager musicManager;
	public HideManager hudManager;
	public CameraWinManager cameraManager;

	[Header("Win Settings")]
	public Light directional;
	public Light [] policeWinLights;
	public GameObject homieWinLights;
	public Animator carAnim;
	public Animator truckAnim;

	[Header("Lider NPCS A")]
	public NPCALeaderFSM NPCS_A_Leader;
	[Header("Todos los NPCS A")]
	public List<NPCAFSM> NPCS_A;
	[Header("Todos los NPCS B")]
	public List<NPCBFSM> NPCS_B;

	[Header("Cada cuanto se reasignan enemigos")]
	public float assignNPCSTimer;

	private float currentAssignNPCSTimer;
	private float deadTimer;
	private float lightTimer;
	private float currentDeadTimer;

	private int maxEnemiesTargetsPerNPCB;
	private int maxEnemiesTargetsPerNPCA;

	private bool leaderChecked;
	private bool onVictory;
	private bool sceneAnimated;

	private List<NPCBFSM> NPCB_OnScene = new List<NPCBFSM>();
	private List<NPCAFSM> NPCA_OnScene = new List<NPCAFSM>();

	void Awake()
	{
		deadTimer = 0.5f;
		maxEnemiesTargetsPerNPCA = 2;
		maxEnemiesTargetsPerNPCB = 2;
		currentAssignNPCSTimer = assignNPCSTimer;

		NPCB_OnScene.AddRange(NPCS_B);
		NPCA_OnScene.AddRange(NPCS_A);
	}

	void Start()
	{
		musicManager = FindObjectOfType<MusicManager>(); //No encontramos una mejor manera de hacerlo, el objeto no se destruye en cambio de escena.
	}

	void Update()
	{
		RefreshDeadTargets();
		NPCSAssign();
		VictoryLights();
	}

	public void StarSimulation()
	{
		StartCoroutine(StartDelay());
	}

	private void NPCSAssign()
	{
		if (onVictory) return;
		currentAssignNPCSTimer += Time.deltaTime;
		if (currentAssignNPCSTimer >= assignNPCSTimer)
		{
			Clear();

			AssignTargetToLeader();
			AssignTargetToNPCA();
			AssignTargetToNPCB();

			currentAssignNPCSTimer = 0;
		}
	}

	private void Clear()
	{
		NPCS_A_Leader.enemyTarget = null;
		NPCS_A_Leader.npcsEnemiesAmount = 0;

		foreach (var npc in NPCS_B)
		{
			npc.npcsEnemiesAmount = 0;
			npc.enemyTarget = null;
		}
		foreach (var npc in NPCS_A)
		{
			npc.npcsEnemiesAmount = 0;
			npc.enemyTarget = null;
		}
	}

	private void RefreshDeadTargets()
	{
		currentDeadTimer += Time.deltaTime;
		if (currentDeadTimer >= deadTimer)
		{
			for (int i = 0; i < NPCS_A.Count; i++)
			{
				if (NPCS_A[i].isDead)
				{
					maxEnemiesTargetsPerNPCA++;
					currentAssignNPCSTimer = assignNPCSTimer; //En caso de muerte, forzamos una reasignacion de targets para que ninguno quede golpeando el abismo existencial.
					NPCS_A.RemoveAt(i);
				}
			}

			for (int i = 0; i < NPCS_B.Count; i++)
			{
				if (NPCS_B[i].isDead)
				{
					maxEnemiesTargetsPerNPCB++;
					currentAssignNPCSTimer = assignNPCSTimer; 
					NPCS_B.RemoveAt(i);
				}
			}

			if (NPCS_A_Leader.isDead && !leaderChecked)
			{
				maxEnemiesTargetsPerNPCA++;
				leaderChecked = true;
			}

			CheckVictory();
			currentDeadTimer = 0;
		}

	}

	private void CheckVictory()
	{
		if (onVictory) return;
		
		if (!NPCB_OnScene.Any(x => !x.isDead))
		{
			NPCS_A_Leader.victory = true;
			foreach (var item in NPCA_OnScene)
				item.victory = true;

			winner = NPCS.A;
			musicManager.NPCAVictory();
			cameraManager.NPCAVictory();
			VictoryHUD();
			onVictory = true;
		}

		if (!NPCA_OnScene.Any(x => !x.isDead) && NPCS_A_Leader.isDead)
		{
			foreach (var item in NPCB_OnScene)
				item.victory = true;

			winner = NPCS.B;
			musicManager.NPCBVictory();
			cameraManager.NPCBVictory();
			VictoryHUD();
			onVictory = true;
		}

	}

	private void VictoryHUD()
	{
		if (winner == NPCS.A)
			hudManager.NPCAVictory();
		else if (winner == NPCS.B)
			hudManager.NPCBVictory();
	}

	private void VictoryLights()
	{
		if (!onVictory) return;
		
		if (winner == NPCS.A)
		{
			lightTimer += Time.deltaTime;
			if (lightTimer >= 0.0f && lightTimer < 0.25f)
				homieWinLights.SetActive(true);
			else if (lightTimer >= 0.25f && lightTimer < 0.5)
				homieWinLights.SetActive(false);
			else
				lightTimer = 0;
		}

		else if (winner == NPCS.B)
		{
			lightTimer += Time.deltaTime;
			
			if (lightTimer >= 0f && lightTimer < 0.5f)
			{
				policeWinLights[1].enabled = false;
				policeWinLights[0].enabled = true;
			}
			else if(lightTimer >= 0.5f && lightTimer < 1.0f)
			{
				policeWinLights[0].enabled = false;
				policeWinLights[1].enabled = true;
			}
			else
				lightTimer = 0;
		}

		if (!sceneAnimated)
		{
			directional.enabled = false;
			carAnim.enabled = true;
			truckAnim.enabled = true;
			sceneAnimated = true;
		}
	}

	private void AssignTargetToLeader()
	{
		if (NPCS_A_Leader.isDead) return;
		GameObject target = SearchNPCBTarget(NPCS_B);
		NPCS_A_Leader.enemyTarget = target;
	}

	private void AssignTargetToNPCA()
	{
		foreach (var npcA in NPCS_A)
		{
			if (npcA.isDead) continue;
			GameObject target = SearchNPCBTarget(NPCS_B);
			npcA.enemyTarget = target;
		}
	}

	private void AssignTargetToNPCB()
	{
		foreach (var npcB in NPCS_B)
		{
			if (npcB.isDead) continue;
			GameObject target;			
			float attackLeaderProbability = Random.value; 
			if (attackLeaderProbability <= 0.3f && NPCS_A_Leader.npcsEnemiesAmount < maxEnemiesTargetsPerNPCA && !NPCS_A_Leader.isDead)
			{
				NPCS_A_Leader.npcsEnemiesAmount++;
				target = NPCS_A_Leader.gameObject;
			}

			else
			{
				if (NPCS_A.Count == 0) //Si murieron todos los npcs A, directamenta atacan al lider.
				target = NPCS_A_Leader.gameObject;
				else   //Sino buscan uno.
				target = SearchNPCATarget(NPCS_A);
			}
			
			npcB.enemyTarget = target;
		}
	}

	private GameObject SearchNPCBTarget(List<NPCBFSM> npcsB)
	{
		if (npcsB.Count <= 0)	return null;
	
		List<NPCBFSM> customList = new List<NPCBFSM>();
		customList.AddRange(npcsB);
		NPCBFSM tempTarget = customList[Random.Range(0, customList.Count)];

		if (tempTarget.npcsEnemiesAmount < maxEnemiesTargetsPerNPCA)
		{
			tempTarget.npcsEnemiesAmount++;
			return tempTarget.gameObject;
		}
		else
		{
			customList.Remove(tempTarget);
			return SearchNPCBTarget(customList);
		}
	}

	private GameObject SearchNPCATarget(List<NPCAFSM> npcsA)
	{
		if (npcsA.Count <= 0) return null;
		
		List<NPCAFSM> customList = new List<NPCAFSM>();
		customList.AddRange(npcsA);
		NPCAFSM tempTarget = customList[Random.Range(0, customList.Count)];

		if (tempTarget.npcsEnemiesAmount < maxEnemiesTargetsPerNPCB)
		{
			tempTarget.npcsEnemiesAmount++;
			return tempTarget.gameObject;
		}
		else
		{
			customList.Remove(tempTarget);
			return SearchNPCATarget(customList);
		}
	}

	IEnumerator StartDelay()
	{
		yield return new WaitForSeconds(1.0f);
		
		NPCS_A_Leader.startStateMachine = true;
		foreach (var item in NPCS_B)
			item.startStateMachine = true;

		foreach (var item in NPCS_A)
			item.startStateMachine = true;
	}
}

