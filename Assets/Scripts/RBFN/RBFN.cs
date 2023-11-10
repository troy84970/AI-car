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
    private Vector<double> sigmas;
    private double bias = 20;
    private double learningRate = 0.7;
    private const int k = 15;
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
            string[] data = trainingDatas[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var tmp = new[] { double.Parse(data[0]), double.Parse(data[1]), double.Parse(data[2]) };
            var tmp2 = new[] { double.Parse(data[0]), double.Parse(data[1]), double.Parse(data[2]), double.Parse(data[3]) };
            ys.Add(double.Parse(data[3]));
            inputs.Add(Vector<double>.Build.DenseOfArray(tmp));
            datas.Add(Vector<double>.Build.DenseOfArray(tmp2));
        }
        weights = Vector<double>.Build.Dense(k);
        for (int i = 0; i < weights.Count; i++) weights[i] = UnityEngine.Random.Range(100.0f, 200.0f);
        rbfs = Vector<double>.Build.Dense(k);
        centers = new List<Vector<double>>();
        Kmeans();//use K-means to initialize centers
        sigmas = Vector<double>.Build.Dense(centers.Count);
        for (int i = 0; i < centers.Count; i++)
        {
            sigmas[i] = 0.7;
        }
    }

    public void Train()
    {
        for (int n = 0; n < inputs.Count; n++)
        {
            //calculate error:yn-Fn
            double error = ys[n] - Output(inputs[n]);
            //Debug.Log(ys[n] + " " + Output(inputs[n]));
            //update bias and weight
            for (int i = 0; i < weights.Count; i++)
            {
                double sigmaTmp = sigmas[i];
                sigmas[i] += learningRate * error * weights[i] * rbfs[i] / sigmas[i] / sigmas[i] / sigmas[i] * Distance.Euclidean(inputs[n], centers[i]) * Distance.Euclidean(inputs[n], centers[i]);
                double scale = learningRate * error * weights[i] * rbfs[i] / sigmaTmp / sigmaTmp;
                centers[i] = centers[i] + (inputs[n] - centers[i]) * scale;
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
            double distance = Distance.Euclidean(input, centers[i]);
            double rbf = Math.Exp(-distance * distance / (2 * sigmas[i] * sigmas[i]));
            if (double.IsNaN(rbf)) rbf = 0.001;
            rbfs[i] = rbf;
        }
        //caculate F(n)
        double F = weights.ToRowMatrix().Multiply(rbfs)[0] + bias;
        //Debug.Log(F);
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
            //Debug.Log(center[0] + " " + center[1] + " " + center[2]);
            Vector<double> v = Vector<double>.Build.DenseOfArray(center);
            centers.Add(v);
        }
    }
}
