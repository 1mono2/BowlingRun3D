#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MoNo.Bowling
{

    [CustomEditor(typeof(OrganizeQuantity))]
    public class OrganizeQuantityEditor : Editor
    {
        /// <summary>
        /// Updating Inspector's GUI 
        /// </summary>
        ///

        SerializedProperty _numProp;

        private void OnEnable()
        {
            _numProp = serializedObject.FindProperty("_num");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //targetを変換して対象を取得
            OrganizeQuantity quantity = target as OrganizeQuantity;

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();


            // 値に変更があったときは speedが必ず0以上になるように調整
            if (EditorGUI.EndChangeCheck())
            {
                _numProp.floatValue = Mathf.Max(0, _numProp.floatValue);
            }

            //PublicMethodを実行する用のボタン
            if (GUILayout.Button("Add"))
            {
                Undo.RecordObject(quantity, "Add");
                quantity.AddNum();
            }

            if (GUILayout.Button("Subtract"))
            {
                Undo.RecordObject(quantity, "Subtrace");
                quantity.SubtractNum();
            }

            serializedObject.ApplyModifiedProperties();

        }
    }
}