using UnityEngine;
using System.Collections;

public class CameraWinManager : MonoBehaviour
{
    public Transform cameraLevel;
    public Transform homieReference;
    public Transform policeReference;

    private int npcaNewZoom = 4;
    private int npcBNewZoom = 4;

    public void NPCAVictory()
    {
        StartCoroutine(ChangeCameraPosition("A"));
    }

    public void NPCBVictory()
    {
        StartCoroutine(ChangeCameraPosition("B"));
    }

    IEnumerator ChangeCameraPosition(string npcType)
    {
        yield return new WaitForSeconds(1.0f);
       
        if(npcType == "A")
        {
            cameraLevel.gameObject.GetComponent<CameraZoom>().maxZoom -= npcaNewZoom;
            cameraLevel.transform.position = homieReference.position;
            cameraLevel.transform.rotation = homieReference.rotation;
        }
        
        else if(npcType == "B")
        {
            cameraLevel.gameObject.GetComponent<CameraZoom>().maxZoom -= npcBNewZoom;
            cameraLevel.transform.position = policeReference.position;
            cameraLevel.transform.rotation = policeReference.rotation;
        }
    }
}
