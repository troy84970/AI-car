using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Runtime.Remoting.Messaging;

public class MLP
{
    TextAsset textAsset;
    List<Vector<double>> inputs;
    List<double> ds;
    int epoch = 10;
    int inputCount = 3;
    int outputCount = 1;
    int hiddenLayerNeuronCount = 10;
    int hiddenLayerCount = 4;
    Matrix<double> inputLayer;
    Matrix<double> outputLayer;
    List<Matrix<double>> hiddenLayer;
    List<Matrix<double>> Layeroutputs;//每層的輸出 [0]h1 output
    List<Matrix<double>> v_List;//vj
    List<Matrix<double>> biases; //colum matrix //h1 biases[0]對應hiddenLayer[0]
    double outPutBias;
    double learningRate = 0.55;
    public MLP()
    {
        UnityEngine.Random.InitState(41);
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
        GetData();
        //random w and bias
        RandomizeBias();
        RandomizeWeights();
        for (int i = 0; i < epoch; i++)
        {
            for (int n = 0; n < inputs.Count; n++)
            {
                double y = FeedForward(inputs[n]);
                BackPropogation(y, ds[n]);
            }
        }
    }
    public double FeedForward(Vector<double> input)
    {
        //input to hidden
        if (input[0] > 20) input[0] = 5;
        else input[0] /= 15;
        if (input[1] > 20) input[1] = 5;
        else input[1] /= 15;
        if (input[1] > 20) input[1] = 5;
        else input[1] /= 15;
        Matrix<double> inputMatrix = input.ToColumnMatrix();
        inputLayer = inputMatrix;
        v_List[0] = hiddenLayer[0] * inputMatrix - biases[0];
        Layeroutputs[0] = TanH(v_List[0]);
        // hidden to hidden
        for (int j = 1; j < hiddenLayerCount; j++)
        {
            v_List[j] = hiddenLayer[j] * Layeroutputs[j - 1] - biases[j];
            Layeroutputs[j] = TanH(v_List[j]);
        }
        // hidden to output
        v_List[hiddenLayerCount] = outputLayer * Layeroutputs[hiddenLayerCount - 1];
        v_List[hiddenLayerCount][0, 0] -= outPutBias;
        Layeroutputs[hiddenLayerCount] = TanH(v_List[hiddenLayerCount]);
        return Layeroutputs[hiddenLayerCount][0, 0];
    }
    private void RandomizeBias()
    {
        for (int i = 0; i < biases.Count; i++)
        {
            for (int j = 0; j < biases[i].RowCount; j++)
            {
                biases[i][j, 0] = UnityEngine.Random.Range(-1f, 1f);
            }
        }
        outPutBias = UnityEngine.Random.Range(-1f, 1f);
    }
    private void RandomizeWeights()
    {
        for (int i = 0; i < hiddenLayer.Count; i++)
        {
            for (int j = 0; j < hiddenLayer[i].RowCount; j++)
            {
                for (int k = 0; k < hiddenLayer[i].ColumnCount; k++)
                {
                    hiddenLayer[i][j, k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                    if (i == 0) { hiddenLayer[i][j, 0] = 0.75f; hiddenLayer[i][j, 1] = -0.7f; hiddenLayer[i][j, 2] = 0.7f; }
                }
            }
        }
        outputLayer = Matrix<double>.Build.Dense(outputCount, hiddenLayerNeuronCount);
        for (int i = 0; i < outputLayer.ColumnCount; i++)
            outputLayer[0, i] = UnityEngine.Random.Range(-0.5f, 0.5f);
    }
    public double Predict(Vector<double> input)
    {
        double r = FeedForward(input);
        r *= 40;
        return r;
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
            ds.Add(double.Parse(data[3]));
            inputs.Add(Vector<double>.Build.DenseOfArray(tmp));
        }
    }
    private void BackPropogation(double y, double d)
    {
        //last error = Act'(vj)*(d-yj)
        d /= 40;
        Matrix<double> lastError = TanH_Derivative(v_List[hiddenLayerCount]) * (d - y);
        //modify weight of output layer
        // deltaW = learning rate * error * input (yi)
        Matrix<double> deltaW = learningRate * lastError * Layeroutputs[hiddenLayerCount - 1].Transpose();
        Matrix<double> backError = (outputLayer.Transpose() * lastError).PointwiseMultiply(TanH_Derivative(v_List[hiddenLayerCount - 1]));//上一層的error
        outputLayer += deltaW;
        outPutBias += learningRate * lastError[0, 0] * (-1);
        lastError = backError.Clone();
        // hidden layer
        for (int j = hiddenLayer.Count - 1; j > 0; j--)
        {
            deltaW = learningRate * lastError * Layeroutputs[j - 1].Transpose();
            backError = (hiddenLayer[j].Transpose() * lastError).PointwiseMultiply(TanH_Derivative(v_List[j - 1]));
            hiddenLayer[j] += deltaW;
            biases[j] += learningRate * lastError * (-1);
            lastError = backError.Clone();
        }
        //h0
        deltaW = learningRate * lastError * inputLayer.Transpose();
        hiddenLayer[0] += deltaW;
        biases[0] += learningRate * lastError * (-1);
    }
    private double LReLU(double val)
    {
        return val >= 0 ? val : 0.9 * val;
    }
    private Matrix<double> LReLU(Matrix<double> matrix)
    {
        Matrix<double> m = matrix.Clone();
        for (int i = 0; i < m.RowCount; i++)
        {
            m[i, 0] = LReLU(m[i, 0]);
        }
        return m;
    }
    private double LReLU_Derivative(double val)
    {
        return (val >= 0) ? 1 : 0.9;
    }
    private Matrix<double> LReLU_Derivative(Matrix<double> matrix)
    {
        Matrix<double> m = matrix.Clone();
        for (int i = 0; i < m.RowCount; i++)
        {
            m[i, 0] = LReLU_Derivative(m[i, 0]);
        }
        return m;
    }
    private double TanH(double val)
    {
        return Math.Tanh(val);
    }
    private Matrix<double> TanH(Matrix<double> matrix)
    {
        Matrix<double> m = matrix.Clone();
        for (int i = 0; i < m.RowCount; i++)
        {
            m[i, 0] = TanH(m[i, 0]);
        }
        return m;
    }
    private double TanH_Derivative(double val)
    {
        return 1 - TanH(val) * TanH(val);
    }
    private Matrix<double> TanH_Derivative(Matrix<double> matrix)
    {
        Matrix<double> m = matrix.Clone();
        for (int i = 0; i < m.RowCount; i++)
        {
            m[i, 0] = TanH_Derivative(m[i, 0]);
        }
        return m;
    }

}