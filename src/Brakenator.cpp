#include "Brakenator.h"

int printStuff(const char* str, int repeat)
{
    for(int i = 0; i < repeat; i++)
        std::cout << str << '\n';

    return repeat * 99999;
}
