using MyFSM;
using System.Collections.Generic;
using UnityEngine;

public class NPCBFSM : MonoBehaviour
{
	private enum NPCBStates { IDLE, PATHFINDING, ATTACK, BLOCK, TAKELIFE, CATCHENEMY, DIE , WIN, DANCE}
	private EventFSM<NPCBStates> myFSM;
	private Roulette rouletteSelection;

	public NpcController myController;
	public NPCPathFindingData pathFindingManager;
	public NPCAnimator animatorManager;
	public NPCVFX myVFX;
	public NPCSFX mySFX;

	[HideInInspector] public bool startStateMachine;
	[HideInInspector] public bool lifeAlreadyTaked;
	[HideInInspector] public bool isDead;
	[HideInInspector] public bool victory;

	private bool onFightZone;
	private bool blockingState;
	private bool takingLife;
	private bool decisionsChanged;


	[HideInInspector] public float maxLife = 10;
	[HideInInspector] public float limitLife = 7;
	[HideInInspector] public float blockResistance = 1.0f;
	[HideInInspector] public float currentblockResistance;
	[HideInInspector] public float currentLife;

	private float attackOrBlockTimer = 0.5f;
	private float currentAttackOrBlockTimer;
	private float minDistanceToTarget = 1.35f;	
	private float percentToHealth = 50.0f;

	private int blockRepeat;
	private int maxBlocks = 3;

	[Header("Roulette Wheel Selection")]
	public List<string> posibleDecisions;
	
	[Range(0, 100)] public List<int> decisionsWeights;
	[Range(0, 100)] public List<int> secondDecisionsWeights;

	[HideInInspector] public int npcsEnemiesAmount;

	[HideInInspector] public GameObject enemyTarget;

	public Transform danceZone;
	public GameObject lifeCan;

	void Awake()
	{
		currentLife = maxLife;
		CreateAndSetMyStates();
		rouletteSelection = new Roulette(posibleDecisions, decisionsWeights);
		lifeCan.SetActive(false);
	}

	void Update()
	{
		if (!startStateMachine) return;
		myFSM.Update();
	}

	private void CreateAndSetMyStates()
	{
		#region Create States
		State<NPCBStates> idle = new State<NPCBStates>("IDLE");
		State<NPCBStates> pathfinding = new State<NPCBStates>("PATHFINDING");
		State<NPCBStates> attack = new State<NPCBStates>("ATTACK");
		State<NPCBStates> block = new State<NPCBStates>("BLOCK");
		State<NPCBStates> takelife = new State<NPCBStates>("TAKELIFE");
		State<NPCBStates> catchEnemy = new State<NPCBStates>("CATCHENEMY");
		State<NPCBStates> die = new State<NPCBStates>("DIE");
		State<NPCBStates> win = new State<NPCBStates>("WIN");
		State<NPCBStates> dance = new State<NPCBStates>("DANCE");

		StateConfigurer.Create(idle).SetTransition(NPCBStates.PATHFINDING, pathfinding)
									.SetTransition(NPCBStates.ATTACK, attack)
									.SetTransition(NPCBStates.BLOCK, block)
									.SetTransition(NPCBStates.TAKELIFE, takelife)								
									.SetTransition(NPCBStates.WIN, win)
									.SetTransition(NPCBStates.DIE, die)
									.Done();

		StateConfigurer.Create(pathfinding).SetTransition(NPCBStates.IDLE, idle)
										   .SetTransition(NPCBStates.DIE, die)
										   .Done();

		StateConfigurer.Create(attack).SetTransition(NPCBStates.IDLE, idle)
									  .SetTransition(NPCBStates.CATCHENEMY, catchEnemy)
									  .SetTransition(NPCBStates.WIN, win)
									  .SetTransition(NPCBStates.TAKELIFE, takelife)
									  .SetTransition(NPCBStates.DIE, die)
									  .Done();

		StateConfigurer.Create(block).SetTransition(NPCBStates.IDLE, idle)
									 .SetTransition(NPCBStates.WIN, win)
									 .SetTransition(NPCBStates.DIE, die)
									 .SetTransition(NPCBStates.TAKELIFE, takelife)
									 .Done();

		StateConfigurer.Create(takelife).SetTransition(NPCBStates.IDLE, idle)
										.Done();

		StateConfigurer.Create(catchEnemy).SetTransition(NPCBStates.ATTACK, attack)
										  .SetTransition(NPCBStates.TAKELIFE, takelife)
									      .SetTransition(NPCBStates.WIN, win)
										  .SetTransition(NPCBStates.DIE, die)
										  .Done();

		StateConfigurer.Create(die).SetTransition(NPCBStates.WIN, win)
								   .Done();	

		StateConfigurer.Create(win).SetTransition(NPCBStates.DANCE, dance)
								   .Done();

		StateConfigurer.Create(dance).Done();
		#endregion;

		#region Set States
		idle.OnEnter += x => OnIdleEnter();
		idle.OnUpdate += OnIdle;

		pathfinding.OnEnter += x => OnPathFindingEnter();
		pathfinding.OnUpdate += OnPathFinding;
		pathfinding.OnExit += x => OnPathFindingExit();

		attack.OnUpdate += OnAttack;

		block.OnEnter += x => OnBlockEnter();
		block.OnUpdate += OnBlock;
		block.OnExit += x => OnBlockExit();

		catchEnemy.OnEnter += x => OnCatchEnemyEnter();
		catchEnemy.OnUpdate += OnCatchEnemy;
		catchEnemy.OnExit += x => OnCatchEnemyExit();

		takelife.OnEnter += x => OnTakeLifeEnter();
		takelife.OnUpdate += OnTakeLife;
		takelife.OnExit += x => OnTakeLifeExit();

		die.OnEnter += x => OnDeadEnter();
		die.OnUpdate += OnDead;

		win.OnEnter += x => OnVictoryEnter();
		win.OnUpdate += OnVictory;
		win.OnExit += x => OnVictoryExit();

		dance.OnEnter += x => OnDanceEnter();

		#endregion;

		myFSM = new EventFSM<NPCBStates>(idle); 
	}

	private void SendInputToFSM(NPCBStates input)
	{
		myFSM.SendInput(input);
	}

	#region FSM Voids
	private void OnIdleEnter()
	{
		currentblockResistance = blockResistance;
		animatorManager.RunState(false);
	}

	private void OnIdle()
	{
		CheckVictory();
		CheckLife();
		if (!onFightZone)
			SendInputToFSM(NPCBStates.PATHFINDING);
		else
		{
			float myLifeAverage = (currentLife / maxLife) * 100;

			if (myLifeAverage > percentToHealth)
			{
				LookToEnemy();
				TakeDecision();
			}
			else
			{
				if (!lifeAlreadyTaked)
					SendInputToFSM(NPCBStates.TAKELIFE);
				else
				{
					ChangeDecisionsWeights();
					TakeDecision();
				}
			}
			
		}
	}

	private void OnPathFindingEnter()
	{
		animatorManager.RunState(true);

		var nodeToGo = pathFindingManager.AskAPath(myController, transform);
		myController.MoveTo(nodeToGo);
	}

	private void OnPathFinding()
	{
		PathNode[] path = myController.AstarPathGetter;
		var lastNode = path[path.Length - 1].gameObject.transform.position;
		var distanceToLastNode = Vector3.Distance(myController.transform.position, lastNode);

		myController.GoToPath();

		if (distanceToLastNode <= 1)
			SendInputToFSM(NPCBStates.IDLE);
	}

	private void OnPathFindingExit()
	{
		onFightZone = true;
	}

	private void OnAttack()
	{
		CheckLife();
		CheckVictory();
		if (enemyTarget == null)
		{
			SendInputToFSM(NPCBStates.IDLE);
			return;
		}

		float distanceToTarget = Vector3.Distance(transform.position, enemyTarget.transform.position);
		if (distanceToTarget <= minDistanceToTarget)
		{
			transform.LookAt(enemyTarget.transform.position);
			float randomNumber = Random.value;
			if (randomNumber <= 0.5f)
				animatorManager.AttackState(false);
			else
				animatorManager.AttackState(true);

			SendInputToFSM(NPCBStates.IDLE);
		}
		else
			SendInputToFSM(NPCBStates.CATCHENEMY);
	}

	private void OnBlockEnter()
	{
		animatorManager.Block(true);
		blockingState = true;
	}

	private void OnBlock()
	{
		CheckLife();
		CheckVictory();
		currentblockResistance -= Time.deltaTime;
		if (currentblockResistance <= 0)
			SendInputToFSM(NPCBStates.IDLE);
	}

	private void OnBlockExit()
	{
		animatorManager.Block(false);
		blockingState = false;
	}

	private void OnCatchEnemyEnter()
	{
		animatorManager.RunState(true);
	}
	
	private void OnCatchEnemy()
	{
		CheckLife();
		CheckVictory();
		if (enemyTarget == null)
		{
			SendInputToFSM(NPCBStates.ATTACK);
			return;
		}

		float distanceToTarget = Vector3.Distance(transform.position, enemyTarget.transform.position);
		
		if (distanceToTarget > minDistanceToTarget)
			myController.GoToEnemyTarget(enemyTarget.transform);
		else
			SendInputToFSM(NPCBStates.ATTACK);
	}

	private void OnCatchEnemyExit()
	{
		animatorManager.RunState(false);
	}

	private void OnTakeLifeEnter()
	{
		lifeCan.SetActive(true);
		takingLife = true;
		animatorManager.RunState(false);
		animatorManager.Block(false);
		animatorManager.TakeLife();
	}

	private void OnTakeLife()
	{
		if (takingLife || isDead) return;
		SendInputToFSM(NPCBStates.IDLE);
	}

	private void OnTakeLifeExit()
	{
		lifeCan.SetActive(false);
		currentLife = maxLife;
		lifeAlreadyTaked = true;
		takingLife = false;
	}

	private void OnDeadEnter()
	{
		animatorManager.DeadState();
		gameObject.layer = 0;
		GetComponent<Rigidbody>().isKinematic = true;
		GetComponent<CapsuleCollider>().enabled = false;
		isDead = true;
	}

	private void OnDead()
	{
		CheckVictory();
	}

	private void OnVictoryEnter()
	{
		GetComponent<CapsuleCollider>().enabled = true;
	}

	private void OnVictory()
	{
		float distanceToTarget = Vector3.Distance(transform.position, danceZone.position);
		if (distanceToTarget >= 0.8f)
		{
			animatorManager.RunState(true);
			myController.GoToEnemyTarget(danceZone);
		}
		else
			SendInputToFSM(NPCBStates.DANCE);
	}

	private void OnVictoryExit()
	{
		transform.rotation = Quaternion.Euler(0, 90, 0);
		animatorManager.RunState(false);
	}

	private void OnDanceEnter()
	{
		transform.rotation = Quaternion.Euler(0, 90, 0);
		animatorManager.VictoryState();
	}

	#endregion;

	private void ChangeDecisionsWeights()
	{
		if (decisionsChanged) return;
		//Cambiamos las prioridades. Pasamos a defendernos mas.
		rouletteSelection = new Roulette(posibleDecisions, secondDecisionsWeights);
		decisionsChanged = true;
	}

	private void TakeDecision()
	{
		currentAttackOrBlockTimer += Time.deltaTime;
		if (currentAttackOrBlockTimer >= attackOrBlockTimer) 
		{
			if(blockRepeat < maxBlocks)	  //Utilizamos esto para que a pesar de depender del Roulette Wheel, que no sea posible que bloquee X veces seguidas.
			{
				string rouletteResult = rouletteSelection.RouletteWheelSelection();
			
				if (rouletteResult == posibleDecisions[0])
					SendInputToFSM(NPCBStates.ATTACK);
				else if (rouletteResult == posibleDecisions[1])
				{
					SendInputToFSM(NPCBStates.BLOCK);
					blockRepeat++;
				}
			}

			else
			{
				SendInputToFSM(NPCBStates.ATTACK);
				blockRepeat = 0;
			}
			
			currentAttackOrBlockTimer = 0;
		}
	}
	
	public void TakeDamage(float damage)
	{
		if (takingLife) return;

		mySFX.DamageSound();
		if (!blockingState)
		{
			animatorManager.TakeDamageState();
			currentLife -= damage;
			myVFX.OnDamageVFX();
		}
	}
	
	public void OnTakeLifeEnd()
	{
		takingLife = false;
	}
	
	private void CheckLife()
	{
		if (currentLife <= 0)
			SendInputToFSM(NPCBStates.DIE);
	}
	
	private void CheckVictory()
	{
		if (victory)
			SendInputToFSM(NPCBStates.WIN);
	}

	private void LookToEnemy()
	{
		if (enemyTarget != null)
			transform.forward = Vector3.Slerp(transform.forward,
								(enemyTarget.transform.position - transform.position),
								myController.rotationSpeed * Time.deltaTime);
	}
}



