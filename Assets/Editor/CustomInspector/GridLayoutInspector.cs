using UnityEngine;

namespace CustomInspector
{
    using UnityEditor;

    [CustomEditor(typeof(GridLayout))]
    public class GridLayoutInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty boxCollider = null;

            var currentObject = serializedObject.GetIterator();
            currentObject.NextVisible(true);

            while (true)
            {
                if (currentObject.name == "m_UnitVolume")
                    boxCollider = currentObject.Copy();

                if (currentObject.name != "m_ObjectVolume" ||
                    currentObject.name == "m_ObjectVolume" && 
                        boxCollider != null && boxCollider.objectReferenceValue == null)
                    EditorGUILayout.PropertyField(currentObject);

                if (!currentObject.NextVisible(false))
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
