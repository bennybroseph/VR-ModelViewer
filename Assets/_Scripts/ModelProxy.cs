using UnityEngine;

using ThirdPartyScripts;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ModelProxy : MonoBehaviour
{
    /// <summary> An instance of the ObjImporter class to be used by all ModelProxies </summary>
    private static ObjImporter s_ObjImporter;

    /// <summary> An instance of a basic sphere primitive mesh to be used by all ModelProxies </summary>
    private static Mesh s_PrimitiveBaseMesh;

    [SerializeField, Tooltip("The relative path to the model")]
    private string m_ModelPath;

    private MeshFilter m_MeshFilter;
    private MeshRenderer m_MeshRenderer;

    private Mesh m_Mesh;

    /// <summary>
    /// Determines the mesh which should be used at any given time.
    /// Either the 's_PrimitiveBaseMesh'(true) or the loaded 'm_Mesh'(false
    /// </summary>
    private bool m_ProxyMode = true;

    public string modelPath { get { return m_ModelPath; } set { m_ModelPath = value; } }

    public bool proxyMode
    {
        get { return m_ProxyMode; }
        set { m_ProxyMode = value; SetMesh(); }
    }

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
    }

    private void Start()
    {
        SetMesh();

        name = m_ModelPath.Substring(m_ModelPath.LastIndexOf('/') + 1);
    }

    private void SetMesh()
    {
        // Load the mesh if it hasn't been already
        if (m_Mesh == null)
        {
            m_Mesh = s_ObjImporter.ImportFile(m_ModelPath);
            m_Mesh.name = m_ModelPath;
        }

        // Set it to either the primitive mesh(proxy) or the loaded mesh
        m_MeshFilter.sharedMesh =
            m_ProxyMode ?
                s_PrimitiveBaseMesh :
                m_Mesh;
    }

    public void ToggleProxyMode() { proxyMode = !proxyMode; }
}
