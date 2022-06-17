# SIVERT V2X simulation framework (Unity3D part)


`SIVERT framework` implements a bi-directionally coupled simulations using [Veneris](http://pcacribia.upct.es/veneris/) and [NS3](https://www.nsnam.org) simulator, introducing the spatially-consistent Geometry based Stochastic Channel Models (GSCM) with account for antenna patterns using Effective Anerture Distribution Function (EADF). The LTE-V2X module is reused from the implementation kindly shared by [Eckerman et.al.](https://github.com/FabianEckermann/ns-3_c-v2x). Coupling is done via bi-directional API using the TCP/IP sockets between simulators using [ZeroMQ](https://zeromq.org). Veneris is extensively extended with GSCM channel modelling. API data structures are implemented using [google flatbuffer] https://google.github.io/flatbuffers/ cross platform serialization library.  


## Installation

Does not require any special installation. Current version is running using `Unity 2019.2.9` (potentially could be run with later versions of Unity3D).

**NB** Current version is implemented for MacOS, Linux version should become available later.

## Simulation test game scene

The test scene currently available is __Final demo__ is an example as of now (can be found in `Assets/Scenes`).


## Simulation manager

`SimManagerSIVERT_ECS` game object contains `Sivert_API` script attached for selection of V2X technology and V2X scenario. Currently there are two V2V stacks available: DSRC 802.11p based on WAVE NS3 module and LTE-V2X Mode 4 implementation based on LTE module (see references above).
Any of two V2X stacks can be picked from drop-down menu.


### Scenarios

There're currently 3 V2X application options available:
- `Lund_intersection` - in which vehicle driving from the south will brake automatically upon reception of the first message from vehicle coming form north
- `Demo_intersection_assist` - where cameras inside on the driver position of the vehicles are activated. __Display 3__ is located in the vehicle approaching the intersection that has to give a way. This vehicle will project the Warning message on the screen upon reception of the beacon from the vehicle on a collision course.
- `None` disables any additional logic upon reception of the V2X messages while keeping the normal operation of the V2X communications.

### Logger

Data from each simulation run is recorded by SivertSqLiteLogger into a separate sqLite database and can be used after for analysis. The name of the output database defined in `SivertSqLiteLogger` script attached to the simulation manager. The database after simulation run can be found in the `Results/**scene_name**` folder located at the root directory of the project.

### NS3 binary

By default SIVERT will use the NS3 binary located in the `Packages/SIVERT_NS3_bin`, this is defined by the `UseNS3Binary` flag in the simulation manager. This is pre-build executable from NS3. If users would like to modify NS3 scenario, parameters or introduce the extensions, they needs to build NS3 project source shared in [SIVERT_NS3_to_be_shared](https://github.com/sibsutispds/NS3git.git) and modify the scenario.

## ManagerGSCM

The GSCM parameters can be modified in this prefab. The main one is an antenna pattern chosen on the vehicle. Currently there are several exemplary simple patterns (Isotropic and several differently oriented dipoles) - the patterns diagrams plots in horizontal plane can be found in `Antennas` folder in the root of the project. User can substitute these pattern with any other pattern implemented in the EADF format adding it to the `ChannelGenManager` script using it as a boilerplate.
