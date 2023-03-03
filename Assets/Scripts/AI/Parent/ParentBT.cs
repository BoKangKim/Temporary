using UnityEngine;
using BehaviorTree;
using static BehaviorTree.BehaviorTreeMan;
using System;
using System.Collections.Generic;
using Battle.Stage;
using Battle.Location;
using Battle.RTASTAR;
using Battle.EFFECT;
using Photon.Pun;
using Photon.Realtime;

namespace Battle.AI
{
    [RequireComponent(typeof(UnitClass.Unit), typeof(Rigidbody), typeof(BoxCollider))]
    public abstract class ParentBT : MonoBehaviourPun
    {
        #region MyData
        private INode root = null;
        private INode specialRoot = null;
        protected ParentBT target = null;
        protected ParentBT skilltarget = null;

        protected STAGETYPE stageType = STAGETYPE.PREPARE;

        protected Animator myAni = null;
        private List<ParentBT> enemies = null;
        private List<ParentBT> myUnits = null;

        private ParentBT[] allUnits = null;

        protected string myType = null;
        [SerializeField] protected string nickName = null;
        [SerializeField] protected Effect standardAttackEffect = null;
        [SerializeField] protected SkillEffect skillEffect = null;

        protected LocationXY myLocation;
        protected LocationXY next;
        protected Vector3 nextPos;
        protected Vector3 dir;

        Vector3 nextLocation = Vector3.zero;
        protected RTAstar rta = null;
        protected UnitClass.Unit unitData = null;

        [Header("unitStatus")]
        private float currentHP = 100f;
        protected bool ishit = false;

        protected float attackDamage = 0f;
        protected float spellPower = 0f;
        protected float maxMana = 10f;
        protected float maxHP = 0f;
        protected float shield = 0f;

        protected float mana = 0f;
        protected float attackRange = 0f;
        protected float manaRecovery = 5f;

        private bool die = false;
        private bool isInit = false;
        private bool isFirst = true;
        [SerializeField] private string enemyNickName = "";

        private Vector3 startposition;
        private bool isEnabled = true;
        private StageControl sc = null;
        #endregion
        #region GET,SET

        public ParentBT getSkillTarget()
        {
            return skilltarget;
        }
        
        public void setAttackDamage(float addAtk)
        {
            attackDamage += addAtk;
        }
        public void setSpellPower(float addSpellPower)
        {
            attackDamage += addSpellPower;
        }
        public float getSpellPower()
        {
            return spellPower;
        }
        public Animator getAnimator()
        {
            return myAni;
        }
        public ParentBT getTarget()
        {
            return target;
        }
        public float getMaxHP()
        {
            return maxHP;
        }
        public void setShield(float addShield)
        {
            currentHP += addShield;
        }

        public void setRecoveryCurrentHP(float Recovery)
        {
            currentHP += Recovery;
            if (currentHP > maxHP)
            {
                currentHP = maxHP;
            }
            
        }
        public UnitClass.Unit getUnitData()
        {
            return unitData;
        }
        public void setAttackRange(float attackRange)
        {
            this.attackRange = attackRange;
        }

        public void setState(STAGETYPE state)
        {
            this.stageType = state;
        }

        public void setIsHit(bool ishit)
        {
            this.ishit = ishit;
        }

        public string getMyNickName()
        {
            return nickName;
        }

        public LocationXY getMyLocation()
        {
            return myLocation;
        }

        public LocationXY getNextLocation()
        {
            return next;
        }

        public void setMyLocation()
        {
            myLocation = LocationControl.convertPositionToLocation(gameObject.transform.localPosition);
        }

        public float getAttackDamage()
        {
            return attackDamage;
        }

        public bool getIsDeath()
        {
            return die;
        }
        public List<ParentBT> getFindEnemies()
        {
            return enemies;
        }
        public List<ParentBT> getFindMyUnits()
        {
            return myUnits;
        }
        public string getMyType()
        {
            return myType;
        }

        public void setEnemyNickName(string enemyNickName)
        {
            enemies.Clear();
            this.enemyNickName = enemyNickName;
        }
        #endregion

        private void Awake()
        {
            InitializingRootNode();
            specialRoot = initializingSpecialRootNode();
            rta = new RTAstar(myLocation,gameObject.name);
            myType = initializingMytype();
            initializingData();
            if(photonView.IsMine == true)
            {
                nickName = PhotonNetwork.NickName;
                Debug.Log(nickName + " AWAKE");
                photonView.RPC("RPC_SetNickName", RpcTarget.Others, nickName);
            }
        }

        private void OnEnable()
        {
            if(myAni == null && 
                enemies == null)
            {
                myAni = GetComponent<Animator>();
                enemies = new List<ParentBT>();
                myLocation = LocationControl.convertPositionToLocation(gameObject.transform.localPosition);
            }

            initializingAfterBattle();

            if(photonView.IsMine == true
                && isEnabled == true
                && myType.CompareTo("UnitAI") == 0)
            {
                this.enabled = false;

                isEnabled = false;
            }
        }

        private void Start()
        {
            sc = FindObjectOfType<StageControl>();
            if (sc != null)
            {
                sc.changeStage += changeStage;
            }
        }

        private void Update()
        {
            if (photonView.IsMine == false)
            {
                return;
            }

            if (stageType == STAGETYPE.PREPARE)
            {
                return;
            }

            if (die == true)
            {
                return;
            }

            if (specialRoot != null 
                && specialRoot.Run() == true)
            {
                return;
            }

            root.Run();
            myLocation = LocationControl.convertPositionToLocation(transform.localPosition);
        }

        private void InitializingRootNode()
        {
            root = Selector
                (
                    IfAction(isDeath, death),

                    Sequence
                    (
                        ActionN(idle),
                        NotIf(findEnemy)
                    ),

                    IfElseAction(isArangeIn, attack, move)
                );
        }

        protected virtual INode initializingSpecialRootNode() { return null; }
        protected abstract string initializingMytype();
        protected abstract float setAttackRange();

        protected virtual void initializingAfterBattle()
        {
            if(myAni != null)
            {
                myAni.SetBool("isMove", false);
                myAni.ResetTrigger("isAttack");
            }
            
            
            sc = FindObjectOfType<StageControl>();
            if (sc != null)
            {
               sc.changeStage += changeStage;
            }

            isInit = false;
            enemies.Clear();
            target = null;
            mana = 0;
            die = false;
            initializingData();
            rta.initCloseList();

            if (photonView.IsMine == true)
            {
                nickName = PhotonNetwork.NickName;
                photonView.RPC("RPC_SetNickName", RpcTarget.Others, nickName);
            }
        }

        [PunRPC]
        public void RPC_SetNickName(string nickName)
        {
            this.nickName = nickName;
        }

        [PunRPC]
        public void RPC_initializingAfterBattle()
        {
            myAni.SetBool("isMove", false);
            myAni.ResetTrigger("isAttack");
            isInit = false;
            enemies.Clear();
            target = null;
            mana = 0;
            die = false;
            nickName = PhotonNetwork.NickName;
            initializingData();
        }

        public void doDamage()
        {
            target.photonView.RPC("RPC_DoDamage", RpcTarget.All,attackDamage);
        }

        public void doDamage(float damage)
        {
            target.photonView.RPC("RPC_DoDamage", RpcTarget.All, damage);
        }

        [PunRPC]
        public void RPC_DoDamage(float damage)
        {
            Damage(damage);
        }

        private void initializingData()
        {
            if (TryGetComponent<UnitClass.Unit>(out unitData) == false)
            {
                Debug.LogError("Not Found Unit Data");
            }

            currentHP = unitData.totalMaxHp;
            maxMana = unitData.totalMaxMp;
            maxMana = 5f;
            manaRecovery += unitData.totalMpRecovery;
            attackRange = unitData.totalAttackRange;
            attackDamage = unitData.totalAtkDamage;
            spellPower = unitData.totalSpellPower;
            //myAni.speed = unitData.totalAttackSpeed;
        }

        public void changeStage(STAGETYPE stageType)
        {
            this.stageType = stageType;
            initializingAfterBattle();
        }

        #region Searching Enemy
        protected void findEnemyFuncOnStart(ParentBT[] fieldAIObejects)
        {
            rta.initAllUnits(fieldAIObejects);

            switch (myType)
            {
                case "UnitAI":
                    addEnemyList(fieldAIObejects);
                    break;
                case "Monster":
                    addEnemyList(fieldAIObejects, "Monster");
                    break;
                case "Boss":
                    addEnemyList(fieldAIObejects, "Boss");
                    break;
            }

        }

        private void addEnemyList(ParentBT[] fieldAIObejects, string compare)
        {
            for (int i = 0; i < fieldAIObejects.Length; i++)
            {
                if (fieldAIObejects[i].myType.CompareTo(compare) == 0
                    || fieldAIObejects[i].photonView.IsMine == false)
                {
                    continue;
                }

                enemies.Add(fieldAIObejects[i]);
            }

            Debug.Log(enemies.Count);
        }

        private void addEnemyList(ParentBT[] fieldAIObejects)
        {
            if (stageType == STAGETYPE.PVP
                || stageType == STAGETYPE.CLONE)
            {
                for (int i = 0; i < fieldAIObejects.Length; i++)
                {
                    if (fieldAIObejects[i].nickName.CompareTo(nickName) == 0)
                    {
                        continue;
                    }

                    if (fieldAIObejects[i].enabled == false)
                    {
                        continue;
                    }

                    if (fieldAIObejects[i].nickName.CompareTo(enemyNickName) == 0)
                    {
                        enemies.Add(fieldAIObejects[i]);
                    }

                }
            }
            else
            {
                addEnemyList(fieldAIObejects, "UnitAI");
            }
        }

        protected virtual void searchingTarget()
        {
            float minDistance = 100000f;
            float temp = 0f;
            target = null;

            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] == null)
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                if ((temp = Vector3.Distance(enemies[i].transform.localPosition,transform.localPosition)) <= minDistance)
                {
                    minDistance = temp;
                    target = enemies[i];
                }
            }

        }

        private bool checkIsOverlapUnits()
        {
            LocationXY unitLocation;
            for (int i = 0; i < allUnits.Length; i++)
            {
                if (allUnits[i].Equals(this))
                    continue;
                if (allUnits[i] == null)
                {
                    continue;
                }

                unitLocation = LocationControl.convertPositionToLocation(allUnits[i].gameObject.transform.localPosition);
                if (unitLocation.CompareTo(myLocation) == true)
                {
                    return true;
                }
            }

            return false;
        }

        public void Damage(float damage)
        {
            currentHP -= damage;
        }
        #endregion

        #region AI Behavior
        protected virtual Action idle
        {
            get     
            {
                return () =>
                {
                    myLocation = LocationControl.convertPositionToLocation(gameObject.transform.localPosition);
                    photonView.RPC("RPC_SetMyLocation",RpcTarget.Others,myLocation.x,myLocation.y);
                    if (enemies.Count == 0 || isInit == false)
                    {
                        photonView.RPC("RPC_EnableThis",RpcTarget.All);
                        findEnemyFuncOnStart((allUnits = FindObjectsOfType<ParentBT>()));
                        searchingTarget();

                        if(target == null)
                        {
                            Debug.Log(enemies.Count + " target null");
                            return;
                        }

                        if (rta.checkLocationArrange(target.getMyLocation().x, target.getMyLocation().y) == false)
                        {
                            enemies.Clear();
                            return;
                        }

                        next = rta.searchNextLocation(myLocation, target.getMyLocation());
                        nextPos = LocationControl.convertLocationToPosition(next);
                        dir = (nextPos - transform.localPosition).normalized;
                        dir.y = 0f;
                        
                        isInit = true;
                        
                    }
                };
            }
        }

        [PunRPC]
        public void RPC_EnableThis()
        {
            if(this.enabled == false)
            {
                this.enabled = true;
            }
        }

        protected virtual Func<bool> findEnemy
        {
            get
            {
                return () =>
                {
                    searchingTarget();
                    if (target == null)
                    {
                        myAni.SetBool("isMove",false);
                        myAni.ResetTrigger("isAttack");
                        dir = Vector3.zero;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                };
            }
        }

        protected virtual Action move
        {                                                                                                                         
            get
            {
                return () =>
                {
                    if(target == null)
                    {
                        return;
                    }

                    if(rta.checkLocationArrange(target.getMyLocation().x,target.getMyLocation().y) == false)
                    {
                        return;
                    }

                    myAni.SetBool("isMove",true);
                    myLocation = LocationControl.convertPositionToLocation(transform.localPosition);
                    photonView.RPC("RPC_SetMyLocation", RpcTarget.Others, myLocation.x, myLocation.y);
                    
                    if (Vector3.Distance(nextPos, transform.localPosition) <= 0.4f)
                    {
                        next = rta.searchNextLocation(myLocation, target.getMyLocation());
                        nextPos = LocationControl.convertLocationToPosition(next);

                        dir = (nextPos - transform.localPosition).normalized;
                        dir.y = 0f;
                    }

                    transform.rotation = Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(dir), Time.deltaTime * 10f);
                    gameObject.transform.Translate(dir * 5f * Time.deltaTime,Space.World);
                };
            }
        }

        protected virtual Func<bool> isArangeIn
        {
            get
            {
                return () =>
                {
                    
                    myLocation = LocationControl.convertPositionToLocation(transform.localPosition);
                    photonView.RPC("RPC_SetMyLocation", RpcTarget.Others, myLocation.x, myLocation.y);
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        if (enemies[i].getIsDeath() == true) 
                        {
                            enemies[i] = null;
                            continue;
                        }
                        if (enemies[i].isActiveAndEnabled == false)
                        {
                            target = null;
                            continue;
                        }

                        Vector3 targetPos = LocationControl.convertLocationToPosition(enemies[i].getMyLocation());

                        if (Vector3.Distance(targetPos, transform.localPosition) <= (LocationControl.radius * attackRange)
                        && checkIsOverlapUnits() == false)
                        {
                            rta.initCloseList();
                            target = enemies[i];
                            next = myLocation;
                            return true;
                        }
                    }

                    return false;
                };
            }
        }

        protected virtual Action attack
        {
            get
            {
                return () =>
                {
                    this.transform.LookAt(target.transform);
                    myAni.SetBool("isMove",false);
                    photonView.RPC("RPC_SetTriggerAttack",RpcTarget.All);
                };
            }
        }

        [PunRPC]
        public void RPC_SetMyLocation(int x,int y)
        {
            this.myLocation.x = x;
            this.myLocation.y = y;
        }

        [PunRPC]
        public void RPC_SetTriggerAttack()
        {
            if(myAni == null)
            {
                if(gameObject.TryGetComponent<Animator>(out myAni) == false)
                {
                    Debug.Log("Not Found Animator" + gameObject.name);
                    return;
                }
            }

            myAni.SetTrigger("isAttack");
        }

        protected virtual Func<bool> isDeath
        {
            get
            {
                return () =>
                {
                    if(currentHP <= 0f)
                    {
                        die = true;
                        photonView.RPC("setIsDeath",RpcTarget.Others,die);
                        return true;
                    }

                    return false;
                };
            }
        }

        [PunRPC]
        public void setIsDeath(bool die)
        {
            this.die = die;
        }

        protected virtual Action death
        {
            get
            {
                return () =>
                {
                    photonView.RPC("RPC_SetTriggerDeath",RpcTarget.All);
                };
            }
        }

        [PunRPC]
        public void RPC_SetTriggerDeath()
        {
            myAni.SetTrigger("isDeath");
        }

        protected virtual Action hit 
        {
            get 
            {
                return () =>
                {
                };
            }
        }

        protected void DestroyObject()
        {
            if(photonView.IsMine == true)
            {
                PhotonNetwork.Destroy(gameObject);
                GameManager.Inst.GetPlayerInfoConnector().MinusUnitCount();

                if(GameManager.Inst.GetPlayerInfoConnector().GetUnitCount() <= 0
                    && myType.CompareTo("UnitAI") == 0)
                {
                    GameManager.Inst.GetPlayerInfoConnector().GetPlayer().CurHP -= (enemies.Count * 2);
                    GameManager.Inst.GetPlayerInfoConnector().SyncOwnerHP();
                }
            }
        }

        #endregion

        public virtual void StartEffect()
        {
            //GameObject _effect = Instantiate<GameObject>(effect,transform.position,Quaternion.LookRotation(transform.forward));
        }

        public virtual void StartSkillEffect()
        {
            
        }
    }

}








