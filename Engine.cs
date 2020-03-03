using System;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    public int populationSize;
    public Transform firstCar;
    public float timeScale;
    public float crossProb, mutationProb, elitism;

    List<Transform> cars;
    public static int finishedCars;
    GeneticAlgorithm ga;
    int generation;
    Vector3 initPosition;
    Quaternion initRotation;
    GameObject best;
    int bestCounter;
    public float shakyCam = .5f;
    float totalFitness = 0;
    public bool useSmartCar = true;

    public void notify(float fitness)
    {
        finishedCars++;
        
        if(totalFitness < fitness)
            totalFitness = fitness;
    }

    public void checkBest(Transform t, int l)
    {
        //if(bestCounter < l)
        //{
        //    bestCounter = l;
        //}
    }

    void Start()
    {
        bestCounter = 0;
        generation = 0;
        if (populationSize == 0 || firstCar == null)
        {
            Debug.LogError("Public parameters missing");
            UnityEditor.EditorApplication.isPlaying = false;
        }

        initPosition = firstCar.transform.position;
        initRotation = firstCar.transform.rotation;

        /*if (useSmartCar)
            populationSize = 5;*/

        restert(null);
    }

    // restarts a new generation
    void restert(List<List<double>> ws)
    {
        // destroy all car transforms
        if (cars != null)
            foreach (Transform t in cars)
                Destroy(t.gameObject);

        // create new car transforms
        cars = new List<Transform>();

        GenerateNewGeneration(ws);
    }

    void GenerateNewGeneration(List<List<double>> weights)
    {
        for (int i = 0; i < populationSize; i++)
        {
            List<double> w = null;

            if (useSmartCar && i < populationSize / 4)
                w = smartCar();
            else if (weights != null)
                w = weights[i];

            // use firstCar as a template to generate an
            // instance of a car
            firstCar.gameObject.SetActive(true);
            Transform t = Instantiate(firstCar, initPosition, initRotation);
            firstCar.gameObject.SetActive(false);

            // setup car with new chromosome
            CarScript cs = t.GetComponent<CarScript>();
            cs.enabled = true;
            cs.restart(w);
            cars.Add(t);
        }
    }

    public List<double> smartCar()
    {
        return new List<double>(
            new double[]
            {-2.48138506627414, -1.87832858891515, 1.41594332319697, -0.841116059342846, -0.266552266865722, -3.19878735062439, -0.896892583479892, -3.2487821425456, -0.737891993160721, -0.902778203246462, -2.02646733336543, -1.09355509413209, 1.57672277139919, 1.095754989585, -1.86303175584902, -1.46748351588036, 0.221756087577699, -3.28430390496427, 1.84978956408105, 1.33930627314768, -4.73015518586938, 0.83953892185942, 0.360861755320048, -1.03642427384505, 1.32182717840203, 1.1599855368652, 0.159310815133043, 1.19088094684531, -1.13915530971076, 0.688261589657658, 1.20829220156344, 0.851847024427135, -1.62821948343818, 1.07914415747792, -0.133345916591877, -4.07395977603361, -2.27904770631642, -1.14986191976393, 0.138224116271017, -1.87262797032073, -0.277143321094819, 0.534832384474625, 0.713352403858018, 0.788370358082408, 0.713757874973987, 1.94606447617672, -0.414968581564012, -0.453786456063934, 0.312426560547785, 0.30311659763625, 0.81264165292118, 1.56179694139638, 0.548721287360404, -2.56438140990626, -0.345940079717464, 0.924989953531945, 0.615267407911754, 0.717533270041951, -0.764982677562891, 3.93041572595241, 1.30070162818674, 1.1466134088829, -0.614557718049815, 0.0292581389043514}
            );
    }

    void FixedUpdate()
    {
        Time.timeScale = timeScale;

        // when a generation has died, calculate a new one
        if (finishedCars == populationSize)
        {
            finishedCars = 0;
            Debug.Log("Generation " + (++generation));

            List<Chromosome> chromosomes = new List<Chromosome>();

            // gather all chromosomes of population
            foreach(Transform t in cars)
               chromosomes.Add(t.GetComponent<CarScript>().getChromosome());

            // create a new instance of a genetic algorithm
            ga = new GeneticAlgorithm(populationSize, crossProb, mutationProb, elitism);
            
            // generate a new generation using a genetic algorithm
            List<Chromosome> nextG = ga.getNextGeneration(chromosomes, totalFitness);

            List<List<double>> fs = new List<List<double>>();

            foreach (Chromosome c in nextG)
                fs.Add(c.getList());

            // restart a new generation
            restert(fs);
        }else
        {
            // as long as there a still active cars remaining
            // find the first one and make the camera follow it
            foreach (Transform t in cars)
                if (t.gameObject.activeSelf)
                {
                    Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, t.position - t.forward * 6 + t.up * 2, shakyCam);
                    Camera.main.transform.LookAt(t);
                    break;
                }

        }
    }
}