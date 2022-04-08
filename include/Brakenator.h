#ifdef WIN32
#ifdef BN_EXPORTS
#define BN_API __declspec(dllexport)
#else
#define BN_API __declspec(dllimport)
#endif
#else
#define BN_API
#endif

#include <iostream>
#include <string>

extern "C"
{
	int BN_API printStuff(const char* str, int repeat);
}
