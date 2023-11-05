using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Vector<double> v = Vector<double>.Build.Dense(3);
        v[0] = 22.0;
        v[1] = 8.0;
        v[2] = 8.0;
        RBFN rBFN = new RBFN(3);
        for (int i = 0; i < 40; i++)
        {
            rBFN.Train();
        }
        Debug.Log("!!!!!!");
        Debug.Log(rBFN.Predict(v));
        v[0] = 9.0;
        v[1] = 9.0;
        v[2] = 27.0;
        Debug.Log(rBFN.Predict(v));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
