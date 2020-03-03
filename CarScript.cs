using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class CarScript : MonoBehaviour
{
    NeuralNetwork brain;
    
    public Transform c1, c2, c3, c4, c5, c12, c22, c32, c42, c52;
    Rigidbody rb;
    Vector<double> result;
    Engine engine;
    float distance, fitness;

    public bool running = true;
    public float speed;

    public float param1, param2;
    public WheelCollider w11, w21, w12, w22;
    private readonly int m_numberOfCities;
    private List<Vector3> path;
    int lastTime = 0;
    float itterationTime, totalTime;
    public bool checkLine = false;
    bool endOfTheLine = false;

    public void restart(List<double> weights)
    {
        itterationTime = Time.timeSinceLevelLoad;
        path = new List<Vector3>();
        rb = GetComponent<Rigidbody>();

        brain = new NeuralNetwork(
            weights: weights, // Pre loaded weights to restart with.
            adj: 1000, // Adjustment
            liSize: 11, // input size corresponding to 10 distance reasings and velocity value
            lhSize: 8, // chosen hidden layer size
            loSize: 2 // output size corresponding to thrust and rotation
            );

        // set car to running
        running = true;
        engine = Camera.main.GetComponent<Engine>();
    }

    void Update ()
    {
        // if car was terminated finish exit the update function to
        // reduce cpu usage
        if (!running)
            return;

        speed = GetComponent<Rigidbody>().velocity.magnitude;

        // if car has either:
        //    * not passed the checkline
        //    * has a velocity of less than 0.2
        // retminate the car
        if (Time.timeSinceLevelLoad - itterationTime > 10 && (!checkLine || speed < 0.2))
            terminate();

        // Generate rays
        Ray r1 = new Ray(c1.position, c1.up);
        Ray r2 = new Ray(c2.position, c2.up);
        Ray r3 = new Ray(c3.position, c3.up);
        Ray r4 = new Ray(c4.position, c4.up);
        Ray r5 = new Ray(c5.position, c5.up);

        Ray r12 = new Ray(c12.position, c12.up);
        Ray r22 = new Ray(c22.position, c22.up);
        Ray r32 = new Ray(c32.position, c32.up);
        Ray r42 = new Ray(c42.position, c42.up);
        Ray r52 = new Ray(c52.position, c52.up);

        RaycastHit rh1, rh2, rh3, rh4, rh5, rh12, rh22, rh32, rh42, rh52;

        int lm = ~(1 << 9);

        Physics.Raycast(r1, out rh1, Mathf.Infinity, lm);
        Physics.Raycast(r2, out rh2, Mathf.Infinity, lm);
        Physics.Raycast(r3, out rh3, Mathf.Infinity, lm);
        Physics.Raycast(r4, out rh4, Mathf.Infinity, lm);
        Physics.Raycast(r5, out rh5, Mathf.Infinity, lm);

        Physics.Raycast(r12, out rh12, Mathf.Infinity, lm);
        Physics.Raycast(r22, out rh22, Mathf.Infinity, lm);
        Physics.Raycast(r32, out rh32, Mathf.Infinity, lm);
        Physics.Raycast(r42, out rh42, Mathf.Infinity, lm);
        Physics.Raycast(r52, out rh52, Mathf.Infinity, lm);

        // Create an input vector which consists of 10 distance readings
        // and the cars currentvelocity
        double[] input = new double[]{
            rh1.distance, rh2.distance, rh3.distance, rh4.distance, rh5.distance,
            rh12.distance, rh22.distance, rh32.distance, rh42.distance, rh52.distance,
            rb.velocity.magnitude
        };

        // Feed the input vector into the neural network and get a V2 response
        // output[0] = thrust
        // output[1] = wheel rotation
        double[] output = brain.feedForward(input);
        result = Vector<double>.Build.DenseOfArray(output);

        // Set thrust on car wheels
        w11.motorTorque = (float)result[0];
        w12.motorTorque = (float)result[0];

        // Set rotation to stearing wheel
        w21.steerAngle = (float)result[1] / 5;
        w22.steerAngle = (float)result[1] / 5;

        // Collect location points every second to calculate approx
        // distance traveled for fitness function
        int currentSec = ((int)Time.timeSinceLevelLoad + 1);
        if (lastTime < currentSec)
        {
            path.Add(transform.position);
            lastTime = currentSec;
        }
    }

    // Returns the car's network encoded into a gene 
    public Chromosome getChromosome()
    {
        Chromosome _c = brain.getChromosome();

        _c.setFitness(distance);

        return _c;
    }

    private void calculateFitness()
    {
        if (path.Count > 0)
        {
            Vector3 comp = path[0];

            // Calculate this total distance travelled
            for (int v = 0; v < path.Count; v++)
            {
                distance += Vector3.Distance(comp, path[v]);
                comp = path[v];
            }
        }

        // calculate the overall fitness (actual fitness function)
        fitness = Mathf.Pow(distance * distance, 1 / (1 + totalTime));
    }

    void terminate()
    {
        running = false;
        
        // add point of termination to total path
        // and calculate the fitness function
        path.Add(transform.position);
        calculateFitness();

        totalTime = itterationTime - Time.timeSinceLevelLoad;
        engine.notify(fitness);
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision c)
    {
        if (running && c.gameObject.tag == "Terrain")
        {
            terminate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject.tag == "CheckLine")
            checkLine = true;

        if (other.transform.gameObject.tag == "FinishLine")
        {
            endOfTheLine = true;
            //running = false;
            string _s = brain.toString();

        }
    }
}
