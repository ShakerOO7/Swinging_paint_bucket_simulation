using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class sph_math
{
    static float kernel(float q)
    {
        if (q < 1)
        {
            return 3.0f / (2 * Mathf.PI) * (2.0f / 3.0f - q * q + 0.5f * q * q * q);
        }
        else if (q < 2)
        {
            return 3.0f / (2 * Mathf.PI) * (1.0f / 6.0f * Mathf.Pow(2.0f - q, 3.0f));
        }
        return 0.0f;
    }

    static float dkernel(float q)
    {
        if (q < 1)
        {
            return 3.0f / (2.0f * Mathf.PI) * (-2.0f * q + 1.5f * q * q);
        }
        else if (q < 2)
        {
            return 3.0f / (2.0f * Mathf.PI) * (-3.0f / 6.0f * Mathf.Pow(2.0f - q, 2.0f));
        }
        return 0.0f;
    }

    public static float w_ij(Vector3 pos1, Vector3 pos2, float h)
    {
        float r = Vector3.Distance(pos1, pos2);
        float q = r / h;
        return 1.0f / (h * h * h) * kernel(q);
    }

    public static Vector3 dw_ij(Vector3 pos1, Vector3 pos2, float h) {
        Vector3 d = pos1 - pos2;
        float r = Vector3.Distance(pos1, pos2);
        float q = r / h;
        if (r < 1e-10)
        {
            return new Vector3(0, 0, 0);
        }
        return d * (1.0f / (h * h * h * h) * dkernel(q) / r);
    }


    public static int Hash(Vector3 cell)
    {
        int ret = ((int)cell.x * 73856093 ^ (int)cell.y * 19349663 ^ (int)cell.z * 83492791) % Fluid.M;
        ret += Fluid.M;
        return ret % Fluid.M;
    }

    public static Vector3 getCell(Vector3 pos, float H)
    {
        return new Vector3((int)Mathf.Ceil(pos.x / H * 2),
            (int)Mathf.Ceil(pos.y / H * 2),
            (int)Mathf.Ceil(pos.z / H * 2));
    }
}
