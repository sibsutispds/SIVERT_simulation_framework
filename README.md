# SIVERT V2X simulation framework (Unity3D part)


`SIVERT framework` implements a bi-directionally coupled simulations using [Veneris](http://pcacribia.upct.es/veneris/) [[1]](#1) and [NS3](https://www.nsnam.org) simulator, introducing the spatially-consistent Geometry based Stochastic Channel Models (GSCM) with account for antenna patterns using Effective Aperture Distribution Function (EADF). The LTE-V2X module is reused from the implementation kindly shared by [Eckerman et.al.](https://github.com/FabianEckermann/ns-3_c-v2x) [[2]](#2). Coupling is done via bi-directional API using the TCP/IP sockets between simulators using [ZeroMQ](https://zeromq.org). Original Veneris simulator is extensively extended with GSCM channel modelling. API data structures are implemented using [google flatbuffer](https://google.github.io/flatbuffers/) cross platform serialization library. The real-time execution capability of the framework is achieved by the optimization of the GSCM implementation explained in the following [paper](https://ieeexplore.ieee.org/document/9723279) [[3]](#3).

The XC40 3D model currently used in the project: [Showroom with a 3D model of Volvo Cars’ electric SUV XC40 Recharge](https://blog.unity.com/manufacturing/take-a-ride-with-the-auto-showroom-sample-template-and-volvo-xc40-recharge)

# Installation

Does not require any special installation. Current version is running using `Unity 2019.2.9` (potentially should be possible  to run using later versions of Unity3D, however, was not tested).

**NB** Current version is implemented for **MacOS** only, Linux version should become available later.

## Simulation test game scene

The test scene currently available is __Final demo__ is an example as of now (can be found in `Assets/Scenes`).


## Simulation manager

`SimManagerSIVERT_ECS` game object contains `Sivert_API` script attached for selection of V2X technology and V2X scenario. Currently there are two V2V stacks available: DSRC 802.11p based on WAVE NS3 module and LTE-V2X Mode 4 implementation based on LTE module (see references above).
Any of two V2X stacks can be picked from drop-down menu.


### Scenarios

There're currently 3 V2X application options available:
- `Lund_intersection` - in which vehicle driving from the south will brake automatically upon reception of the first message from vehicle coming form north direction.
- `Demo_intersection_assist` - where cameras inside on the driver position of the vehicles are activated. __Display 3__ is located in the vehicle approaching the intersection that has to give a way. This vehicle will project the Warning message on the screen upon reception of the beacon from the vehicle on a collision course.
- `None` disables any additional logic upon reception of the V2X messages while keeping the normal operation of the V2X communications.

### Logger

Data from each simulation run is recorded by `SivertSqLiteLogger` into a separate SQLite database and can be used after for the analysis. The name of the output database defined in `SivertSqLiteLogger` script attached to the simulation manager. The database after simulation run can be found in the `Results/**scene_name**` folder located at the root directory of the project.

# NS3 binary

By default SIVERT will use the NS3 binary located in the `Packages/SIVERT_NS3_bin`, this is controlled by the `UseNS3Binary` flag in the simulation manager. This is pre-build executable (for MacOS) from NS3. If users would like to modify NS3 scenario, parameters or introduce the extensions, they needs to build NS3 project source shared in [SIVERT_NS3_to_be_shared](https://github.com/sibsutispds/NS3git.git) and modify the scenario.


## NS3 dependencies

Short manual so far for NS3 part.
1. install flatbuffers, add simlynk for include/flatbuffers to /usr/local/include. For instance:
`sudo ln -s /{_pathtoSIVERTns3_}/flatbuffers/include/flatbuffers /usr/local/include`
2. install cppzmq from git, add symlink for zmq.hpp to /usr/local/include
`sudo ln -s /{_pathtoSIVERTns3_}/cppzmq-4.2.2/zmq.hpp /usr/local/include`
3. Take zhelpers.hpp from zquide git and add symlink to /usr/local/include
`sudo ln -s /{_pathtoSIVERTns3_}/cppzmq-4.2.2/zhelpers.hpp /usr/local/include`



## ManagerGSCM

The GSCM parameters can be modified in this prefab. The main one is the radiation pattern of the antenna installed on the vehicles. Currently there are several exemplary simple patterns (Isotropic and several differently oriented dipoles) - the patterns diagrams plots in horizontal plane can be found in `Antennas` folder in the root of the project. User can substitute these pattern with any other pattern implemented in the EADF format adding it to the `ChannelGenManager` script using it as a boilerplate.

## References

<a id="1">[1]</a>
Esteban Egea-Lopez, Fernando Losilla, Juan Pascual-Garcia and Jose Maria Molina-Garcia-Pardo, "Vehicular Network Simulation with Realistic Physics", IEEE Access, 2019, DOI:10.1109/ACCESS.2019.2908651

<a id="2">[2]</a>
F. Eckermann, M. Kahlert, C. Wietfeld, "Performance Analysis of C-V2X Mode 4 Communication Introducing an Open-Source C-V2X Simulator", In 2019 IEEE 90th Vehicular Technology Conference (VTC-Fall), Honolulu, Hawaii, USA, September 2019.

<a id="3">[3]</a>
A. Fedorov, N. Lyamin and F. Tufvesson, "Implementation of spatially consistent channel models for real-time full stack C-ITS V2X simulations," 2021 55th Asilomar Conference on Signals, Systems, and Computers, 2021, pp. 67-71, doi: 10.1109/IEEECONF53345.2021.9723279.
