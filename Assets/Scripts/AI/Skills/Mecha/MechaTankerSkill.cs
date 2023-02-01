using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechaTankerSkill : SkillEffect
{
    protected override float setDestroyTime()
    {
        return 5f;
    }

    protected override float setSpeed()
    {
        return 0f;
    }

    protected override bool setIsNonAttackEffect()
    {
        return true;
    }

    protected override void specialLogic()
    {
    }
}
