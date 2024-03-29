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
		BN_OK,
		///@brief the API key file was not found, or the key inside the file is invalid.
		BN_INVALID_API_KEY,
		///@brief the request was invalid. Gets returned if the coordinate is out of bounds eg.
		BN_INVALID_REQUEST,
		///@brief the request has timed out.
		BN_TIMED_OUT,
		///@brief the passed API key has no more requests avaliable
		BN_EXCEEDED_API_LIMIT,
		///@brief an error has occured with the target host of the GET request.
		BN_HOST_ERROR,
		///@brief an unknown error has occured.
		BN_UNKNOWN
	};

	enum BN_WEATHER: short
	{
		///@brief the road is dry.
		BN_DRY,
		///@brief the road is wet.
		BN_WET,
		///@brief the road has a layer of water on top of it.
		BN_WLAYER,
		///@brief the road is icy.
		BN_ICY
	};

	///@brief structure containing information about the braking of the car.
	struct BrakingInfo
	{
		///@brief the distance the car will have to travel, before its velocity reaches 0.
		double distance;
		///@brief the time interval the car will have to brake in, before its velocity reaches 0.
		double time;
	};

	/// @brief initializes curl. Should be called before any other functions.
	BN_API void BNinit();
	/// @brief cleans up curl. Should be called when the application no longer needs to use this codebase.
	BN_API void BNcleanup();

	///@brief adds an entry into the velocity-friction coefficient table for the passed weather id.
	BN_API void addCoeff(BN_WEATHER weather, double velocity, double coeff);
	///@brief adds an entry into the velocity-friction coefficient table for the passed weather id and velocity.
	BN_API void removeCoeff(BN_WEATHER weather, double velocity);

	///@brief retrieves the braking distance and time estimated from the current conditions of the roead.
	///@param velocity the current velocity of the car, in km/h.
	///@param info_out a pointer to the structure of which getBrakingInfo should write its result to.
	BN_API void getBrakingInfo(double velocity, BrakingInfo* info_out);
	
	///@brief sets the directory of the openweathermap api key file.
	BN_API void setWeatherKey(const char* path);

	///@brief sets the reaction time that should be used in the braking distance estimation
	BN_API void setReactionTime(double reaction);

	///@brief sets the wether type that should be used when approximating the friction coefficient.
	BN_API void setWeather(BN_WEATHER weather, bool user = true);
	///@brief clears the user_weather flag.
	/// This allows autoWeather to override the current weather.
	BN_API void clearUserWeather();
	///@brief returns wether the current weather has been set by the user.
	BN_API bool isUserWeather();
	///@brief automaticly determines the weather based on the location passed.
	///@return returns BN_OK on success, and the failure code on a failure.
	BN_API BN_ERR autoWeather(double lat, double lon);
	///@brief gets the current weather id used to approximate the friction coefficient.
	BN_API BN_WEATHER getWeather();

	///@brief samples the elevation for the passed location.
	BN_API BN_ERR sampleElevation(double lat, double lon);
}
