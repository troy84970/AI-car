using System.IO;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public class DataWriter : MonoBehaviour
{
    public void Write4d(Vector<double> v, double d)
    {
        string data;
        string filePath = Path.Combine(Application.dataPath, "track4D.txt");
        for (int i = 0; i < 3; i++)
        {
            data = v[i].ToString() + " ";
            File.AppendAllText(filePath, data);
        }
        data = d.ToString() + "\n";
        File.AppendAllText(filePath, data);
    }
    public void Write6d(Vector<double> v, double d)
    {
        string data;
        string filePath = Path.Combine(Application.dataPath, "track6D.txt");
        for (int i = 0; i < 5; i++)
        {
            data = v[i].ToString() + " ";
            File.AppendAllText(filePath, data);
        }
        data = d.ToString() + "\n";
        File.AppendAllText(filePath, data);
    }
}
