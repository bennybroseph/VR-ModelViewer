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
        if (currentlyHeld || transform.parent != null && transform.parent.GetComponent<GridLayoutGroup>())
            return;

        if (other.gameObject.GetComponent<TriggerVolume>() == null)
            return;

        var triggerVolume = other.gameObject.GetComponent<TriggerVolume>();

        if (triggerVolume.triggerName == "To Proxy")
            m_MeshProxy.proxyMode = true;
        if (triggerVolume.triggerName == "To Mesh")
            m_MeshProxy.proxyMode = false;

        if (triggerVolume.triggerName == "Grid")
        {
            transform.SetParent(triggerVolume.transform, false);
            transform.SetAsLastSibling();

            m_Rigidbody.isKinematic = true;
        }
    }

    public void Grab(Transform newParent)
    {
        transform.SetParent(newParent, true);
        transform.localPosition = new Vector3(0f, -0.01f, 0.05f);

        m_Rigidbody.isKinematic = true;

        currentlyHeld = false;
    }

    public void Release(Vector3 newVelocity, Vector3 newAngularVelocity)
    {
        transform.SetParent(null, true);

        m_Rigidbody.isKinematic = false;
        m_Rigidbody.velocity = newVelocity;
        m_Rigidbody.angularVelocity = newAngularVelocity;

        currentlyHeld = false;
    }

    public void Rotate(Vector3 rotation)
    {
        transform.Rotate(rotation);
    }

    public void Pan(Vector3 translation)
    {
        transform.localPosition += translation;
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
