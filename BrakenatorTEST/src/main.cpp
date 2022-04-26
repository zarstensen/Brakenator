#include <Brakenator.h>

int main()
{
	setWeatherKey("weather_key.txt");
	std::cout << autoWeather(49.08939117171668, 12.90991949851313) << '\n';

	sampleElevation(55.784347, 12.497148);
	sampleElevation(55.783871, 12.490840);

	std::cout << getWeather() << '\n';
}
