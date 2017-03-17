using UnityEngine;

public interface IGrabbable
{
    bool currentlyHeld { get; }

    void Grab(Transform newParent);
    void Release(Vector3 newVelocity);

    void Rotate(Vector3 rotation);
}
