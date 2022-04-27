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
#include <map>

extern "C"
{

	enum BN_ERR: short
	{
		BN_OK = 0,
		BN_INVALID_API_KEY,
		BN_INVALID_REQUEST,
		BN_TIMED_OUT,
		BN_EXCEEDED_API_LIMIT,
		BN_INVLAID_FILE,
		BN_HOST_ERROR,
		BN_INVALID_ARGS,
		BN_UNKNOWN
	};

	enum BN_WEATHER: short
	{
		BN_DRY,
		BN_WET,
		BN_WLAYER,
		BN_ICY
	};

	///@brief structure containing information about a weather type.
	struct WeatherID
	{
		///@brief the last digit in the openweather weather id.
		/// broadly describes the type of weather.
		uint16_t group = 0;
		///@brief the middle digit in the openweather weather id.
		/// describes a sub group of the current weather type
		uint8_t sub_group = 0;
		///@brief the first digit in the openweather weather id.
		/// describes how severe the current weather type is.
		uint8_t severity = 0;

		///@brief check if two WeatherID's are equal (true only if all of the attributes are equal)
		bool operator==(const WeatherID& other)
		{
			return group == other.group && sub_group == other.sub_group && severity == other.severity;
		}
	};

	///@brief structure containing information about the braking of the car.
	struct BrakingInfo
	{
		///@brief the distance the car will have to travel, before its velocity reaches 0.
		double distance;
		///@brief the time interval the car will have to brake in, before its velocity reaches 0.
		double time;
	};

	///@brief adds an entry into the velocity-friction coefficient table for the passed weather id.
	BN_API void addCoeff(BN_WEATHER weather, double velocity, double coeff);
	///@brief adds an entry into the velocity-friction coefficient table for the passed weather id and velocity.
	BN_API void removeCoeff(BN_WEATHER weather, double velocity, double coeff);

	///@brief retrieves the braking distance and time estimated from the current conditions of the roead.
	///@param velocity the current velocity of the car, in km/h.
	///@param info_out a pointer to the structure of which getBrakingInfo should write its result to.
	BN_API void getBrakingInfo(double velocity, BrakingInfo* info_out);
	
	///@brief sets the directory of the openweathermap api key file.
	BN_API void setWeatherKey(const char* path);

	///@brief sets the reaction time that should be used in the braking distance estimation
	BN_API void setReactionTime(double reaction);

	///@brief sets the wether type that should be used when approximating the friction coefficient.
	/// for details on how to pass the parameters, see https://openweathermap.org/weather-conditions
	BN_API void setWeather(BN_WEATHER weather, bool user = true);
	///@brief clears the user_weather flag.
	/// This allows autoWeather to override the current weather.
	BN_API void clearUserWeather();
	///@brief automaticly determines the weather based on the location passed.
	///@return returns BN_OK on success, and the failure code on a failure.
	BN_API BN_ERR autoWeather(double lat, double lon);
	///@brief gets the current weather id used to approximate the friction coefficient.
	BN_API BN_WEATHER getWeather();

	///@brief samples the elevation for the passed location.
	BN_API BN_ERR sampleElevation(double lat, double lon);
}
