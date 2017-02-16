// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the AURORALEAGUELOGILEDWRAPPER_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// AURORALEAGUELOGILEDWRAPPER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef AURORALEAGUELOGILEDWRAPPER_EXPORTS
#define AURORALEAGUELOGILEDWRAPPER_API __declspec(dllexport)
#else
#define AURORALEAGUELOGILEDWRAPPER_API __declspec(dllimport)
#endif

// This class is exported from the Aurora-LeagueLogiLEDWrapper.dll
class AURORALEAGUELOGILEDWRAPPER_API CAuroraLeagueLogiLEDWrapper {
public:
	CAuroraLeagueLogiLEDWrapper(void);
	// TODO: add your methods here.
};

extern AURORALEAGUELOGILEDWRAPPER_API int nAuroraLeagueLogiLEDWrapper;

AURORALEAGUELOGILEDWRAPPER_API int fnAuroraLeagueLogiLEDWrapper(void);
