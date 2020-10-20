# Graphy

## General Info
Graphy allows you to write 3D texts easily in Catia V5 on complex surfaces and directions.<br />
Text obtained can be kept as a surface or in volume but cannot be modified directly in Catia.<br />
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
Graphy needs 4 basic shapes:
- A surface where the projection is done.
- A line or a curve to follow. Should be constructed on the surface.
- A starting point. Should be constructed on the surface and on the line.
- An axis system to orientate the drawing:
  - The axis system origin must be the starting point.
  - The X axis must be the tangent to the curve passing by its origin. Its direction points the marking direction.
  - The Y axis is the results of X and Z axis. Its direction points the characters direction.
  - The Z axis must be the normal to the surface passing by its origin. Its direction points the material adding direction.
<br />
Example of possible orientations just by changing the axis system directions.<br />
![AxisSystemEffect](/Images/AxisSystemEffect.png)




## Third party software

MVVMLight
Copyright (c) 2009 - 2018 Laurent Bugnion under the MIT License (MIT).
https://github.com/lbugnion/mvvmlight/blob/master/LICENSE
