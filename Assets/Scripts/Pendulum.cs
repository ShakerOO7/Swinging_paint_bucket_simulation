using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pendulum : MonoBehaviour
{
    //public TrailRenderer trailRenderer;

    public float Gravity = 9.81f;

    [Range(2.0f, 10.0f)]
    public float Length = 2.0f;

    [Range(10.0f, 90.0f)]
    public float thetaDeg = 45.0f, phiDeg = 45.0f;
    private float theta, phi;

    [Range(10.0f, 90.0f)]
    public float thetaAngularVelocity = 45.0f, phiAngularVelocity = 45.0f;
    private float theta_v = 45.0f, phi_v=45.0f;

    //[Range(1.0f, 1000.0f)]
    //public float SphereMass = 20.0f, RodMass = 2.0f;

    [Range(0.0f, 5.0f)]
    public float BucketRaduis = 1.0f;//, RodRadius = 1.0f;

    [Range(1.0f, 10.0f)]
    public float AirDensity = 1.225f;

    float getX()
    {
        return Length * Mathf.Sin(theta) * Mathf.Cos(phi);
    }

    float getZ()
    {
        return -Length * Mathf.Sin(theta) * Mathf.Sin(phi);
    }

    float getY()
    {
        return -Length * Mathf.Cos(theta);
    }

    Vector3 getPosition()
    {
        return new Vector3(getX(), getY(), getZ());
    }

    void Start()
    {
        thetaDeg = SimVars.Pendulum.thetaDeg;
        phiDeg = SimVars.Pendulum.phiDeg;
        thetaAngularVelocity = SimVars.Pendulum.thetaAngularVelocity;
        phiAngularVelocity = SimVars.Pendulum.phiAngularVelocity;
        BucketRaduis = SimVars.Pendulum.BucketRaduis;
        AirDensity = SimVars.Pendulum.AirDensity;

        theta = Mathf.Deg2Rad * thetaDeg;
        phi = Mathf.Deg2Rad * phiDeg;
        theta_v = Mathf.Deg2Rad * thetaAngularVelocity;
        phi_v = Mathf.Deg2Rad * phiAngularVelocity;
        transform.position = getPosition();
        //if (trailRenderer != null)
        //    trailRenderer.enabled = true;
    }

    float getThetaAngularAccl(float _theta_v, float _phi_v, float _theta)
    {
        return Mathf.Pow(_phi_v, 2) * Mathf.Sin(_theta) * Mathf.Cos(_theta) - Gravity * Mathf.Sin(_theta) / Length -
            0.5f * AirDensity * Mathf.PI * Mathf.Pow(BucketRaduis, 2) * 0.5f * _theta_v / Mathf.Pow(Length, 2);
    }

    float getPhiAngularAccl(float _theta_v, float _phi_v, float _theta)
    {
        return -2 * _theta_v * _phi_v * Mathf.Cos(_theta) / Mathf.Sin(_theta) -
            0.5f * AirDensity * Mathf.PI * Mathf.Pow(BucketRaduis, 2) * 0.5f * _phi_v / Mathf.Pow(Length, 2); ;
    }

    float[] Step(float _theta_v, float _phi_v, float _theta)
    {
        float[] ret = {getThetaAngularAccl(_theta_v, _phi_v, _theta),
                       getPhiAngularAccl(_theta_v, _phi_v, _theta),
                       _theta_v, _phi_v };
        return ret;
    }

    float[] RungeKutta_5th(float dt)
    {
        // RK_5
        float[] k1 = Step(theta_v, phi_v, theta);
        float[] k2 = Step(theta_v + k1[0] * 0.5f * dt, phi_v + 0.5f * k1[1] * dt, theta + 0.5f * k1[2] * dt);
        float[] k3 = Step(theta_v + 0.5f * k2[0] * dt, phi_v + 0.5f * k2[1] * dt, theta + 0.5f * k2[2] * dt);
        float[] k4 = Step(theta_v + k3[0] * dt, phi_v + k3[1] * dt, theta + k3[2] * dt);

        float[] sum = {
            dt * (k1[0] + 2*k2[0] + 2*k3[0] + k4[0]) / 6,
            dt * (k1[1] + 2*k2[1] + 2*k3[1] + k4[1]) / 6,
            dt * (k1[2] + 2*k2[2] + 2*k3[2] + k4[2]) / 6,
            dt * (k1[3] + 2*k2[3] + 2*k3[3] + k4[3]) / 6
        };

        return sum;
    }

    void FixedUpdate()
    {
        float[] state = RungeKutta_5th(Time.fixedDeltaTime);

        theta_v += state[0];
        phi_v += state[1];

        thetaAngularVelocity = theta_v * Mathf.Rad2Deg;
        phiAngularVelocity = phi_v * Mathf.Rad2Deg;

        theta += state[2];
        phi += state[3];

        thetaDeg = theta * Mathf.Rad2Deg;
        phiDeg = phi * Mathf.Rad2Deg;

        transform.position = getPosition();
        Quaternion Orientation = Quaternion.LookRotation(new Vector3(-transform.position.x, -transform.position.y, -transform.position.z));

        Quaternion correction = Quaternion.Inverse(
                                   Quaternion.LookRotation(Vector3.up, transform.position)
                                );
        transform.rotation = Orientation * correction;

    }

    public Vector3 getLinearVelocity()
    {
        return new Vector3(theta_v / Length, phi_v / Length, 0.0f);
    }

}
