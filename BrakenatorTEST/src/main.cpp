#include <Brakenator.h>

int main()
{
	autoWeather(0, 0);

	sampleElevation(55.784347, 12.497148);
	sampleElevation(55.783871, 12.490840);

	std::cout << slopeAngle();
}
