#ifdef CS_EXPORTS
#define CS_API __declspec(dllexport)
#else
#define CS_API __declspec(dllimport)
#endif

#include <iostream>
#include <string>

extern "C" int CS_API printStuff(const char* str, int repeat);
