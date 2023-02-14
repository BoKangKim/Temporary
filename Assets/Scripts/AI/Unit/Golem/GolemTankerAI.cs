using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemTankerAI : MeleeAI
{
    public override void StartSkillEffect()
    {
        SkillEffect skill = null;
        Vector3 targetPos = target.transform.position;
        Instantiate(skillEffect.gameObject).TryGetComponent<SkillEffect>(out skill);
        skill.gameObject.transform.position = new Vector3(targetPos.x, 0f, targetPos.z);
        skill.setOwner(this);
    }
}