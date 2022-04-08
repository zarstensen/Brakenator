#if defined(WIN32) && defined(BN_DLL)
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
#include <asio.hpp>

extern "C"
{
	int BN_API printStuff(const char* str, int repeat);
	void BN_API makePostReq();
}
