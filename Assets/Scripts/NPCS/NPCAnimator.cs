using UnityEngine;

public class NPCAnimator : MonoBehaviour
{
    public Animator anim;
    public string danceID;

    public void RunState(bool onRun)
    {
        anim.SetBool("OnRun", onRun);
    }

    public void AttackState(bool hardAttack)
    {
        if (hardAttack)
            anim.SetTrigger("HardAttack");
        else
            anim.SetTrigger("NormalAttack");
    }

    public void Block(bool onBlock)
    {
        anim.SetBool("OnBlock", onBlock);
    }
    
    public void TakeDamageState()
    {
        int random = Random.Range(0, 1);
        if(random == 0)
        anim.SetTrigger("Damage");
        else
        anim.SetTrigger("Damage2");
    }

    public void TakeLife()
    {
        anim.SetTrigger("TakeLife");
        anim.SetBool("TakingLife", true);
    }

    public void LifeTaked()
    {
        anim.SetBool("TakingLife", false);
    }

    public void DeadState()
    {
        anim.SetBool("Dead", true);
        int random = Random.Range(0, 1);
        if (random == 0)
            anim.Play("Death 1");
        else
            anim.Play("Death 2");
    }

    public void VictoryState()
    {
      anim.Play(danceID);
    }

}
