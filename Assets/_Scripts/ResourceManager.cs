using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using ThirdPartyScripts;

public class ResourceManager : MonoBehaviour
{
    [SerializeField]
    private GameObject m_GridLayoutAnchor;
    [SerializeField]
    private GridLayoutGroup m_GridLayoutGroupPrefab;

    [Space, SerializeField, Range(0f, 10f)]
    private float m_CheckDirectoryTime = 1f;

    [Space, SerializeField, Range(0.1f, 10f)]
    private float m_MeshVolume = 0.5f;

    [Header("Debugging"), SerializeField]
    private Text m_DebugText;

    private GridLayoutGroup m_GridLayoutGroup;

    private List<MeshProxy> m_MeshProxies = new List<MeshProxy>();

    private float m_PrevModelVolume;

    public float MeshVolume { get { return m_MeshVolume; } set { m_MeshVolume = value; OnValidate(); } }

    private void OnValidate()
    {
        if (m_PrevModelVolume != m_MeshVolume)
        {
            foreach (var meshProxy in m_MeshProxies)
                meshProxy.meshVolume = m_MeshVolume;

            m_PrevModelVolume = m_MeshVolume;
        }
    }

    private void Awake()
    {
        if (m_GridLayoutGroupPrefab == null)
            return;

        m_GridLayoutGroup = Instantiate(m_GridLayoutGroupPrefab);
        m_GridLayoutGroup.transform.SetParent(m_GridLayoutAnchor.transform, false);
    }

    private void Start()
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
                file => !m_MeshProxies.Select(proxy => proxy.meshPath).Contains(file) && (
                file.EndsWith(".obj") || file.EndsWith(".fbx"))).ToList();

        // A new Model was added to the directory since last checked
        if (newFiles.Any())
        {
            foreach (var newFile in newFiles)
            {
                var newGameObject = CreateGrabbableMeshProxy(newFile);
                m_MeshProxies.Add(newGameObject.GetComponent<MeshProxy>());
            }
        }

        var removedFiles = m_MeshProxies.Where(proxy => !files.Contains(proxy.meshPath)).ToList();

        // A Model was removed from the directory since last checked
        if (removedFiles.Any())
        {
            foreach (var removedFile in removedFiles)
            {
                m_MeshProxies.Remove(removedFile);
                Destroy(removedFile.gameObject);
            }
        }

        if (m_DebugText == null)
            return;

        // Update the debug text if needed
        if (newFiles.Any() || removedFiles.Any())
        {
            m_DebugText.text = string.Empty;
            foreach (var file in m_MeshProxies.Select(proxy => proxy.meshPath))
                m_DebugText.text += '\n' + file;
        }
    }

    public GameObject CreateGrabbableMeshProxy(string newMeshPath)
    {
        var newGameObject = new GameObject();
        newGameObject.AddComponent<RectTransform>();

        var newGrabbableMeshProxy = newGameObject.AddComponent<GrabbableMeshProxy>();
        var newMeshProxy = newGameObject.GetComponent<MeshProxy>();

        newGameObject.transform.SetParent(m_GridLayoutGroup.transform, false);
        newGameObject.transform.Rotate(new Vector3(90f, 0f, 0f));

        newMeshProxy.meshPath = newMeshPath;
        newMeshProxy.meshVolume = m_MeshVolume;

        return newGameObject;
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
