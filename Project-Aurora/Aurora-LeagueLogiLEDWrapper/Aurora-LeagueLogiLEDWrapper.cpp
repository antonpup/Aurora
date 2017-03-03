// Aurora-LeagueLogiLEDWrapper.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "Aurora-LeagueLogiLEDWrapper.h"


// This is an example of an exported variable
AURORALEAGUELOGILEDWRAPPER_API int nAuroraLeagueLogiLEDWrapper=0;

// This is an example of an exported function.
AURORALEAGUELOGILEDWRAPPER_API int fnAuroraLeagueLogiLEDWrapper(void)
{
    return 42;
}

// This is the constructor of a class that has been exported.
// see Aurora-LeagueLogiLEDWrapper.h for the class definition
CAuroraLeagueLogiLEDWrapper::CAuroraLeagueLogiLEDWrapper()
{
    return;
}
