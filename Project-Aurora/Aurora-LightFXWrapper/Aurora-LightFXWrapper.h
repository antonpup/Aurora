// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the AURORALIGHTFXWRAPPER_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// AURORALIGHTFXWRAPPER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef AURORALIGHTFXWRAPPER_EXPORTS
#define AURORALIGHTFXWRAPPER_API __declspec(dllexport)
#else
#define AURORALIGHTFXWRAPPER_API __declspec(dllimport)
#endif

// This class is exported from the Aurora-LightFXWrapper.dll
class AURORALIGHTFXWRAPPER_API CAuroraLightFXWrapper {
public:
	CAuroraLightFXWrapper(void);
	// TODO: add your methods here.
};

extern AURORALIGHTFXWRAPPER_API int nAuroraLightFXWrapper;

AURORALIGHTFXWRAPPER_API int fnAuroraLightFXWrapper(void);
