// Aurora-LightFXWrapper.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "Aurora-LightFXWrapper.h"


// This is an example of an exported variable
AURORALIGHTFXWRAPPER_API int nAuroraLightFXWrapper=0;

// This is an example of an exported function.
AURORALIGHTFXWRAPPER_API int fnAuroraLightFXWrapper(void)
{
    return 42;
}

// This is the constructor of a class that has been exported.
// see Aurora-LightFXWrapper.h for the class definition
CAuroraLightFXWrapper::CAuroraLightFXWrapper()
{
    return;
}
