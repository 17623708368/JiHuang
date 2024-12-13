using System.Collections;

public class AI_HurtState : AI_StateBase
{
    public override void Enter()
    {
        AI.PlayAudio(AIState.Hurt, 1);
        AI.PlayAnimation("Hurt");
        AI.AddAnimationEvent("OverHurt",OverHurt);
    }

    private void OverHurt()
    {  
        AI.ChangeState(AIState.Pursue);
    }
    

    public override void Exit()
    {
        AI.RemoveAnimationEvent("OverHurt");
    }
}