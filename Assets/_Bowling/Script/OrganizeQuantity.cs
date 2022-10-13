using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace MoNo.Bowling
{
    public class OrganizeQuantity : MonoBehaviour
    {
        public enum ArithmeticOperations
        {
            addition,
            subtraction,
            multiplication,
            division,
        }
        [SerializeField] public ArithmeticOperations operations = ArithmeticOperations.addition;
        [SerializeField, Range(1, 100)] public int _num = 1;
        public int num => _num;

        public int SetNum(int num)
        {
            _num = num;
            return num;
        }

        public void AddNum()
        {
            _num += 1; 
        }

        public void SubtractNum()
        {
            if(_num > 0)
            {
                _num -= 1;
            }
        }


        /// <summary>
        /// inputting the number of current pins, this function returns how many pins it add or sub.
        /// </summary>
        /// <param name="currentPinNum"></param>
        /// <returns></returns>
        public int Calculation(int currentPinNum)
        {
            int reNum;
            if(_num < 1)
            {
                Debug.LogWarning("This class has an invalid params");
                return 1;
            }
            switch (operations)
            {
                case ArithmeticOperations.addition:
                    reNum = _num;
                    break;
                case ArithmeticOperations.subtraction:
                    reNum = -_num;
                    break;
                case ArithmeticOperations.multiplication:
                    reNum = currentPinNum * (_num - 1);
                    break;
                case ArithmeticOperations.division:
                    reNum = - (_num -1) * currentPinNum / _num ;
                    break;
                default:
                    reNum = _num;
                    break;
            }
            return reNum;
        }
    }
}
