using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimVars
{
    public static class Pendulum
    {
        public static float thetaDeg, phiDeg, thetaAngularVelocity, phiAngularVelocity, BucketRaduis, AirDensity;
    }
    public static class Fluid
    {
        public static int ParNum;
        public static float Stiffness, SourceRaduis, ParMass, RestDensity, kinematicViscosity;
        public static Vector3 Gravity;
    }
}
