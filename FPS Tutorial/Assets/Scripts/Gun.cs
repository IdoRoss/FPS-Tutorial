using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//This class is ment to define a guns behavior
public class Gun : MonoBehaviour
{
    public bool isAutomatic;
    public float timeBetweenShots = 0.08f, heatPerShot = 1f;
    public GameObject muzzleFlash;
    public int damage;
}
