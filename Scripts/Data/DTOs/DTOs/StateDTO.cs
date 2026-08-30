using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateDTO : MonoBehaviour
{
    [System.Serializable]
    public class State
    {
        public int maxHp, currHp;
        public int changeMaxHp, changeCurrHp;
        public bool isChange = false; //변신 상태인지 확인
        public int shield;
        public int maxAction = 4;
        public int currAction = 2;
        public List<int> buff = Enumerable.Repeat(0, 40).ToList();
        public List<int> deBuff = Enumerable.Repeat(0, 40).ToList();
        public GameObject statesButton;

        public void TakeDamage(int damage, int shieldDamage = 1, bool isPentration = false)
        {
            if(!isPentration)
            {
                if (shield > 0)
                {
                    if (damage * shieldDamage < shield)
                    {
                        shield -= damage * shieldDamage;
                        damage = 0;
                    }
                    else
                    {
                        shield = 0;
                        int newDam = damage * shieldDamage - shield;
                        damage = newDam / shieldDamage;
                    }
                }
            }

            if (isChange)
            {
                changeCurrHp -= damage;
                if (changeCurrHp < 0)
                {
                    changeCurrHp = 0;
                    isChange = false;
                }
            }
            else
            {
                currHp -= damage;
                if (currHp <= 0)
                {

                    if (buff[10] > 0){//버티기 효과 확인
                        currHp = 1;
                    }
                    else
                    {
                        currHp = 0;
                    }
                }
            }

        }
    }
}
