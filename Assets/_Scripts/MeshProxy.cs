using UnityEngine;

using ThirdPartyScripts;
using UnityEngine.Events;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshProxy : MonoBehaviour
{
    /// <summary> An instance of the ObjImporter class to be used by all ModelProxies </summary>
    private static ObjImporter s_ObjImporter;

    /// <summary> An instance of a basic sphere primitive mesh to be used by all ModelProxies </summary>
    private static Mesh s_PrimitiveBaseMesh;

    [SerializeField, Tooltip("The relative path to the model")]
    private string m_MeshPath;

    private MeshFilter m_MeshFilter;
    private MeshRenderer m_MeshRenderer;

    private Mesh m_Mesh;
    private float m_MeshVolume;
    private Vector3 m_NormalizedScale;

    /// <summary>
    /// Determines the mesh which should be used at any given time.
    /// Either the 's_PrimitiveBaseMesh'(true) or the loaded 'm_Mesh'(false
    /// </summary>
    private bool m_ProxyMode = true;

    private bool m_MeshFailedToLoad;

    public string meshPath
    {
        get { return m_MeshPath; }
        set
        {
            m_MeshPath = value;

            meshFailedToLoad = false;

            LoadMesh();
            if (m_Mesh == null)
                meshFailedToLoad = true;
        }
    }

    public Mesh mesh { get { return m_Mesh; } }

    public bool proxyMode
    {
        get { return m_ProxyMode; }
        set { m_ProxyMode = value; SetMesh(); }
    }

    public UnityEvent onMeshLoaded { get; private set; }
    public UnityEvent onMeshChanged { get; private set; }

    private bool meshFailedToLoad
    {
        get { return m_MeshFailedToLoad; }
        set
        {
            m_MeshFailedToLoad = value;

            // If the mesh fails to load, print to the console with an error message
            if (value == true)
                SendLoadError();
        }
    }

    public float meshVolume { get { return m_MeshVolume; } set { m_MeshVolume = value; SetMesh(); } }

    private static void CreatePrimitiveBaseMesh()
    {
        // Create a primitive sphere game object just like in Unity's create menu
        var tempGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        // Grab its mesh
        s_PrimitiveBaseMesh = tempGameObject.GetComponent<MeshFilter>().sharedMesh;

        // Get rid of the game object
        Destroy(tempGameObject);
    }

    private void Awake()
    {
        // Initialize all static members if they haven't been already
        if (s_ObjImporter == null)
            s_ObjImporter = new ObjImporter();

        if (s_PrimitiveBaseMesh == null)
            CreatePrimitiveBaseMesh();

        m_MeshFilter = GetComponent<MeshFilter>();

        m_MeshRenderer = GetComponent<MeshRenderer>();
        // Set the material to be the default Unity usually provides in the create menu
        m_MeshRenderer.material = new Material(Shader.Find("Standard"));

        onMeshLoaded = new UnityEvent();
        onMeshChanged = new UnityEvent();
    }

    private void Start()
    {
        // Attempt to load the mesh
        LoadMesh();
        // If it fails, make sure we know it didn't load
        if (m_Mesh == null)
            meshFailedToLoad = true;
        else
            onMeshLoaded.Invoke();

        SetMesh();

        name = m_MeshPath.Substring(m_MeshPath.LastIndexOf('/') + 1);
    }

    private void LoadMesh()
    {
        // Don't attempt to load meshs that have already failed to load
        if (m_MeshFailedToLoad)
            return;

        m_Mesh = s_ObjImporter.ImportFile(m_MeshPath);

        m_Mesh.name = m_MeshPath;

        var normalizedSize = m_Mesh.bounds.size.normalized;
        var largestValue =
            Mathf.Max(normalizedSize.x, normalizedSize.y, normalizedSize.z) /
            Mathf.Max(m_Mesh.bounds.size.x, m_Mesh.bounds.size.y, m_Mesh.bounds.size.z);

        m_NormalizedScale = new Vector3(largestValue, largestValue, largestValue);
    }

    private void SetMesh()
    {
        // Set it to either the primitive mesh(proxy) or the loaded mesh
        if (m_ProxyMode)
        {
            m_MeshFilter.sharedMesh = s_PrimitiveBaseMesh;
            transform.localScale = m_MeshVolume * Vector3.one;

            onMeshChanged.Invoke();
        }
        // Set the mesh to the mesh at 'm_MeshPath' but only if it's possible
        else if (!m_MeshFailedToLoad && m_Mesh != null)
        {
            m_MeshFilter.sharedMesh = m_Mesh;
            transform.localScale = m_MeshVolume * m_NormalizedScale;

            onMeshChanged.Invoke();
        }
    }

    private void SendLoadError()
    {
        Debug.LogError(m_MeshPath + " not able to be loaded!");
    }

    public void ToggleProxyMode() { proxyMode = !proxyMode; }
}
