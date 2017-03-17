using System.Linq;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private SteamVR_TrackedObject m_RightController;
    [SerializeField]
    private SteamVR_TrackedObject m_LeftController;

    private IGrabbable m_HeldObject;

    private void Update()
    {
        if (!m_RightController.isValid)
            return;

        var controllerData = SteamVR_Controller.Input((int)m_RightController.index);
        if (controllerData.GetHairTriggerDown())
        {
            if (m_HeldObject == null)
            {
                var rayCastHits =
                    Physics.RaycastAll(
                        new Ray(m_RightController.transform.position, m_RightController.transform.forward));

                var grabbableObjects =
                    rayCastHits.
                        Select(raycastHit => raycastHit.transform.gameObject.GetComponent<IGrabbable>()).ToList();

                if (grabbableObjects.Any())
                {
                    var grabbableObject = grabbableObjects.First(grabbable => grabbable != null);
                    grabbableObject.Grab(m_RightController.transform);
                }
            }
        }
        else if (m_HeldObject != null)
        {
            m_HeldObject.Release(controllerData.velocity);
            m_HeldObject = null;
        }
    }

    private void OnRenderObject()
    {
        CreateLineMaterial();
        lineMaterial.SetPass(0);

        if (!m_RightController.isValid)
            return;

        var start = m_RightController.transform.position;
        var end = start + 5f * m_RightController.transform.forward;

        GL.Begin(GL.LINES);
        {
            GL.Color(Color.cyan);

            GL.Vertex3(start.x, start.y, start.z);
            GL.Vertex3(end.x, end.y, end.z);
        }
        GL.End();
    }

    static Material lineMaterial;

    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }
}
