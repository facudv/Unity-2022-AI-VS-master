using UnityEngine;
using UnityEngine.UI;

public class LifeHUD : MonoBehaviour
{
    public enum NPCHUDType { NPCB, NPCA, NPCLEADER}
    public NPCHUDType typeOfHUD;

    public GameObject canvas;
    private GameObject lifeBarBG;
    public Image lifeHUD;
    public Image resistanceHUD;
    public Image lifeAvailableHUD;

    public NPCALeaderFSM myLeader;
    public NPCAFSM myNPCA;
    public NPCBFSM myNPCB;

    private bool lifeAvailableChecked;
    private bool deadChecked;

    void Start()
    {
        lifeBarBG = lifeHUD.transform.parent.gameObject;
    }

    void Update()
    {
        CheckVictory();
        CheckDead();
        LookToCamera();
        RefreshResistance();
        RefreshLife();
        TheyHaveAlreadyTakenTheirVitaminsYouKnow();
    }

    private void TheyHaveAlreadyTakenTheirVitaminsYouKnow()
    {
        if (lifeAvailableChecked) return;

        if (typeOfHUD == NPCHUDType.NPCLEADER)
        {
            if (myLeader.lifeAlreadyTaked)
            {
                lifeAvailableHUD.enabled = false;
                lifeAvailableChecked = true;
            }

        }
        else if (typeOfHUD == NPCHUDType.NPCA)
        {
            if (myNPCA.lifeAlreadyTaked)
            {
                lifeAvailableHUD.enabled = false;
                lifeAvailableChecked = true;
            }
        }
        else
        {
            if(myNPCB.lifeAlreadyTaked)
            {
                lifeAvailableHUD.enabled = false;
                lifeAvailableChecked = true;
            }
        }
    }

    private void CheckVictory()
    {
        if (typeOfHUD == NPCHUDType.NPCLEADER)
        {
            if (myLeader.victory)
            {
                canvas.SetActive(false);
                this.enabled = false;
            }

        }
        else if (typeOfHUD == NPCHUDType.NPCA)
        {
            if (myNPCA.victory)
            {
                canvas.SetActive(false);
                this.enabled = false;
            }
        }
        else
        {
            if (myNPCB.victory)
            {
                canvas.SetActive(false);
                this.enabled = false;
            }
        }
    }

    private void CheckDead()
    {
        if (deadChecked) return;

        if (typeOfHUD == NPCHUDType.NPCLEADER)
        {
            if(myLeader.isDead)
            {
                canvas.SetActive(false);
                deadChecked = true;
            }

        }
        else if (typeOfHUD == NPCHUDType.NPCA)
        {
            if(myNPCA.isDead)
            {
                canvas.SetActive(false);
                deadChecked = true;
            }
        }
        else
        {
            if (myNPCB.isDead)
            {
                canvas.SetActive(false);
                deadChecked = true;
            }
        }
    }

    private void RefreshResistance()
    {
        if (typeOfHUD == NPCHUDType.NPCLEADER)
        {
            float resValue = myLeader.currentblockResistance / myLeader.blockResistance;
            resistanceHUD.fillAmount = resValue;
        }

        else if (typeOfHUD == NPCHUDType.NPCA)
        {
            float resValue = myNPCA.currentblockResistance / myNPCA.blockResistance;
            resistanceHUD.fillAmount = resValue;
        }
        else
        {
            float resValue = myNPCB.currentblockResistance / myNPCB.blockResistance;
            resistanceHUD.fillAmount = resValue;
        }
    }

    private void RefreshLife()
    {
        if(typeOfHUD == NPCHUDType.NPCLEADER)
        {
            float hudLifeValue = myLeader.currentLife / myLeader.maxLife;
            lifeHUD.fillAmount = hudLifeValue;
        }
        else if(typeOfHUD == NPCHUDType.NPCA)
        {
            float hudLifeValue = myNPCA.currentLife / myNPCA.maxLife;
            lifeHUD.fillAmount = hudLifeValue;
        }
        else
        {
            float hudLifeValue = myNPCB.currentLife / myNPCB.maxLife;
            lifeHUD.fillAmount = hudLifeValue;
        }
    }

    private void LookToCamera()
    {
        if(lifeBarBG != null)
            lifeBarBG.transform.LookAt(Camera.main.transform.position);
    }
}
