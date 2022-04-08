using UnityEngine;

public class RotationInput : MonoBehaviour
{
    public void IncrementRotationEuler(float x, float y, float z)
    {
        Vector3 current = transform.localEulerAngles;
        current = current + new Vector3(x, y, z);
        current.x = current.x % 360;
        current.y = current.y % 360;
        current.z = current.z % 360;

        //Debug.Log("called" +x.ToString()+" "+y.ToString() + " " + z.ToString());
        Debug.Log("called" +current);
        transform.localEulerAngles = current;
    }

    public void IncrementRotationEurlerX(float x)
    {
        IncrementRotationEuler(x, 0, 0);
    }
    public void IncrementRotationEurlerY(float y)
    {
        IncrementRotationEuler(0, y, 0);
    }
    public void IncrementRotationEurlerZ(float z)
    {
        IncrementRotationEuler(0, 0, z);
    }
}
