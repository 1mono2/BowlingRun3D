using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


namespace MoNo.Bowling
{
    [RequireComponent(typeof(OrganizeQuantity))]
    [ExecuteInEditMode]
    public class DisplayOrganizeQuantity : MonoBehaviour
    {
        OrganizeQuantity quantity;
        TextMeshPro text;

        private void Start()
        {
            Reset();
        }

        void Reset()
        {
            quantity = GetComponent<OrganizeQuantity>();
            text = GetComponentInChildren<TextMeshPro>();

            switch (quantity.operations)
            {
                case OrganizeQuantity.ArithmeticOperations.addition:
                    text.text = $"+" + quantity.num;
                    break;
                case OrganizeQuantity.ArithmeticOperations.subtraction:
                    text.text = $"-" + quantity.num;
                    break;
                case OrganizeQuantity.ArithmeticOperations.multiplication:
                    text.text = $"x" + quantity.num;
                    break;
                case OrganizeQuantity.ArithmeticOperations.division:
                    text.text = $"÷" + quantity.num;
                    break;
                default:
                    text.text = $"Nan";
                    break;
            }
        }


    }
}