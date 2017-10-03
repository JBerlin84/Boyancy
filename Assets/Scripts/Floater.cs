// File: Floater.cs
// Description: Apply forces to a floating object when hitting a Liquid object.
// Date: 2016-03-21
// Written by: Jimmy Berlin

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Floater : MonoBehaviour {

    Transform t;
	Rigidbody rb;
    float updateFrequency = 0.1f;             // delay between updates in s.
    float nextUpdate;

    float defaultAngularDragCoefficient;
    float defaultDragCoefficient;

    public float submergedDragCoefficient = 5;
    public float angularDragCoefficient = 5;

	// Use this for initialization
	void Awake () {
        t = GetComponent<Transform> ();
		rb = GetComponent<Rigidbody> ();

        defaultAngularDragCoefficient = rb.angularDrag;
        defaultDragCoefficient = rb.drag;
	}

	/// <summary>
	/// Raises trigger event when objects are currently colliding.
	/// </summary>
	/// <param name="other">Other.</param>
	void OnTriggerStay(Collider other) {
		if(other.GetComponent<Liquid> ()) {
            if (nextUpdate < Time.time) {       // We dont need to update every single time, 10 times per second is enough
                bool submerged = false;
                float percentUnderWater = 1f;
                Vector3 centreOfBuoyancy = GetCentreOfBuoyancy(other, out percentUnderWater, out submerged);

                Vector3 drag = GetDrag(other.gameObject);
                Vector3 bouyancy = GetBouyancy(other.gameObject, percentUnderWater)*8f;
                Vector3 force = drag + bouyancy;

                // If center of boyancy is 0, then we increase the drag to stop spinning as the
                // physics engine follows newtons first law.
                if (submerged)
                {
                    print("submerged");
                    rb.angularDrag = CalculateAngularDrag(other);
                    rb.drag = submergedDragCoefficient;
                    rb.AddForce(force);
                }
                else
                {
                    print("not submerged");
                    rb.angularDrag = defaultAngularDragCoefficient;
                    if (percentUnderWater > 0)
                    {
                        rb.drag = submergedDragCoefficient * percentUnderWater;
                        rb.AddForceAtPosition(force, centreOfBuoyancy);
                    }
                    else
                    {
                        rb.drag = defaultDragCoefficient;
                    }
                }
                nextUpdate = Time.time + updateFrequency;
            }
		}
	}

    /// <summary>
    /// Calculate the force for the bouyancy to apply based on the liquid and this object
    /// has to have a Liquid script applied.
    /// </summary>
    /// <returns>The "boyancy" force.</returns>
    /// <param name="liquid">Liquid gameobject.</param>
    private Vector3 GetBouyancy(GameObject liquidObject, float percentUnderWater) {
        Vector3 force = Vector3.zero;

        try {
            Liquid liquid = liquidObject.GetComponent<Liquid>();

            // calculate boyancy to object.
            // Calculate the volume of ourselves in m3.
            float volume = transform.localScale.x * transform.localScale.y * transform.localScale.z;
            // Calculate the volume of the displaced water in m3
            float volumeOfDisplacedWater = volume * percentUnderWater;
            
            force = Vector3.up * liquid.density * volumeOfDisplacedWater;
        }
        catch (MissingComponentException e) {
            Debug.LogError("Could not calculate the force to apply to floating object.\n");
            Debug.LogException(e);
        }

        return force;
    }

    /// <summary>
    /// Calculates the angular drag based on the drag equation.
    /// </summary>
    /// <param name="other">liquid collider object</param>
    /// <returns>angular drag for when the object is completely under liquid</returns>
    private float CalculateAngularDrag(Collider other) {
        float p = other.gameObject.GetComponent<Liquid>().density;                      // Liquid mass density
        float u = rb.angularVelocity.x + rb.angularVelocity.y + rb.angularVelocity.z;   // total angular velocity
        float CD = angularDragCoefficient;
        float A = t.localScale.x * t.localScale.y + t.localScale.x * t.localScale.z + t.localScale.y * t.localScale.z;
        float FD = 0.5f * p * u * u * CD * A;

        return FD / 10000;
    }

	/// <summary>
	/// Calculate the drag to apply to this gameobject based on the liquid and this
	/// has to have a Liquid script applied.
	/// </summary>
	/// <returns>The "boyancy" force.</returns>
	/// <param name="liquid">Liquid gameobject.</param>
	private Vector3 GetDrag(GameObject liquidObject) {
		Vector3 force = Vector3.zero;

		try {
            Liquid liquid = liquidObject.GetComponent<Liquid>();

			// add drag of water
			force = rb.velocity * -1 * liquid.viscosity;
			// add boyancy to object.
			//force += Vector3.up*10;
            force += Vector3.up * liquid.density;

		} catch (MissingComponentException e) {
			Debug.LogError ("Could not calculate the force to apply to floating object.\n");
			Debug.LogException (e);
		}

		return force;
	}

    
	/// <summary>
	/// Calculate and return the centerOfBoyancy of the object.
	/// </summary>
	/// <param name="other">liquid box.</param>
    private Vector3 GetCentreOfBuoyancy(Collider other, out float percentUnderWater, out bool submerged) {

        submerged = false;
        float totalPoints = 8f;
        
        List<Vector3> allPoints = new List<Vector3>();
        List<Vector3> pointsInLiquid = new List<Vector3>();

        // These are temporary, just to make everything easier.
        // NOTE: Might be removed for a itty bitty performance;
        Vector3 up = Vector3.up;
        Vector3 down = Vector3.down;
        Vector3 left = Vector3.left;
        Vector3 right = Vector3.right;
        Vector3 forward = Vector3.forward;
        Vector3 back = Vector3.back;

        allPoints.Add(up + left + forward);
        allPoints.Add(up + left - forward);
        allPoints.Add(up - left + forward);
        allPoints.Add(up - left - forward);
        allPoints.Add(-up + left + forward);
        allPoints.Add(-up + left - forward);
        allPoints.Add(-up - left + forward);
        allPoints.Add(-up - left - forward);

        // scale all points according to half of the scale to
        // Get all the points at the correct place.
        Vector3 halfScale = t.localScale / 2;
        for (int i = 0; i < allPoints.Count; i++) {
            allPoints[i] = Vector3.Scale(allPoints[i], halfScale);
        }

        // Put them all in correct position according to objects world position
        Vector3 worldPos = t.position;
        for (int i = 0; i < allPoints.Count; i++)
        {
            allPoints[i] = worldPos + allPoints[i];
        }

        // Rotate all points according to the objects rotation in world space
        Quaternion rotation = t.rotation;
        for (int i = 0; i < allPoints.Count; i++)
        {
            allPoints[i] = RotatePointAroundPivot(allPoints[i], worldPos, rotation);
        }

        // Check which corners are in the liquid
        foreach(Vector3 v in allPoints) {
            if (other.bounds.Contains(v)) {
                pointsInLiquid.Add(v);
            }
        }

        // Calculate rough estimate of percent under water.
        percentUnderWater = pointsInLiquid.Count / totalPoints;
        if (percentUnderWater == 1) {
            submerged = true;
        }

        // Calculate the center of all points in the liquid.
        Vector3 avg = Vector3.zero;
        if (!submerged) {
            Vector3 sum = Vector3.zero;
            foreach (Vector3 v in pointsInLiquid) {
                sum += v;
            }
            avg = sum / pointsInLiquid.Count;
        }

        return avg;
	}

    /// <summary>
    /// Rotate point around pivot according to the angle given in angle.
    /// </summary>
    /// <param name="point">point to rotate</param>
    /// <param name="pivot">pivot to rotate aroind</param>
    /// <param name="angle">angle to rotate</param>
    /// <returns>new point</returns>
    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion angle) {
        Vector3 direction = point - pivot;
        direction = angle * direction;
        return direction + pivot;
    }
}
