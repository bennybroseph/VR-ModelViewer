using System.Linq;
using Library;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace BennyBrosephVR
{
    using System;
    using JetBrains.Annotations;

    public class InteractionManager : MonoSingleton<InteractionManager>
    {
        private enum TouchpadMode
        {
            Rotate,
            Pan,
        }

        [SerializeField]
        private Canvas m_ControllerCanvasPrefab;

        [SerializeField]
        private float m_RotationCoefficient;
        [SerializeField]
        private float m_TranslationCoefficient;

        private Canvas m_ControllerCanvas;
        private Text m_Text;

        private GameObject m_HighlightedObject;
        private GameObject m_HeldObject;

        private TouchpadMode m_TouchpadMode;

        private bool m_TriggerIsHeld;
        private Vector2 m_PrevAxis;

        protected override void Awake()
        {
            base.Awake();

            m_ControllerCanvas = Instantiate(m_ControllerCanvasPrefab);
            m_ControllerCanvas.worldCamera = Camera.main;

            m_Text = m_ControllerCanvas.GetComponentInChildren<Text>();
            m_Text.text = string.Empty;
        }

        protected void Start()
        {
            if (InputWrapper.self.GetRightTrackedObject() != null)
                m_ControllerCanvas.transform.SetParent(InputWrapper.self.GetRightTrackedObject().transform);

            InputWrapper.self.onTrigger.AddListener(OnTrigger);
            InputWrapper.self.onTouchpad.AddListener(OnTouchpad);
            InputWrapper.self.onTouchpadDelta.AddListener(OnTouchpadDelta);
        }

        private void LateUpdate()
        {
            var primaryController = InputWrapper.self.GetPrimaryTrackedObject();
            if (primaryController == null || !primaryController.isValid)
                return;

            m_HighlightedObject = GetFirstGrabbable();
            if (m_HighlightedObject == null)
            {
                m_Text.text = string.Empty;
                return;
            }

            m_Text.text = m_HighlightedObject.name;
        }

        private void OnRenderObject()
        {
            var primaryController = InputWrapper.self.GetPrimaryTrackedObject();
            if (primaryController == null || !primaryController.isValid)
                return;

            CreateLineMaterial();
            s_LineMaterial.SetPass(0);

            var start = primaryController.transform.position;
            var end = start + 5f * primaryController.transform.forward;

            GL.Begin(GL.LINES);
            {
                GL.Color(Color.cyan);

                GL.Vertex3(start.x, start.y, start.z);
                GL.Vertex3(end.x, end.y, end.z);
            }
            GL.End();
        }

        private void OnTrigger(SteamVR_Controller.Device device, ButtonState buttonState)
        {
            if (device.index != InputWrapper.self.GetPrimaryDeviceIndex())
                return;

            var primaryController = InputWrapper.self.GetPrimaryTrackedObject();
            if (primaryController == null || !primaryController.isValid)
                return;

            switch (buttonState)
            {
                case ButtonState.None:
                    break;
                case ButtonState.Press:
                    if (m_HeldObject == null && m_HighlightedObject != null)
                    {
                        var highlightedGrabbable = m_HighlightedObject.GetComponent<IGrabbable>();
                        highlightedGrabbable.Grab(
                            primaryController.transform.
                                FindChild("Model").
                                FindChild("tip").GetChild(0));

                        m_HeldObject = m_HighlightedObject;
                    }
                    else if (m_HeldObject != null)
                    {
                        var heldGrabbable = m_HeldObject.GetComponent<IGrabbable>();
                        heldGrabbable.Release(device.velocity, device.angularVelocity);

                        m_HeldObject = null;
                    }
                    break;
                case ButtonState.Hold:
                    break;
                case ButtonState.Release:
                    break;

                default:
                    throw new ArgumentOutOfRangeException("buttonState", buttonState, null);
            }
        }

        private void OnTouchpad(SteamVR_Controller.Device device, ButtonState buttonState)
        {
            if (device.index != InputWrapper.self.GetPrimaryDeviceIndex())
                return;

            switch (buttonState)
            {
                case ButtonState.None:
                    break;
                case ButtonState.Press:
                    switch (m_TouchpadMode)
                    {
                        case TouchpadMode.Rotate:
                            m_TouchpadMode = TouchpadMode.Pan;
                            break;
                        case TouchpadMode.Pan:
                            m_TouchpadMode = TouchpadMode.Rotate;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case ButtonState.Hold:
                    break;
                case ButtonState.Release:

                    break;

                default:
                    throw new ArgumentOutOfRangeException("buttonState", buttonState, null);
            }
        }

        private void OnTouchpadDelta(SteamVR_Controller.Device device, Vector2 position, Vector2 delta)
        {
            if (device.index != InputWrapper.self.GetPrimaryDeviceIndex())
                return;

            if (m_HeldObject == null)
                return;

            if (m_TouchpadMode == TouchpadMode.Rotate)
                m_HeldObject.GetComponent<IGrabbable>().
                    Rotate(new Vector3(delta.y, delta.x, 0f) * m_RotationCoefficient);
            if (m_TouchpadMode == TouchpadMode.Pan)
                m_HeldObject.GetComponent<IGrabbable>().
                    Pan(new Vector3(0f, delta.x, delta.y) * m_TranslationCoefficient);
        }

        [CanBeNull]
        private static GameObject GetFirstGrabbable()
        {
            var primaryTrackedObject = InputWrapper.self.GetPrimaryTrackedObject();
            if (primaryTrackedObject == null)
                return null;

            var rayCastHits =
                Physics.RaycastAll(
                    new Ray(
                        primaryTrackedObject.transform.position,
                        primaryTrackedObject.transform.forward));
            if (!rayCastHits.Any())
                return null;

            var grabbableObjects =
                rayCastHits.
                    Where(raycastHit => raycastHit.transform.gameObject.GetComponent<IGrabbable>() != null).
                    ToList();

            if (grabbableObjects.Any())
                return grabbableObjects.First().transform.gameObject;

            return null;
        }

        static Material s_LineMaterial;

        private static void CreateLineMaterial()
        {
            if (!s_LineMaterial)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                s_LineMaterial = new Material(shader);
                s_LineMaterial.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                s_LineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                s_LineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                s_LineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                s_LineMaterial.SetInt("_ZWrite", 0);
            }
        }
    }
}
