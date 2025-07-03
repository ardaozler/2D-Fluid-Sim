# 2D Fluid Simulation - Jos Stam's Algorithm


This Unity(6000.0.25f1) project implements a fluid simulation algorithm as described in Jos Stam's paper Real-Time Fluid Dynamics for Games, creating realistic fluid dynamics that are used in computer graphics, animation, and games.


![fluidsim](https://media2.giphy.com/media/v1.Y2lkPTc5MGI3NjExem92NWl5dWNmcHA5Z2V1Zm5wb253MHBhdWI1eGgyZ3l3aGUwNGp3dSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/Jv0xM2IEl4glcCKtQh/giphy.gif)


## Table of Contents:
- [Overview](#overview)

- [Features](#features)

- [Usage](#usage)

- [Algorithm Details](#algoritm-details)

## License

### Overview
This project simulates fluid behavior using a grid-based system that tracks fluid properties such as velocity and density. The algorithm consists of multiple steps: advection, diffusion, and projection, which ensure that the fluid remains incompressible and behaves realistically. The code is designed to run in Unity using C#.

### Features
Real-Time Fluid Dynamics: Fluid simulation in real-time using Jos Stam's algorithm.

Interactive Simulation: User input is supported to influence fluid behavior (e.g., mouse interaction for adding velocity and density).

Incompressibility: The simulation ensures that the fluid remains incompressible at all times.

Diffusion and Advection: Implementations of fluid diffusion and advection algorithms for realistic behavior.

Boundary Conditions: Custom boundary conditions for controlling fluid interactions with the environment.

Visualization: Fluid density and velocity visualized through color gradients and arrows to represent velocity direction.


## Usage
Mouse Interaction:

Left-click or hold: Add velocity to the fluid at the mouse position.

Right-click or hold: Increase the density of the fluid at the mouse position.

Adjustable Parameters:

Diffusion Rate: Control how quickly the fluid diffuses.

Advection Count: Control the number of iterations for fluid advection.



## Algorithm Details
Add Source: Adds density and velocity to the grid based on user input or predefined sources.

Diffuse: Spreads density and velocity across neighboring cells to simulate diffusion.

Advect: Moves the density and velocity based on fluid velocity, transporting fluid across the grid.

Projection: Solves for incompressibility, removing any divergence from the velocity field.
