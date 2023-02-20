using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonAssassinSkill : SkillEffect
{
    protected override float setSpeed()
    {
        return 1f;
    }

    protected override float setDestroyTime()
    {
        return 2f;
    }

    protected override bool setIsNonAttackEffect()
    {
        return true;
    }
    private void OnEnable()
    {
        owner.getSkillTarget().doDamage(owner.getAttackDamage() * 5);
    }
}
