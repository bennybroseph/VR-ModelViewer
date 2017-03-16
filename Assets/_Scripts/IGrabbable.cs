using UnityEngine;

public interface IGrabbable
{
    bool currentlyHeld { get; }

    void Grab(Transform newParent);
    void Release();

    void Rotate(Vector3 rotation);
}
