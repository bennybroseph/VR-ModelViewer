using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using ThirdPartyScripts;

public class ResourceManager : MonoBehaviour
{
    [SerializeField, Range(0f, 10f)]
    private float m_CheckDirectoryTime = 1f;

    [Header("Debugging"), SerializeField]
    private Text m_DebugText;

    private List<ModelProxy> m_ModelProxies = new List<ModelProxy>();

    private void Awake()
    {
        CheckDirectory();

        StartCoroutine(CheckDirectoryEnumerator());
    }

    private void CheckDirectory()
    {
        var files = Directory.GetFiles(Application.streamingAssetsPath);

        var unityCurrentDirectory = Directory.GetCurrentDirectory().Replace('\\', '/');

        // Get rid of the current directory since it's unnecessary
        // Change all '\' to '/' to stay consistent
        // Remove any '/' from the beginning of the path
        for (var i = 0; i < files.Length; i++)
        {
            files[i] = files[i].Replace(unityCurrentDirectory, "");
            files[i] = files[i].Replace('\\', '/');
            files[i] = files[i].TrimStart('/');
        }

        var newFiles =
            files.Where(
                file => !m_ModelProxies.Select(proxy => proxy.modelPath).Contains(file) && (
                file.EndsWith(".obj") || file.EndsWith(".fbx"))).ToList();

        // A new Model was added to the directory since last checked
        if (newFiles.Any())
        {
            foreach (var newFile in newFiles)
            {
                var newModelProxy = new GameObject().AddComponent<ModelProxy>();
                newModelProxy.modelPath = newFile;

                m_ModelProxies.Add(newModelProxy);
            }
        }

        var removedFiles = m_ModelProxies.Where(proxy => !files.Contains(proxy.modelPath)).ToList();

        // A Model was removed from the directory since last checked
        if (removedFiles.Any())
        {
            foreach (var removedFile in removedFiles)
            {
                m_ModelProxies.Remove(removedFile);
                Destroy(removedFile.gameObject);
            }
        }

        if (m_DebugText == null)
            return;

        // Update the debug text if needed
        if (newFiles.Any() || removedFiles.Any())
        {
            m_DebugText.text = string.Empty;
            foreach (var file in m_ModelProxies.Select(proxy => proxy.modelPath))
                m_DebugText.text += '\n' + file;
        }
    }

    private IEnumerator CheckDirectoryEnumerator()
    {
        while (true)
        {
            if (m_CheckDirectoryTime > 0f)
                // Wait until it's time to check for new files in the Streaming Assets path
                yield return new WaitForSeconds(m_CheckDirectoryTime);

            // If the streaming assets path doesn't exist, then we can't continue until it does
            while (!Directory.Exists(Application.streamingAssetsPath))
                yield return null;

            // Check for new files
            CheckDirectory();

            yield return null;
        }
    }

    [ContextMenu("Load The Cube")]
    private void LoadTheCube()
    {
        // THIS FUNCTION IS/WAS FOR TESTING ONLY

        var objImporter = new ObjImporter();

        var cubeMesh = objImporter.ImportFile(Application.streamingAssetsPath + "\\cube.obj");

        var newGameObject = new GameObject();
        var newMeshFilter = newGameObject.AddComponent<MeshFilter>();
        var newMeshRenderer = newGameObject.AddComponent<MeshRenderer>();

        newMeshFilter.sharedMesh = cubeMesh;

        newMeshRenderer.material = new Material(Shader.Find("Standard"));
    }
}
