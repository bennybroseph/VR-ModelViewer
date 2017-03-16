namespace CustomInspector
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ModelProxy))]
    public class ModelProxyInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            var modelProxy = target as ModelProxy;
            if (modelProxy == null)
                return;

            if (GUILayout.Button("Toggle Proxy Mode"))
                modelProxy.ToggleProxyMode();
        }
    }
}
