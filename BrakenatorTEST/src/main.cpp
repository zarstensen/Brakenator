#include <Brakenator.h>

int main()
{
	BNinit();
	setWeatherKey("weather_key.txt");
	std::cout << autoWeather(49.08939117171668, 12.90991949851313) << '\n';


	addCoeff(BN_DRY, 50, 1);
	addCoeff(BN_WET, 50, .5);
	addCoeff(BN_WLAYER, 50, .4);
	addCoeff(BN_ICY, 50, .1);
	addCoeff(BN_DRY, 90, .95);
	addCoeff(BN_WET, 90, .2);
	addCoeff(BN_WLAYER, 90, .1);
	addCoeff(BN_DRY, 130, .9);
	addCoeff(BN_WET, 130, .2);
	addCoeff(BN_WLAYER, 130, .1);

	setWeather(BN_ICY);
	std::cout << getWeather() << '\n';

	BrakingInfo binf;

	getBrakingInfo(50, &binf);

	std::cout << binf.distance << ':' << binf.time << '\n';

	setWeather(BN_WLAYER);
	std::cout << getWeather() << '\n';

	getBrakingInfo(50, &binf);

	std::cout << binf.distance << ':' << binf.time << '\n';

	setWeather(BN_WET);

	getBrakingInfo(50, &binf);

	std::cout << binf.distance << ':' << binf.time << '\n';

	setWeather(BN_DRY);

	getBrakingInfo(50, &binf);

	std::cout << binf.distance << ':' << binf.time << '\n';

}
