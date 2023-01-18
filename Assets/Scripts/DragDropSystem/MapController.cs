using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace ZoneSystem
{
    enum SpeciesType
    {
        Dwarf, Undead, Scorpion, Orc, Mecha
    }

    enum ClassType
    {
        Warrior, Tanker, Magician, RangeDealer, Assassin
    }
    public class MapController : MonoBehaviour
    {
        public GameObject[,] safetyObject;
        public GameObject[,] battleObject;

        public TextMeshProUGUI debug;

        //아이템 랜뽑 일단은 스트링값으로
        string[] RandomItem;

        public int battleUnitCount = 0;

        [SerializeField]
        GameObject UnitPrefab;
        [SerializeField]
        GameObject battleZoneTile;

        Transform[] transforms;

        private void Awake()
        {
            transforms = new Transform[5];

            safetyObject = new GameObject[2, 7];
            battleObject = new GameObject[3, 7];
            RandomItem = new string[] { "sword", "cane", "dagger", "Armor", "robe" };
        }
        private void Start()
        {
            //transforms[0]
       

            MapCreate();

        }

        public int BattlezoneChack()
        {
            for (int z = 0; z < 3; z++)
            {
                for (int x = 0; x < 7; x++)
                {
                    if (battleObject[z, x] != null)
                    {
                        ++battleUnitCount;
                    }
                }
            }
            return battleUnitCount;
        }

        public void OnClick_UnitInst()
        {

            for (int z = 0; z < 2; z++)
            {
                for (int x = 0; x < 7; x++)
                {
                    if (safetyObject[z, x] == null)
                    {
                        safetyObject[z, x] = Instantiate(UnitPrefab, new Vector3(x, 0.25f, z - 2), Quaternion.identity);
                        return;
                    }
                }
            }
            debug.text = "세이프티존이 꽉차서 유닛을 소환할 수 없습니다.";
        }

        //������ ȹ��
        public void itemGain(GameObject getItem)
        {
            
            for (int z = 0; z < 2; z++)
            {
                for (int x = 0; x < 7; x++)
                {
                    if (safetyObject[z, x] == null)
                    {
                        Debug.Log(RandomItem[Random.Range(0, 5)]);
                        safetyObject[z, x] = getItem;
                        safetyObject[z, x].name = RandomItem[Random.Range(0, 5)];
                        safetyObject[z, x].transform.position = new Vector3(x, 0.25f, z - 2);
                        safetyObject[z, x].transform.rotation = Quaternion.identity;
                        safetyObject[z, x].layer = 31;
                        return;
                    }
                }
            }
            debug.text = "세이프티존이 꽉차서 아이템을 습득할 수 없습니다.";
        }


        //맵생성
        public void MapCreate()
        {
            for (int z = 0; z < 3; z++)
            {
                for (int x = 0; x < 7; x++)
                {
                    float newPosX = (float)x * 1.5f;
                    float newPosZ = (float)(z * 1.3f); 

                    if (z % 2 == 0) { }

                    else
                    {
                        newPosX += 0.65f;
                    }

                    Vector3 tilePos = new Vector3(newPosX, 0, newPosZ);
                    GameObject newTile = Instantiate(battleZoneTile,tilePos,Quaternion.identity);
                   
                }
            }
        }

    }
}
