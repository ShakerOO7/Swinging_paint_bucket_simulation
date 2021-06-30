using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_events : MonoBehaviour
{
    public Slider thetaDeg, phiDeg, thetaAngularVelocity, phiAngularVelocity;
    public InputField BucketRaduis, AirDensity;
    public Slider ParNum;
    public InputField Stiffness, SourceRaduis, ParMass, RestDensity, kinematicViscosity, gx, gy, gz;
    public void onClick()
    {
        SimVars.Pendulum.thetaDeg = float.Parse(thetaDeg.value.ToString());
        SimVars.Pendulum.phiDeg = float.Parse(phiDeg.value.ToString());
        SimVars.Pendulum.thetaAngularVelocity = float.Parse(thetaAngularVelocity.value.ToString());
        SimVars.Pendulum.phiAngularVelocity = float.Parse(phiAngularVelocity.value.ToString());
        SimVars.Pendulum.BucketRaduis = float.Parse(BucketRaduis.text.ToString());
        SimVars.Pendulum.AirDensity = float.Parse(AirDensity.text.ToString());

        SimVars.Fluid.ParNum = (int) ParNum.value;
        SimVars.Fluid.Stiffness = float.Parse(Stiffness.text.ToString());
        SimVars.Fluid.SourceRaduis = float.Parse(SourceRaduis.text.ToString());
        SimVars.Fluid.ParMass = float.Parse(ParMass.text.ToString());
        SimVars.Fluid.RestDensity = float.Parse(RestDensity.text.ToString());
        SimVars.Fluid.kinematicViscosity = float.Parse(kinematicViscosity.text.ToString());
        SimVars.Fluid.Gravity = new Vector3(float.Parse(gx.text.ToString()), 
            float.Parse(gy.text.ToString()), float.Parse(gz.text.ToString()));
        SceneManager.LoadScene(1);
    }
}
