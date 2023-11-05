using System;
using System.Collections.Generic;
using System.Linq;
using Accord.MachineLearning;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
public class RBFN
{
    private List<Vector<double>> centers;
    private Vector<double> weights;
    private Vector<double> rbfs;
    private List<Vector<double>> datas;
    private List<Vector<double>> inputs;
    private List<double> ys;
    private double sigma = 0.3;//need k 
    private double bias = 100;
    private double learningRate = 0.2;
    private const int k = 20;
    TextAsset textAsset;
    public RBFN(int d)
    {
        //Get Data
        datas = new List<Vector<double>>();
        inputs = new List<Vector<double>>();
        ys = new List<double>();
        textAsset = Resources.Load("train4dAll") as TextAsset;
        string[] trainingDatas = textAsset.text.Split("\n");
        for (int i = 0; i < trainingDatas.Length - 1; i++)
        {
            string[] data = trainingDatas[i].Split(" ");
            var tmp = new[] { double.Parse(data[0]), double.Parse(data[1]), double.Parse(data[2]) };
            var tmp2 = new[] { double.Parse(data[0]), double.Parse(data[1]), double.Parse(data[2]), double.Parse(data[3]) };
            ys.Add(double.Parse(data[3]));
            inputs.Add(Vector<double>.Build.DenseOfArray(tmp));
            datas.Add(Vector<double>.Build.DenseOfArray(tmp2));
        }
        weights = Vector<double>.Build.Dense(k);
        for (int i = 0; i < weights.Count; i++) weights[i] = UnityEngine.Random.Range(-1.0f, 1.0f);
        rbfs = Vector<double>.Build.Dense(k);
        centers = new List<Vector<double>>();
        Kmeans();//use K-means to initialize centers
    }

    public void Train()
    {
        for (int n = 0; n < inputs.Count; n++)
        {
            //calculate error:yn-Fn
            double error = ys[n] - Output(inputs[n]);
            //update bias and weight
            for (int i = 0; i < weights.Count; i++)
            {
                weights[i] += learningRate * rbfs[i] * error;
            }
            bias += learningRate * error;
        }
    }
    private double Output(Vector<double> input)
    {
        //caculate rbf 
        for (int i = 0; i < rbfs.Count; i++)
        {
            double distance = Distance.Euclidean<double>(input, centers[i]);
            double rbf = Math.Exp(-distance / (2 * sigma * sigma));
            rbfs[i] = rbf;
        }
        //caculate F(n)
        double F = weights.ToRowMatrix().Multiply(rbfs)[0] + bias;
        return F;
    }
    public double Predict(Vector<double> input)
    {
        return Output(input);
    }
    private void Kmeans()
    {
        KMeans kmeans = new KMeans(k: k);
        double[][] accordData = inputs.Select(vector => vector.ToArray()).ToArray();//inputs
        KMeansClusterCollection clusters = kmeans.Learn(accordData);
        int[] clusterAssignments = clusters.Decide(accordData);
        double[][] cs = clusters.Centroids;
        for (int i = 0; i < cs.Length; i++)
        {
            double[] center = cs[i];
            Vector<double> v = Vector<double>.Build.DenseOfArray(center);
            centers.Add(v);
        }
    }
}
