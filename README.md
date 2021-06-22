# Graphy

## General Info
Graphy allows you to write texts or draw vector images easily in Catia V5 on complex surfaces and directions.<br />
Genrate drawings directly from the interface or with a csv file for batch applications then kept them as a surface or transform them in volume.<br />
Unfortunatly drawings generated cannot be modified after.<br />
Graphy only uses Generative Shape Design module; it does not need other specific Catia V5 license.<br />
![Hello World](/Images/Hello_World.png)

From version v3.2, it has been tested with Catia V5 R19, R20 and R28.
Work fine if compiled with the associated reference Catia dlls:
- INFITF.dll
- PARTITF.dll
- HybridShapeTypeLib.dll
- SPATypeLib.dll
- MECMOD.dll
- KnowledgewareTypeLib.dll


## How to use
Graphy just needs 4 basic shapes:
- A plane or a surface where the projection is done.
- A line or a curve to follow. Should be constructed on the surface.
- A reference point to locate the marking. Should be constructed on the surface and on the line.
- An axis system to orientate the drawing:
  - The axis system origin must be the reference point.
  - The X axis must be the tangent to the curve passing by its origin. Its direction points the marking direction.
  - The Y axis is the results of X and Z axis. Its direction points the characters direction.
  - The Z axis must be the normal to the surface passing by its origin. Its direction points the material adding direction.



## Third party software

MVVMLight
Copyright (c) 2009 - 2018 Laurent Bugnion under the MIT License (MIT).
https://github.com/lbugnion/mvvmlight/blob/master/LICENSE
