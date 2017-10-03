// File: Liquid.cs
// Description: Holds the physical units for a liquid
// Date: 2016-03-18
// Written by: Jimmy Berlin

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Liquid : MonoBehaviour {

    [Header("Physical units")]
	public float viscosity;     // Gigen in mPa*s
    public float density;       // Given in kg/cm3

}
