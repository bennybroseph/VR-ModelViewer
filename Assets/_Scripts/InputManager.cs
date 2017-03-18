using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private SteamVR_TrackedObject m_RightController;
    [SerializeField]
    private SteamVR_TrackedObject m_LeftController;

    [SerializeField]
    private Canvas m_ControllerCanvasPrefab;

    private Canvas m_ControllerCanvas;
    private Text m_Text;

    private IGrabbable m_HeldObject;

    private bool m_TriggerWasHeld;

    private void Awake()
    {
        m_ControllerCanvas = Instantiate(m_ControllerCanvasPrefab);
        m_ControllerCanvas.worldCamera = Camera.main;

        m_Text = m_ControllerCanvas.GetComponentInChildren<Text>();
        m_Text.text = string.Empty;

        m_ControllerCanvas.transform.SetParent(m_RightController.transform);
    }

    private void Update()
    {
        if (!m_RightController.isValid)
            return;

        var rayCastHits =
                    Physics.RaycastAll(
                        new Ray(m_RightController.transform.position, m_RightController.transform.forward));

        var grabbableObjects =
            rayCastHits.
                Where(raycastHit => raycastHit.transform.gameObject.GetComponent<IGrabbable>() != null).
                Select(rayCastHit => rayCastHit.transform.gameObject).ToList();
        if (!grabbableObjects.Any())
            return;

        var grabbableObject = grabbableObjects.First();

        m_Text.text = grabbableObject.name;

        var controllerData = SteamVR_Controller.Input((int)m_RightController.index);
        if (controllerData.GetHairTriggerDown())
        {
            if (m_HeldObject == null)
            {
                var grabbableComponent = grabbableObject.GetComponent<IGrabbable>();
                grabbableComponent.Grab(m_RightController.transform);
                m_HeldObject = grabbableComponent;
            }

            m_TriggerWasHeld = true;
        }
        else if (!m_TriggerWasHeld && m_HeldObject != null)
        {
            m_HeldObject.Release(controllerData.velocity);
            m_HeldObject = null;
        }
        else if (controllerData.GetHairTrigger() == false)
            m_TriggerWasHeld = false;
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
