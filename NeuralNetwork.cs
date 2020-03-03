using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;
using System.Collections;
using System.Collections.Generic;

public class NeuralNetwork
{
    Vector<double> input, hl1, output;
    Matrix<double> il1, l1o, l1l2;
    int liSize;
    int lhSize;
    int loSize;
    float adj;

    public NeuralNetwork(List<double> weights, float adj, int liSize, int lhSize, int loSize)
    {
        // Init size of layers. Compensate for the bias in the input vector
        this.liSize = liSize + 1;
        this.lhSize = lhSize;
        this.loSize = loSize;
        this.adj = adj;

        if (weights == null)
        {
            // Generate empty weight matrices
            il1 = Matrix<double>.Build.Random(liSize, lhSize);
            l1o = Matrix<double>.Build.Random(lhSize, loSize);

        }
        else
        {
            // load existing network
            List<double> w1 = weights.GetRange(0, liSize * lhSize);
            il1 = Matrix<double>.Build.DenseOfArray(parseMatrix(w1, liSize, lhSize));

            List<double> w2 = weights.GetRange(liSize * lhSize, loSize * lhSize);
            l1o = Matrix<double>.Build.DenseOfArray(parseMatrix(w2, lhSize, loSize));
        }
    }

    public string toString()
    {
        string ret = "";

        //for (int i = 0; i < 6; i++)
        //    for (int j = 0; j < 8; j++)
        //        ret += _il1["":

        Chromosome c = getChromosome();
        List<double> l = c.getList();

        for (int i = 0; i < l.Count; i++)
            ret += l[i] + " ";

        return ret;
    }

    public double[,] parseMatrix(List<double> weights, int rows, int cols)
    {
        double[,] _m = new double[rows, cols];

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                _m[i, j] = weights[i * cols + j];

        return _m;
    }

    // Apply segmoid on a float
    double applySegmoidf(double f)
    {
        double e = MathNet.Numerics.Constants.E;
        double _q = Math.Pow(e, -(0.08) * f);
        double _p = 1f / (1f + _q);
        return (double)_p;
    }

    // Apply segmoid on vector
    Vector<double> applySegmoid(Vector<double> v)
    {
        double[] ret = new double[v.Count];

        for (int i = 0; i < v.Count; i++)
            ret[i] = applySegmoidf(v[i]);

        return Vector<double>.Build.DenseOfArray(ret);
    }

    private void _feedForward()
    {
        // Calculate hidden layer1 [Input -> hidden layer]
        hl1 = applySegmoid(il1.LeftMultiply(input));

        // Calculate output layer  [hidden layer -> output]
        output = applySegmoid(l1o.LeftMultiply(hl1));

        // Normalize output to also have negative
        // values and scale them 
        output -= .5f;
        output *= adj;
    }

    public double[] feedForward(double[] v)
    {
        // Set input vector with an additional node set to 
        // const 1 as a bias
        input = Vector<double>.Build.DenseOfArray(v);
        input.Add(1);

        // calculate output vector
        _feedForward();
        
        return output.ToArray();
    }

    // Returns the network encoded into a gene 
    public Chromosome getChromosome()
    {
        List<double> _c = new List<double>();

        _c.AddRange(vectorizeMatrix(il1));
        _c.AddRange(vectorizeMatrix(l1o));

        return new Chromosome(_c);
    }

    // Returns a vectorized form of a given matrix
    public List<double> vectorizeMatrix(Matrix<double> mat)
    {
        List<double> vec = new List<double>();

        for (int i = 0; i < mat.RowCount; i++)
        {
            Vector<double> _v = mat.Row(i);

            for (int j = 0; j < mat.ColumnCount; j++)
                vec.Add(_v[j]);
        }
        return vec;
    }
}