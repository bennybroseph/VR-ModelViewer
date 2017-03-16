using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshProxy), typeof(Rigidbody), typeof(SphereCollider))]
public class GrabbableMeshProxy : MonoBehaviour, IGrabbable
{
    private MeshProxy m_MeshProxy;

    private Rigidbody m_Rigidbody;
    private BoxCollider m_BoxCollider;
    private SphereCollider m_ProxyCollider;

    public bool currentlyHeld { get; private set; }

    // Use this for initialization
    private void Awake()
    {
        m_MeshProxy = GetComponent<MeshProxy>();
        m_Rigidbody = GetComponent<Rigidbody>();

        m_ProxyCollider = GetComponent<SphereCollider>();

        m_Rigidbody.isKinematic = true;

        m_MeshProxy.onMeshLoaded.AddListener(OnMeshLoaded);
        m_MeshProxy.onMeshChanged.AddListener(OnMeshChanged);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentlyHeld || transform.parent.GetComponent<GridLayoutGroup>())
            return;

        if (other.CompareTag("To Proxy"))
            m_MeshProxy.proxyMode = true;
        if (other.CompareTag("To Mesh"))
            m_MeshProxy.proxyMode = false;
    }

    public void Grab(Transform newParent)
    {
        transform.SetParent(newParent, true);
        transform.localPosition = Vector3.zero;

        m_Rigidbody.isKinematic = true;

        currentlyHeld = false;
    }

    public void Release()
    {
        transform.SetParent(null, true);

        m_Rigidbody.isKinematic = false;

        currentlyHeld = false;
    }

    public void Rotate(Vector3 rotation)
    {
        transform.Rotate(rotation);
    }

    private void OnMeshLoaded()
    {
        m_BoxCollider = gameObject.AddComponent<BoxCollider>();

        m_BoxCollider.center = m_MeshProxy.mesh.bounds.center;
        m_BoxCollider.size = m_MeshProxy.mesh.bounds.size;

        m_BoxCollider.enabled = false;
    }

    private void OnMeshChanged()
    {
        m_ProxyCollider.enabled = m_MeshProxy.proxyMode;

        if (m_BoxCollider != null)
            m_BoxCollider.enabled = !m_MeshProxy.proxyMode;
    }
}
