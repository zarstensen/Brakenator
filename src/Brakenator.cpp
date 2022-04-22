#include "Brakenator.h"
// 1:25, 
#define _USE_MATH_DEINFES
#include <cmath>

#include <string>
#include <curl/curl.h>
#include <rapidjson/document.h>
#include <fstream>
#include <sstream>
#include <chrono>
#include <filesystem>

using namespace std::chrono;
using namespace std::chrono_literals;

namespace rjson = rapidjson;

using BNduration = high_resolution_clock::duration;
using Path = std::filesystem::path;

// the weather id used to approximate the friction coefficient.
WeatherID s_current_weather;
// stores whether the current weather was set manually.
bool s_manual_weather;

// how often the weather should be updated, if the location is unchanged
BNduration s_weather_freq = 10min;

BNduration s_weather_last_call = 0min;

double s_min_weather_distance = 2.5; // km

double s_prev_lat = INFINITY;
double s_prev_lon = INFINITY;

Path s_weather_key_file;
Path s_elevation_key_file;

// takes a to the power of n, where n is a whole number.
template<typename T>
constexpr T ipow(T a, size_t n)
{
    T result = 1;

    for(size_t i = 0; i < n; i++)
        result *= a;

    return result;
}

// converts degrees to radians
constexpr double dtor(double deg)
{
    return M_PI / 180 * deg;
}

void setWeatherKey(const char* path)
{
    s_weather_key_file = path;
}

void setElevationKey(const char* path)
{
    s_elevation_key_file = path;
}

///@brief calculates the distane (in km) from (lat1,lon1) to (lat2,lon2) 
double coordToDistance(double lat1, double lon1, double lat2 = 0, double lon2 = 0)
{
    // lattitude will always have an equal length between each degree
    
    // average radius of the earth
    constexpr double EARTH_RAD = 6371.290681854754;
    
    double d_lat = dtor(lat2 - lat1);
    double d_lon = dtor(lon2 - lon1);

    double a = ipow(sin(d_lat / 2), 2) + cos(lat1) * cos(lat2) * ipow(sin(d_lon / 2), 2);

    double c = atan(sqrt(a)) * atan(sqrt(1 - a));

    return EARTH_RAD * c;
}

// ======== EXPORTED FUNCTIONS ========

std::pair<double, double> getBrakingDistance(double lat, double lon)
{
    // if the weather id is unititialized, use autoWeather to get the weather based on location.
    if(s_current_weather == WeatherID())
        autoWeather(lat, lon);

    return { 0, 0 };
}

// callback function for curl
// stores the GET response into the userdata, which is assumed to be a std::string*.
size_t autoWeatherCallback(char* buffer, size_t size, size_t nitems, void* userdata)
{

    std::string* response_str = (std::string*)userdata;

    response_str->append(buffer, nitems);

    return size;
}

BN_ERR autoWeather(double lat, double lon)
{
    if(s_prev_lat + s_prev_lon == INFINITY || coordToDistance(s_prev_lat, s_prev_lon, lat, lon) > s_min_weather_distance ||
    high_resolution_clock::now().time_since_epoch() - s_weather_last_call > s_weather_freq)
    {
        s_prev_lat = lat;
        s_prev_lon = lon;

        s_weather_last_call = high_resolution_clock::now().time_since_epoch();

        CURL* curlh = curl_easy_init();

        if(curlh)
        {
            // read the api key

            std::cout << std::filesystem::absolute(s_weather_key_file) << ':' << std::filesystem::exists(s_weather_key_file) << '\n';

            if(!std::filesystem::exists(s_weather_key_file))
                return BN_INVALID_API_KEY;

            std::ifstream key_file(s_weather_key_file);

            std::string key;

            std::getline(key_file, key);

            key_file.close();

            // setup curl get request

            std::stringstream header;

            header << "https://api.openweathermap.org/data/2.5/weather?lat=" << "40" << "&lon=" << "40" << "&appid=" << key;
            curl_easy_setopt(curlh, CURLOPT_URL, header.str().c_str());
            curl_easy_setopt(curlh, CURLOPT_HTTPGET, true);

            std::string response;

            curl_easy_setopt(curlh, CURLOPT_WRITEDATA, &response);
            curl_easy_setopt(curlh, CURLOPT_WRITEFUNCTION, autoWeatherCallback);


            curl_easy_perform(curlh);

            curl_easy_cleanup(curlh);

            rjson::Document json_response;
            json_response.Parse(response.c_str());

            // check response status code
            int res_code = json_response["cod"].GetInt();

            switch(res_code)
            {
                case 401:
                    return BN_INVALID_API_KEY;
                case 429:
                    return BN_EXCEEDED_API_LIMIT;
                case 408:
                    return BN_TIMED_OUT;
            }

            if(res_code >= 400)
                return BN_UNKNOWN;

            // store the weather id

            uint16_t weather_id = json_response["weather"][0]["id"].GetInt();

            s_current_weather.group = weather_id / 100;
            s_current_weather.sub_group = weather_id / 10 % 10;
            s_current_weather.severity = weather_id % 10;
        }
        else
        {
            return BN_UNKNOWN;
        }
    }

    return BN_OK;
}

WeatherID getWeather()
{
    return s_current_weather;
}

uint16_t getWeatherGroup()
{
    return s_current_weather.group;
}

uint16_t getWeatherGroup()
{
    return s_current_weather.sub_group;
}

uint16_t getWeatherGroup()
{
    return s_current_weather.severity;
}
