using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle.Stage;
using Photon.Pun;
using Photon.Realtime;

public class Timer : MonoBehaviour
{
    private StageControl sc = null;
    private const float STAGE_TIME = 30f;
    private float nowTime = 0f;
    private bool scIsNull = true;
    private bool isPlaying = true;

    private void Start()
    {
        GameManager.Inst.SetTimer(this);
    }

    public float getNowTime()
    {
        return nowTime;
    }

    public void setNowTime(float time)
    {
        if(time > STAGE_TIME + 2)
        {
            return;
        }

        nowTime = time;
    }

    public void SetIsPlaying(bool isPlaying)
    {
        this.isPlaying = isPlaying;
    }

    private void Update()
    {
        if (isPlaying == false)
        {
            return;
        }
        
        if(sc == null)
        {
            return;
        }

        if(PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        nowTime += Time.deltaTime;
        GameManager.Inst.time = nowTime;
        GameManager.Inst.GetPlayerInfoConnector().SyncTimer((30 -(int)nowTime).ToString());

        if(nowTime >= STAGE_TIME)
        {
            nowTime = 0;
            sc.checkNextStageInfo();
            sc.startNextStage();
        }
    }

    public void initializingStageControl(StageControl sc)
    {
        this.sc = sc;
    }

    public void findStageControl()
    {
        sc = FindObjectOfType<StageControl>();
    }
}
