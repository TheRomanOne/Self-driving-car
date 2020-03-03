# Self-driving-car

This is a simulation of neural network powered self driving cars done in Unity.
I used a genetic algorithm optimization for the network.

A population of 150 cars is generated with random neural networks attached to each.
Each cars will feed it's network values from 10 distance sensors and the current
velocity of the car, and get an output for thrust and rotation.

The thrust will be applied on the motorTorque propetry of car's front wheels
The rotation will be applied on the steerAngle propetry of car's back wheels

after the entire generation has stopped moving due to bad thrust or collision
with the environment, a genetic algorithm will be used to calculate a new
generation of 150 cars, with optimized neural networks
