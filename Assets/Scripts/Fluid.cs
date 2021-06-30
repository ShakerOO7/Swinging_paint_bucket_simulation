using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static sph_math;

public class Fluid : MonoBehaviour
{
    [Range(10, 50000)]
    public int ParNum = 10000;
    [HideInInspector]
    public float ParSpace = 0.1f;
    public float ParScale = 0.1f;
    [HideInInspector]
    public float H = 1.0f; 
    public float Stiffness = 7;
    public GameObject ParPrefab;
    public Pendulum[] pendulum;
    public Transform[] Source;
    public float SourceRaduis=0.2f;
    [Range(0.1f, 2.0f)]
    public float ParMass = 0.18f;
    public float RestDensity = 1000;
    public float kinematicViscosity = 0.000001f;
    public Vector3 Gravity = new Vector3(0, 9.81f, 0);
    public int Batch = 12;
    public float[] YBoundries;

    Queue<GameObject> pool;
    HashSet<int> active;
    List<GameObject> fluid;
    List<Particle> fluid_par;
    List<int>[] grid;
    [HideInInspector]
    public const int M = 10000000;

    // Start is called before the first frame update
    void Start()
    {
        ParNum = SimVars.Fluid.ParNum;
        Stiffness = SimVars.Fluid.Stiffness;
        SourceRaduis = SimVars.Fluid.SourceRaduis;
        ParMass = SimVars.Fluid.ParMass;
        RestDensity = SimVars.Fluid.RestDensity;
        kinematicViscosity = SimVars.Fluid.kinematicViscosity;
        Gravity = SimVars.Fluid.Gravity;

        fluid = new List<GameObject>(ParNum);;
        fluid_par = new List<Particle>(ParNum);
        grid = new List<int>[M];
        pool = new Queue<GameObject>();
        ParPrefab.transform.localScale = new Vector3(ParScale, ParScale, ParScale);

        for (int i = 0; i < M; i++)
        {
            grid[i] = new List<int>();
        }

        active = new HashSet<int>();

        initFluid();
    }

    void initFluid()
    {
        int c = 0;
        while(c < this.ParNum)
        {
            c++;
            pool.Enqueue(Instantiate(ParPrefab));
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (int i in active)
        {
            fluid_par[i].neighboors = getNeighboors(i);
            if(fluid_par[i].neighboors.Count == 0)
            {
                Debug.LogWarning("Error zero neighboors");
                //Debug.Break();
            }
        }

        foreach (int i in active)
        {
            if (fluid_par[i].neighboors.Count == 0)
                continue;
            fluid_par[i].density = getDensity(i);
            fluid_par[i].pressure = getPressure(i);
        }
        foreach (int i in active)
        {
            if (fluid_par[i].neighboors.Count == 0)
                continue;
            Vector3 Fp = -(ParMass / fluid_par[i].density) * pressureST(i);
            Vector3 Fv = ParMass * kinematicViscosity * velocityND(i);
            Vector3 Fg = -ParMass * Gravity;
            if (fluid_par[i].position.y == YBoundries[0])
                Fg = Vector3.zero;
            Vector3 Ftotal = Fp + Fv + Fg;

            float dt = Time.deltaTime;
            fluid_par[i].velocity += dt * Ftotal / ParMass;

            Vector3 pos = fluid_par[i].position + dt * fluid_par[i].velocity;

            if (pos.y < YBoundries[0])
            {
                pos.y = YBoundries[0];
                fluid_par[i].timeout += dt;
            }

            updateGrid(i, pos);
            fluid_par[i].position = pos;
            fluid[i].transform.position = fluid_par[i].position;
        }
        List<int> t = new List<int>();
        foreach (int i in active)
        {
            if (fluid_par[i].timeout > 0.3f)
            {
                t.Add(i);
            }
        }
        foreach (int i in t)
            active.Remove(i);
    }

    private void FixedUpdate()
    {
        for(int i=0;i<Source.Length;i++)
            generate(Source[i].position, pendulum[i]);
    }

    void generate(Vector3 Source, Pendulum pendulum)
    {
        if(fluid_par.Count < ParNum)
        {
            int ParNum = Mathf.Min(this.ParNum, fluid_par.Count + Batch);
            int c = fluid_par.Count;
            for (float y = Source.y; c < ParNum; y+=ParSpace)
            {
                for (float x = Source.x - SourceRaduis; x <= Source.x + SourceRaduis && c < ParNum; x += ParSpace)
                {
                    for (float z = Source.z - SourceRaduis; z <= Source.z + SourceRaduis && c < ParNum; z += ParSpace)
                    {
                        try
                        {

                        Vector3 curPos = new Vector3(x, y, z);
                        GameObject curPar = pool.Dequeue();
                        curPar.transform.position = curPos;
                        curPar.SetActive(true);
                        fluid.Add(curPar);
                        Particle par = new Particle(curPos);
                        par.velocity = new Vector3(0.0f, -Mathf.Sqrt(Mathf.Abs(Gravity.y) * 2 * getHeight()), 0.0f) +
                                    pendulum.getLinearVelocity();
                        fluid_par.Add(par);
                        updateGrid(c, fluid_par[c].position);
                        active.Add(c);
                        c++;
                        }catch(Exception e)
                        {
                            Debug.LogError(e.Message);
                            Debug.Break();
                        }
                    }
                }
            }
        }
    }

    private float getHeight()
    {
        int c = (ParNum - fluid_par.Count);
        float lc = (Mathf.PI * SourceRaduis * SourceRaduis) / (ParSpace * ParSpace);
        //return (c / lc) / (ParSpace * ParSpace * ParSpace);
        return 2.0f;
    }

    void _print(params string[] a)
    {
        foreach (var i in a)
        {
            Debug.Log(i);
        }
        Debug.Break();
    }

    void updateGrid(int i, Vector3 newPos)
    {
        Vector3 oldCell = getCell(fluid_par[i].position, H);
        Vector3 newCell = getCell(newPos, H);
        if (grid[Hash(oldCell)].Contains(i))
            grid[Hash(oldCell)].Remove(i);
        if (!grid[Hash(newCell)].Contains(i))
            grid[Hash(newCell)].Add(i);
    }

    private List<int> getNeighboors(int i)
    {
        bool b = false;
        List<int> ret = new List<int>();
        if(b)
        { 
            for(int j=0;j<fluid_par.Count;j++)
            {
                Vector3 curPos = fluid_par[j].position;
                if (Vector3.Distance(curPos, fluid_par[i].position) < H * H * H
                    && Vector3.Distance(curPos, fluid_par[i].position) > 0.000001f)
                {
                    ret.Add(j);
                }
            }
            return ret;
        }
        Vector3 cell = getCell(fluid_par[i].position, H);
        for (float j = cell.x - 1; j <= cell.x + 1; j++)
        {
            for (float u = cell.y - 1; u <= cell.y + 1; u++)
            {
                for (float v = cell.z - 1; v <= cell.z + 1; v++)
                {
                    Vector3 curCell = new Vector3(j, u, v);
                    foreach (int e in grid[Hash(curCell)])
                    {
                        Vector3 curPos = fluid_par[e].position;
                        if (Vector3.Distance(curPos, fluid_par[i].position) < H * H * H
                            && Vector3.Distance(curPos, fluid_par[i].position) > 0.000001f)
                        {
                            ret.Add(e);
                        }
                    }
                }
            }
        }
        if(ret.Count>33)
        {
            //Debug.LogWarning("neighboors: " + ret.Count);
            List<int> r = new List<int>();
            System.Random rng = new System.Random();
            int c = 30;
            while(c-->0)
            {
                int k = rng.Next(ret.Count);
                r.Add(ret[k]);
            }
            return r;
        }
        return ret;
    }

    Vector3 pressureST(int i)
    {
        Vector3 ret = Vector3.zero;
        foreach (int j in fluid_par[i].neighboors)
        {
            ret += ((fluid_par[i].pressure / Mathf.Pow(fluid_par[i].density, 2)) +
                (fluid_par[j].pressure / Mathf.Pow(fluid_par[j].density, 2))) *
                dw_ij(fluid_par[i].position, fluid_par[j].position, H);
        }
        return fluid_par[i].density * ParMass * ret;
    }

    Vector3 velocityND(int i)
    {
        Vector3 ret = Vector3.zero;
        foreach (int j in fluid_par[i].neighboors)
        {
            Vector3 d = fluid_par[i].position - fluid_par[j].position;
            Vector3 dv = fluid_par[i].velocity - fluid_par[j].velocity;
            Vector3 dwij = dw_ij(fluid_par[i].position, fluid_par[j].position, H);
            ret += (1.0f / fluid_par[j].density) * dv
                * (Vector3.Dot(d, dwij) / (Vector3.Dot(d, d) + 0.01f * Mathf.Pow(H, 2)));
        }
        return 2 * ParMass * ret;
    }

    float getPressure(int i)
    {
        return Stiffness * (Mathf.Pow(fluid_par[i].density / RestDensity, 7) - 1);
    }

    float getDensity(int i)
    {
        float ret = 10.0f;
        foreach (int j in fluid_par[i].neighboors)
        {
            ret += w_ij(fluid_par[i].position, fluid_par[j].position, H);
        }
        return ret * ParMass;
    }
}
