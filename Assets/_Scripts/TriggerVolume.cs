using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Collider))]
public class TriggerVolume : MonoBehaviour
{
    [SerializeField]
    private Collider m_Collider;

    [SerializeField]
    private string m_TriggerName;

    public string triggerName { get { return m_TriggerName; } }

    private void Awake()
    {
        if (m_Collider == null)
            m_Collider = GetComponents<Collider>().First(collider => collider.isTrigger);
    }
}
