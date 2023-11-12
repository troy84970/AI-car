using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Accord.MachineLearning;
using Accord.MachineLearning.Clustering;
using Accord.Math.Distances;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using UnityEngine;
public class RBFN
{
    private List<Vector<double>> centers;
    private Vector<double> weights;
    private Vector<double> rbfs;
    private List<Vector<double>> inputs;
    private List<double> ys;
    private Vector<double> cluster_stds;
    private double bias = 5;
    private double learningRate = 0.1;
    private int k = 20;
    TextAsset textAsset;
    int epoch = 10;
    public RBFN(int d)
    {
        UnityEngine.Random.InitState(42);
        //Get Data
        k = d;
        inputs = new List<Vector<double>>();
        ys = new List<double>();
        textAsset = Resources.Load("train4dAll") as TextAsset;
        string[] trainingDatas = textAsset.text.Split("\n");
        for (int i = 0; i < trainingDatas.Length - 1; i++)
        {
            string[] data = trainingDatas[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var tmp = new[] { double.Parse(data[0]), double.Parse(data[1]), double.Parse(data[2]) };
            ys.Add(double.Parse(data[3]));
            inputs.Add(Vector<double>.Build.DenseOfArray(tmp));
        }
        weights = Vector<double>.Build.Dense(k);
        for (int i = 0; i < weights.Count; i++)
        {
            float stdDev = (float)(1.0 / Math.Sqrt(k));
            weights[i] = UnityEngine.Random.Range(-stdDev, stdDev);
        }
        rbfs = Vector<double>.Build.Dense(k);
        centers = new List<Vector<double>>();
        cluster_stds = Vector<double>.Build.Dense(k);
        Kmeans();//use K-means to initialize centers and std Deviation
    }

    public void Train()
    {
        for (int e = 0; e < epoch; e++)
        {
            for (int n = 0; n < inputs.Count; n++)
            {
                //calculate error:yn-Fn
                double error = ys[n] - Output(inputs[n]);
                if (n == 36)
                    Debug.Log("n=" + n + "! " + ys[n] + " output: " + Output(inputs[n]));
                //update bias and weight
                for (int i = 0; i < weights.Count; i++)
                {
                    double stdTmp = cluster_stds[i];
                    cluster_stds[i] += learningRate * error * weights[i] * rbfs[i] * Math.Pow(stdTmp, -3) * Math.Pow(Distance.Euclidean(inputs[n], centers[i]), 2);
                    double scale = learningRate * error * weights[i] * rbfs[i] / Math.Pow(stdTmp, 2);
                    centers[i] += (inputs[n] - centers[i]) * scale;
                    weights[i] += learningRate * rbfs[i] * error;
                }
                bias += learningRate * error;
            }
        }
    }
    private double Output(Vector<double> input)
    {
        //caculate rbf 
        for (int i = 0; i < rbfs.Count; i++)
        {
            double distance = Distance.Euclidean(input, centers[i]);
            double rbf = Math.Exp(-distance * distance / (2 * cluster_stds[i] * cluster_stds[i]));
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
        var clusters = kmeans.Learn(accordData);
        double[][] cs = clusters.Centroids;
        for (int i = 0; i < cs.Length; i++)
        {
            double[] center = cs[i];
            Vector<double> v = Vector<double>.Build.DenseOfArray(center);
            centers.Add(v);
        }

        // 獲取每個群組的標準差
        List<double> distances = new List<double>();
        for (int i = 0; i < k; i++)
        {
            distances.Clear();
            int clusterIndex = i;
            //第i群的所有點
            double[][] clusterPoints = accordData.Where((point, index) => clusters.Decide(point) == clusterIndex).ToArray();
            for (int j = 0; j < clusterPoints.Length; j++)
            {
                // 計算每個點到中心的距離
                distances.Add(Distance.Euclidean(clusterPoints[j], cs[i]));
            }
            // 計算標準差
            cluster_stds[i] = Statistics.StandardDeviation(distances);
            distances.Clear();
        }
    }
}
