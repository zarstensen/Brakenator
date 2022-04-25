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

extern "C"
{

	enum BN_ERR
	{
		BN_OK = 0,
		BN_INVALID_API_KEY,
		BN_INVALID_REQUEST,
		BN_TIMED_OUT,
		BN_EXCEEDED_API_LIMIT,
		BN_INVLAID_FILE,
		BN_UNKNOWN
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

	BN_API void setWeatherKey(const char* path);

	BN_API void setReactionTime(double reaction);

	BN_API BN_ERR getBrakingDistance(double lat, double lon, double& out_distance, double& out_time);

	///@brief sets the wether type that should be used when approximating the friction coefficient.
	/// for details on how to pass the parameters, see https://openweathermap.org/weather-conditions
	BN_API void setWeather(WeatherID weather_id);
	///@brief automaticly determines the weather based on the location passed.
	///@return returns BN_OK on success, and the failure code on a failure.
	BN_API BN_ERR autoWeather(double lat, double lon);
	///@brief gets the current weather id used to approximate the friction coefficient.
	BN_API const WeatherID* getWeather();

	///@brief gets the elevation for the passed location.
	BN_API BN_ERR sampleElevation(double lat, double lon);
}
 
///@brief retrieve the road angle the car is currently driving on.
double slopeAngle();
