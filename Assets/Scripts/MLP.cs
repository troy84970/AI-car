using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System;
using Accord.Math;
using System.Linq;

public class MLP
{
    TextAsset textAsset;
    List<Vector<double>> inputs;
    List<double> ds;
    int epoch = 150;
    int inputCount = 3;
    int outputCount = 1;
    int hiddenLayerNeuronCount = 6;
    int hiddenLayerCount = 3;
    Matrix<double> inputLayer;
    Matrix<double> outputLayer;
    List<Matrix<double>> hiddenLayer;
    List<Matrix<double>> Layeroutputs;//每層的輸出
    List<Matrix<double>> v_List;//vj
    List<Matrix<double>> biases; //biases[0]對應hiddenLayer[0]
    double outPutBias;
    double learningRate = 0.6;
    double dmax = 40, dmin = -40;
    List<double> tmaxs;
    List<double> tmins;

    public MLP(int inCount)
    {
        inputCount = inCount;
        if (inCount == 5)
        {
            epoch = 200;
            hiddenLayerCount = 3;
            hiddenLayerNeuronCount = 7;
        }
        tmaxs = new List<double>();
        tmins = new List<double>();
        UnityEngine.Random.InitState(30);
        //initialize
        biases = new List<Matrix<double>>();
        hiddenLayer = new List<Matrix<double>>();
        v_List = new List<Matrix<double>>(new Matrix<double>[hiddenLayerCount + 1]);
        for (int i = 0; i < hiddenLayerCount; i++)
        {
            if (i == 0)
            {
                biases.Add(Matrix<double>.Build.Dense(hiddenLayerNeuronCount, 1));
                hiddenLayer.Add(Matrix<double>.Build.Dense(hiddenLayerNeuronCount, inputCount));
            }
            else
            {
                biases.Add(Matrix<double>.Build.Dense(hiddenLayerNeuronCount, 1));
                hiddenLayer.Add(Matrix<double>.Build.Dense(hiddenLayerNeuronCount, hiddenLayerNeuronCount));
            }
        }
        Layeroutputs = new List<Matrix<double>>(new Matrix<double>[hiddenLayerCount + 1]);//outputlayer +1
        //Get Data
        if (inputCount == 3)
            GetData();
        else GetData5d();
        //random w and bias
        RandomizeBias();
        RandomizeWeights();
        for (int i = 0; i < epoch; i++)
        {
            for (int n = 0; n < inputs.Count; n++)
            {
                double y = FeedForward(inputs[n]);
                BackPropogation(y, NormalizeY(ds[n]));
            }
            learningRate *= 0.98;
        }
    }
    public double FeedForward(Vector<double> input)
    {
        input = NormalizeData(input);
        //input to hidden
        Matrix<double> inputMatrix = input.ToColumnMatrix();
        inputLayer = inputMatrix;
        v_List[0] = hiddenLayer[0] * inputMatrix - biases[0];
        Layeroutputs[0] = Sigmoid(v_List[0]);
        // hidden to hidden
        for (int j = 1; j < hiddenLayerCount; j++)
        {
            v_List[j] = hiddenLayer[j] * Layeroutputs[j - 1] - biases[j];
            Layeroutputs[j] = Sigmoid(v_List[j]);
        }
        // hidden to output
        v_List[hiddenLayerCount] = outputLayer * Layeroutputs[hiddenLayerCount - 1];
        v_List[hiddenLayerCount][0, 0] -= outPutBias;
        Layeroutputs[hiddenLayerCount] = Sigmoid(v_List[hiddenLayerCount]);
        return Layeroutputs[hiddenLayerCount][0, 0];
    }
    private void RandomizeBias()
    {
        for (int i = 0; i < biases.Count; i++)
        {
            for (int j = 0; j < biases[i].RowCount; j++)
            {
                biases[i][j, 0] = UnityEngine.Random.Range(-2f, 2f);//-1,1
            }
        }
        outPutBias = -0.8;//-0.5
    }
    private void RandomizeWeights()
    {
        for (int i = 0; i < hiddenLayer.Count; i++)
        {
            for (int j = 0; j < hiddenLayer[i].RowCount; j++)
            {
                for (int k = 0; k < hiddenLayer[i].ColumnCount; k++)
                {
                    hiddenLayer[i][j, k] = UnityEngine.Random.Range(-0.6f, 0.8f);
                }
            }
        }
        outputLayer = Matrix<double>.Build.Dense(outputCount, hiddenLayerNeuronCount);
        for (int i = 0; i < outputLayer.ColumnCount; i++)
            outputLayer[0, i] = UnityEngine.Random.Range(-0.6f, 0.8f);
    }
    public double Predict(Vector<double> input)
    {
        double r = FeedForward(input);
        r = (dmax - dmin) * r + dmin;
        if (Math.Abs(r) < 0.8) r = 0;
        return r;
    }
    private double NormalizeData(double d, int i)
    {
        return (d - tmins[i]) / (tmaxs[i] - tmins[i]);
    }
    private Vector<double> NormalizeData(Vector<double> v)
    {
        Vector<double> v2 = v.Clone();
        for (int i = 0; i < v2.Count; i++)
            v2[i] = NormalizeData(v2[i], i);
        return v2;
    }
    private double NormalizeY(double val)
    {
        return (val - dmin) / (dmax - dmin);
    }
    private void GetData()
    {
        inputs = new List<Vector<double>>();
        ds = new List<double>();
        textAsset = Resources.Load("train4dAll") as TextAsset;
        string[] trainingDatas = textAsset.text.Split("\n");
        for (int i = 0; i < trainingDatas.Length - 2; i++)
        {
            string[] data = trainingDatas[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var tmp = new[] { double.Parse(data[0]), double.Parse(data[1]), double.Parse(data[2]) };
            inputs.Add(Vector<double>.Build.DenseOfArray(tmp));
            ds.Add(double.Parse(data[3]));
        }
        for (int i = 0; i < 3; i++)
        {
            var dimensionValues = inputs.Select(v => v[i]);
            double max = dimensionValues.Max();
            double min = dimensionValues.Min();
            tmaxs.Add(max);
            tmins.Add(min);
        }

    }
    private void GetData5d()
    {
        inputs = new List<Vector<double>>();
        ds = new List<double>();
        textAsset = Resources.Load("train6dAll") as TextAsset;
        string[] trainingDatas = textAsset.text.Split("\n");
        for (int i = 0; i < trainingDatas.Length - 2; i++)
        {
            string[] data = trainingDatas[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var tmp = new[] { double.Parse(data[0]), double.Parse(data[1]), double.Parse(data[2]), double.Parse(data[3]), double.Parse(data[4]) };
            inputs.Add(Vector<double>.Build.DenseOfArray(tmp));
            ds.Add(double.Parse(data[5]));
        }
        for (int i = 0; i < 5; i++)
        {
            var dimensionValues = inputs.Select(v => v[i]);
            double max = dimensionValues.Max();
            double min = dimensionValues.Min();
            tmaxs.Add(max);
            tmins.Add(min);
        }
    }
    private void BackPropogation(double y, double d)
    {
        //last error = Act'(vj)*(d-yj)
        Matrix<double> lastError = Sigmoid_Derivative(v_List[hiddenLayerCount]) * (d - y);
        //modify weight of output layer
        // deltaW = learning rate * error * input (yi)
        Matrix<double> deltaW = learningRate * lastError * Layeroutputs[hiddenLayerCount - 1].Transpose();
        Matrix<double> backError = (outputLayer.Transpose() * lastError).PointwiseMultiply(Sigmoid_Derivative(v_List[hiddenLayerCount - 1]));//上一層的error
        outputLayer += deltaW;
        outPutBias += learningRate * lastError[0, 0] * (-1);
        lastError = backError.Clone();
        // hidden layer
        for (int j = hiddenLayer.Count - 1; j > 0; j--)
        {
            deltaW = learningRate * lastError * Layeroutputs[j - 1].Transpose();
            backError = (hiddenLayer[j].Transpose() * lastError).PointwiseMultiply(Sigmoid_Derivative(v_List[j - 1]));
            hiddenLayer[j] += deltaW;
            biases[j] += learningRate * lastError * (-1);
            lastError = backError.Clone();
        }
        //h0
        deltaW = learningRate * lastError * inputLayer.Transpose();
        hiddenLayer[0] += deltaW;
        biases[0] += learningRate * lastError * (-1);
    }
    private double Sigmoid(double val)
    {
        return 1 / (1 + Math.Exp(-val));
    }
    private Matrix<double> Sigmoid(Matrix<double> matrix)
    {
        Matrix<double> m = matrix.Clone();
        for (int i = 0; i < m.RowCount; i++)
        {
            m[i, 0] = Sigmoid(m[i, 0]);
        }
        return m;
    }
    private double Sigmoid_Derivative(double val)
    {
        var s = Sigmoid(val);
        return s * (1 - s);
    }
    private Matrix<double> Sigmoid_Derivative(Matrix<double> matrix)
    {
        Matrix<double> m = matrix.Clone();
        for (int i = 0; i < m.RowCount; i++)
        {
            m[i, 0] = Sigmoid_Derivative(m[i, 0]);
        }
        return m;
    }

}