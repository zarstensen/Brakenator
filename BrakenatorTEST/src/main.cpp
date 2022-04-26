#include <Brakenator.h>

int main()
{

	setWeatherKey("weather_key.txt");

	std::cout << "AWR: " << autoWeather(51.142622, 9.493477) << '\n';

	BrakingInfo info;

	getBrakingInfo(130 / 3.6, &info);

	std::cout << info.distance << ':' << info.time << '\n';

	std::cout << getWeather() << '\n';
}
