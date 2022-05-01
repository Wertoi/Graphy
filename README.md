# Graphy

## General Info
Graphy allows you to write texts or vector images easily in Catia V5 on complex surfaces and directions.<br />
Drawings obtained can be kept as a surface or in volume but cannot be modified directly in Catia.<br />
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
- A surface where the projection is done.
- A line or a curve to follow. Should be constructed on the surface.
- A starting point. Should be constructed on the surface and on the line.
- An axis system to orientate the drawing. This system is generated automatically, you just need to invert or not the directions:
  - The X axis direction points the marking direction.
  - The Y axis direction points the characters direction.
  - The Z axis direction points the material adding direction.



## Third party software

MVVMLight
Copyright (c) 2009 - 2018 Laurent Bugnion under the MIT License (MIT).
https://github.com/lbugnion/mvvmlight/blob/master/LICENSE
