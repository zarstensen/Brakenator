#ifdef CS_EXPORTS
#define CS_API __declspec(dllexport)
#else
#define CS_API __declspec(dllimport)
#endif

#include <iostream>
#include <string>

void CS_API printStuff(std::string str, int repeat);
