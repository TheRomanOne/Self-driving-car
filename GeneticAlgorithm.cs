using System.Collections;
using System.Collections.Generic;
using System;
using MathNet.Numerics;

public class Chromosome
{
    private List<double> chromosome;
    private double fitness;
    public int length;
    Random rnd;

    public Chromosome(List<double> l)
    {
        rnd = new Random();
        chromosome = l;
        length = l.Count;
    }

    public void setFitness(double f)
    {
        fitness = f;
    }

    public double getFitness()
    {
        return fitness;
    }

    public List<double> getList()
    {
        return chromosome;
    }

    public void mutate(double prob)
    {
        List<double> _nc = new List<double>();

        for (int i = 0; i < length; i++)
            _nc.Add(chromosome[i] + ((rnd.NextDouble() * fitness * 2 < prob) ? ((double)rnd.NextDouble() - .5f) : 0));

        chromosome = _nc;
    }
}

public class GeneticAlgorithm
{
    List<Chromosome> genePool;
    int populationSize;
    double crossoverProb, mutationProb, elitism;
    double avgFitness;

    Random rnd;

    public GeneticAlgorithm(int populationSize, double crossoverProb, double mutationProb, double elitism)
    {
        this.elitism = elitism;
        this.crossoverProb = crossoverProb;
        this.mutationProb = mutationProb;
        this.populationSize = populationSize;

        rnd = new Random();
    }

    Chromosome selectParentByTournoment()
    {
        Chromosome p1 = genePool[rnd.Next(populationSize / 2)];
        Chromosome p2 = genePool[rnd.Next(populationSize / 2)];

        return (p1.getFitness() > p2.getFitness()) ? p1 : p2;
    }


    List<Chromosome> crossover(Chromosome p1, Chromosome p2)
    {
        List<double> _c1 = new List<double>();
        List<double> _c2 = new List<double>();

        List<double> _p1 = p1.getList();
        List<double> _p2 = p2.getList();

        for (int i = 0; i < p1.length; i++)
            if(rnd.NextDouble() < crossoverProb)
            {
                _c1.Add(_p1[i]);
                _c2.Add(_p2[i]);
            }else
            {
                _c2.Add(_p1[i]);
                _c1.Add(_p2[i]);
            }

        double pairedFitness = (p1.getFitness() + p2.getFitness()) / 2;
        Chromosome c1 = new Chromosome(_c1);
        c1.setFitness(pairedFitness);
        Chromosome c2 = new Chromosome(_c2);
        c1.setFitness(pairedFitness);

        if (mutationProb > 0)
        {
            c1.mutate(mutationProb);
            c2.mutate(mutationProb);
        }

        return new List<Chromosome>(new Chromosome[]{ c1, c2 });
    }

    Chromosome mutate(Chromosome c)
    {
        List<double> _c = c.getList();
        List<double> _nc = new List<double>();

        for (int i = 0; i < c.length; i++)
            _nc.Add((rnd.NextDouble() < mutationProb) ? (double)rnd.NextDouble() : _c[i]);

        return new Chromosome(_nc);
    }

    public List<Chromosome> getNextGeneration(List<Chromosome> gp, double totalFitness)
    {
        avgFitness = totalFitness;// / gp.Count;

        foreach (Chromosome c in gp)
            c.setFitness(c.getFitness() / avgFitness);

        genePool = gp;
        gp.Sort((c1, c2) => c2.getFitness().CompareTo(c1.getFitness()));
        List<Chromosome> newGenePool = new List<Chromosome>();

        for(int el = 0; el < elitism * gp.Count; el++)
            newGenePool.Add(gp[el]);

        while (newGenePool.Count < populationSize)
        {
            List<Chromosome> children = crossover(
                selectParentByTournoment(),
                selectParentByTournoment()
                );

            newGenePool.AddRange(children);
        }

        return newGenePool;
    }
}
