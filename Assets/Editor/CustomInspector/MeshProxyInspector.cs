namespace CustomInspector
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(MeshProxy))]
    public class MeshProxyInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            var meshProxy = target as MeshProxy;
            if (meshProxy == null)
                return;

            if (GUILayout.Button("Toggle Proxy Mode"))
                meshProxy.ToggleProxyMode();
        }
    }
}
