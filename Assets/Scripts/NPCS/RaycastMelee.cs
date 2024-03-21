using UnityEngine;

public class RaycastMelee : MonoBehaviour
{
	private float attackReach = 1.5f;

	private int layerValueNPCA = 8;
	private int layerValueNPCB = 10;

	private float normalDamage = 2.0f;
	private float hardDamage = 3.0f;

	public enum TypeOfNPC { NPCA, NPCB }
	public TypeOfNPC thisNpc;

	public void NPC_Raycast(string typeOfAttack)
	{
		if (typeOfAttack == "Normal")
			NPC_AttackRaycast(false);
		else
			NPC_AttackRaycast(true);
	}

	private void NPC_AttackRaycast(bool hardAttack)
	{
		int targetLayer = thisNpc == TypeOfNPC.NPCA ? layerValueNPCB : layerValueNPCA;
		RaycastHit hit;
		Vector3 raycastOriginPosition = transform.position + new Vector3(0, 1, 0);

		if (Physics.Raycast(raycastOriginPosition, transform.forward, out hit, attackReach))
		{
			if (hit.collider.gameObject.layer == targetLayer)
			{
				if (targetLayer == layerValueNPCA)
				{
					if (hardAttack)
						NPC_B_HardAttackSuccessful(hit.collider.gameObject);
					else
						NPC_B_NormalAttackSuccessful(hit.collider.gameObject);
				}

				else
				{
					if (hardAttack)
						NPC_A_HardAttackSuccessful(hit.collider.gameObject);
					else
						NPC_A_NormalAttackSuccessful(hit.collider.gameObject);
				}
			}
		}
	}

	private void NPC_A_NormalAttackSuccessful(GameObject target)
	{
		if (target.GetComponent<NPCBFSM>())
			target.GetComponent<NPCBFSM>().TakeDamage(normalDamage);
	}

	private void NPC_A_HardAttackSuccessful(GameObject target)
	{
		if (target.GetComponent<NPCBFSM>())
			target.GetComponent<NPCBFSM>().TakeDamage(hardDamage);
	}

	private void NPC_B_NormalAttackSuccessful(GameObject target)
	{
		if (target.GetComponent<NPCALeaderFSM>())
			target.GetComponent<NPCALeaderFSM>().TakeDamage(normalDamage);

		else if (target.GetComponent<NPCAFSM>())
			target.GetComponent<NPCAFSM>().TakeDamage(normalDamage);

	}

	private void NPC_B_HardAttackSuccessful(GameObject target)
	{
		if (target.GetComponent<NPCALeaderFSM>())
			target.GetComponent<NPCALeaderFSM>().TakeDamage(hardDamage);

		else if (target.GetComponent<NPCAFSM>())
			target.GetComponent<NPCAFSM>().TakeDamage(hardDamage);
	}

}
