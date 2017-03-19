namespace BennyBrosephVR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Library;
    using UnityEngine;
    using UnityEngine.Events;
    using Valve.VR;

    public enum DeviceOrientation
    {
        Right,
        Left,

        Primary,
        Secondary,
    }

    public enum DevicePreference
    {
        Right,
        Left,
    }

    /// <summary> Simple enum to keep track of a controller's button's state </summary>
    public enum ButtonState
    {
        None,
        Press,
        Hold,
        Release,
    }

    /// <summary>
    /// A wrapper to make SteamVR's input more "Unity-Like" since this is a Unity Script after all.
    /// Uses functions which match the functionality of standard Unity Input functions like 'GetButtonDown'
    /// One thing it does that Unity does not is provide the ability to add a listener for Input events
    /// </summary>
    public class InputWrapper : MonoSingleton<InputWrapper>
    {
        #region DATA_TYPES
        /// <summary> Used to hold information about a controller's current state </summary>
        private class ControllerState
        {
            /// <summary> The current position the player is touching the controller's touchpad
            /// </summary>
            public Vector2 touchpadPosition;
            /// <summary> The last position the player touched on the controller's touchpad </summary>
            public Vector2 prevTouchpadPosition;

            /// <summary> Stores all of a controller's button states by its 'EVRButtonId' </summary>
            public Dictionary<EVRButtonId, ButtonState> buttonStates =
                new Dictionary<EVRButtonId, ButtonState>();

            public ControllerState()
            {
                // Populate the 'buttonStates' dictionary with every possible 'EVRButtonId'
                // set to 'ButtonState.None' so that there is no issue with getting an
                // exception when searching for a button's state
                foreach (var value in Enum.GetValues(typeof(EVRButtonId)))
                    buttonStates.Add((EVRButtonId)value, ButtonState.None);
            }
        }

        [Serializable]
        public class SteamVR_ControllerButtonEvent : UnityEvent<SteamVR_Controller.Device, ButtonState> { }
        [Serializable]
        public class SteamVR_ControllerAxisEvent : UnityEvent<SteamVR_Controller.Device, Vector2, Vector2> { }

        #endregion

        [SerializeField]
        private DevicePreference m_DevicePreference;

        [Space, SerializeField, Tooltip("Triggered whenever a button's state changes")]
        private SteamVR_ControllerButtonEvent m_OnTrigger = new SteamVR_ControllerButtonEvent();
        [SerializeField]
        private SteamVR_ControllerButtonEvent m_OnRightTrigger = new SteamVR_ControllerButtonEvent();
        [SerializeField]
        private SteamVR_ControllerButtonEvent m_OnLeftTrigger = new SteamVR_ControllerButtonEvent();

        [SerializeField, Tooltip("Triggered whenever a touchpad's axis value changes")]
        private SteamVR_ControllerAxisEvent m_OnTouchpadDelta = new SteamVR_ControllerAxisEvent();

        private Dictionary<int, ControllerState> m_ControllerStates =
            new Dictionary<int, ControllerState>();

        public DevicePreference devicePreference
        {
            get { return m_DevicePreference; }
            set { m_DevicePreference = value; }
        }

        public SteamVR_ControllerButtonEvent onTrigger { get { return m_OnTrigger; } }
        public SteamVR_ControllerAxisEvent onTouchpadDelta { get { return m_OnTouchpadDelta; } }

        protected override void Awake() { base.Awake(); }

        private void Update()
        {
            for (var i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; ++i)
            {
                if (!m_ControllerStates.ContainsKey(i))
                    m_ControllerStates.Add(i, new ControllerState());

                ParseDeviceInput(SteamVR_Controller.Input(i));
            }
        }

        private void ParseDeviceInput(SteamVR_Controller.Device device)
        {
            if (device.valid == false)
                return;

            var deviceIndex = (int)device.index;
            var controllerState = m_ControllerStates[deviceIndex];

            var triggerIndex = EVRButtonId.k_EButton_SteamVR_Trigger;
            if (device.GetPressDown(triggerIndex))
            {
                controllerState.buttonStates[triggerIndex] = ButtonState.Press;
                m_OnTrigger.Invoke(device, ButtonState.Press);
            }
            else if (device.GetPress(triggerIndex))
            {
                controllerState.buttonStates[triggerIndex] = ButtonState.Hold;
                m_OnTrigger.Invoke(device, ButtonState.Hold);
            }
            else if (device.GetPressUp(triggerIndex))
            {
                controllerState.buttonStates[triggerIndex] = ButtonState.Release;
                m_OnTrigger.Invoke(device, ButtonState.Release);
            }
            else
                controllerState.buttonStates[triggerIndex] = ButtonState.None;

            var touchpadPosition = device.GetAxis();
            if (touchpadPosition != controllerState.prevTouchpadPosition)
            {
                m_OnTouchpadDelta.Invoke(
                    device,
                    touchpadPosition,
                    controllerState.prevTouchpadPosition - touchpadPosition);

                controllerState.prevTouchpadPosition = touchpadPosition;
            }
        }

        #region GET_BUTTON
        /// <summary>
        /// Returns true during the frame the user pressed down the specified button on the specified device
        /// </summary>
        /// <param name="deviceIndex">The device index to check against</param>
        /// <param name="buttonId">The button enum to check against</param>
        public bool GetButtonDown(int deviceIndex, EVRButtonId buttonId)
        {
            return m_ControllerStates[deviceIndex].buttonStates[buttonId] == ButtonState.Press;
        }
        /// <summary>
        /// Returns true while the specified button on the specified device is held down
        /// </summary>
        /// <param name="deviceIndex">The device index to check against</param>
        /// <param name="buttonId">The button enum to check against</param>
        public bool GetButton(int deviceIndex, EVRButtonId buttonId)
        {
            return m_ControllerStates[deviceIndex].buttonStates[buttonId] == ButtonState.Hold;
        }
        /// <summary>
        /// Returns true during the frame the user releases the specified button on the specified device
        /// </summary>
        /// <param name="deviceIndex">The device index to check against</param>
        /// <param name="buttonId">The button enum to check against</param>
        public bool GetButtonUp(int deviceIndex, EVRButtonId buttonId)
        {
            return m_ControllerStates[deviceIndex].buttonStates[buttonId] == ButtonState.Release;
        }

        #endregion

        #region GET_DEVICE_INDEX

        public int GetRightDeviceIndex()
        {
            return SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
        }
        public int GetLeftDeviceIndex()
        {
            return SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
        }
        public int GetPrimaryDeviceIndex()
        {
            return
                SteamVR_Controller.GetDeviceIndex(
                    m_DevicePreference == DevicePreference.Right
                        ? SteamVR_Controller.DeviceRelation.Rightmost
                        : SteamVR_Controller.DeviceRelation.Leftmost);
        }
        public int GetSecondaryDeviceIndex()
        {
            return
                SteamVR_Controller.GetDeviceIndex(
                    m_DevicePreference == DevicePreference.Left
                        ? SteamVR_Controller.DeviceRelation.Leftmost
                        : SteamVR_Controller.DeviceRelation.Rightmost);
        }

        #endregion

        [CanBeNull]
        public SteamVR_TrackedObject GetRightTrackedObject()
        {
            return FindObjectsOfType<SteamVR_TrackedObject>().First(
                trackedObject => (int)trackedObject.index == GetRightDeviceIndex());
        }
        [CanBeNull]
        public SteamVR_TrackedObject GetLeftTrackedObject()
        {
            return FindObjectsOfType<SteamVR_TrackedObject>().First(
                trackedObject => (int)trackedObject.index == GetLeftDeviceIndex());
        }
        [CanBeNull]
        public SteamVR_TrackedObject GetPrimaryTrackedObject()
        {
            return FindObjectsOfType<SteamVR_TrackedObject>().First(
                trackedObject => (int)trackedObject.index == GetPrimaryDeviceIndex());
        }
        [CanBeNull]
        public SteamVR_TrackedObject GetSecondaryTrackedObject()
        {
            return FindObjectsOfType<SteamVR_TrackedObject>().First(
                trackedObject => (int)trackedObject.index == GetSecondaryDeviceIndex());
        }

        #region GET_BUTTON_WITH_ORIENTATION
        /// <summary>
        /// Returns true during the frame the user pressed down the specified button on the specified device
        /// </summary>
        /// <param name="deviceOrientation">
        /// The device's orientation to the player you want to check against
        /// </param>
        /// <param name="buttonId">The button enum to check against</param>

        public bool GetButtonDown(DeviceOrientation deviceOrientation, EVRButtonId buttonId)
        {
            switch (deviceOrientation)
            {
                case DeviceOrientation.Right:
                    return GetButtonDown(GetRightDeviceIndex(), buttonId);
                case DeviceOrientation.Left:
                    return GetButtonDown(GetLeftDeviceIndex(), buttonId);
                case DeviceOrientation.Primary:
                    return GetButtonDown(GetPrimaryDeviceIndex(), buttonId);
                case DeviceOrientation.Secondary:
                    return GetButtonDown(GetSecondaryDeviceIndex(), buttonId);

                default:
                    throw new ArgumentOutOfRangeException("deviceOrientation", deviceOrientation, null);
            }

        }
        /// <summary>
        /// Returns true while the specified button on the specified device is held down
        /// </summary>
        /// <param name="deviceOrientation">
        /// The device's orientation to the player you want to check against
        /// </param>
        /// <param name="buttonId">The button enum to check against</param>
        public bool GetButton(DeviceOrientation deviceOrientation, EVRButtonId buttonId)
        {
            switch (deviceOrientation)
            {
                case DeviceOrientation.Right:
                    return GetButton(GetRightDeviceIndex(), buttonId);
                case DeviceOrientation.Left:
                    return GetButton(GetLeftDeviceIndex(), buttonId);
                case DeviceOrientation.Primary:
                    return GetButton(GetPrimaryDeviceIndex(), buttonId);
                case DeviceOrientation.Secondary:
                    return GetButton(GetSecondaryDeviceIndex(), buttonId);

                default:
                    throw new ArgumentOutOfRangeException("deviceOrientation", deviceOrientation, null);
            }
        }
        /// <summary>
        /// Returns true during the frame the user releases the specified button on the specified device
        /// </summary>
        /// <param name="deviceOrientation">
        /// The device's orientation to the player you want to check against
        /// </param>
        /// <param name="buttonId">The button enum to check against</param>
        public bool GetButtonUp(DeviceOrientation deviceOrientation, EVRButtonId buttonId)
        {
            switch (deviceOrientation)
            {
                case DeviceOrientation.Right:
                    return GetButtonUp(GetRightDeviceIndex(), buttonId);
                case DeviceOrientation.Left:
                    return GetButtonUp(GetLeftDeviceIndex(), buttonId);
                case DeviceOrientation.Primary:
                    return GetButtonUp(GetPrimaryDeviceIndex(), buttonId);
                case DeviceOrientation.Secondary:
                    return GetButtonUp(GetSecondaryDeviceIndex(), buttonId);

                default:
                    throw new ArgumentOutOfRangeException("deviceOrientation", deviceOrientation, null);
            }
        }

        #endregion
    }
}