using UnityEngine;

public interface IGrabbable
{
    bool currentlyHeld { get; }

    void Grab(Transform newParent);
    void Release(Vector3 newVelocity, Vector3 newAngularVelocity);

    void Rotate(Vector3 rotation);

    void Pan(Vector3 translation);
}
