using System.Linq;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private SteamVR_TrackedObject m_RightController;
    [SerializeField]
    private SteamVR_TrackedObject m_LeftController;

    private void Update()
    {
        if (!m_RightController.isValid)
            return;

        var controllerData = SteamVR_Controller.Input((int)m_RightController.index);
        if (controllerData.GetHairTriggerDown())
        {
            var rayCastHits =
                Physics.RaycastAll(
                    new Ray(m_RightController.transform.position, m_RightController.transform.forward));

            foreach (var raycastHit in rayCastHits)
            {
                var grabbable = raycastHit.transform.gameObject.GetComponent<IGrabbable>();
                if (grabbable != null)
                    grabbable.Grab(m_RightController.transform);
            }
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
