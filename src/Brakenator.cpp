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
BN_WEATHER s_weather;
// stores whether the current weather was set manually.
bool s_manual_weather;

// how often the weather should be updated, if the location is unchanged
BNduration s_weather_freq = 10min;

BNduration s_weather_last_call = 0min;

double s_min_weather_distance = 2.5; // km

double s_prev_lat = INFINITY;
double s_prev_lon = INFINITY;

double s_temperature = 0.0;

double s_reaction = 1;

Path s_weather_key_file;

constexpr double GRAVITY = 9.82;

struct ElevationPoint
{
    double lat = INFINITY;
    double lon = INFINITY;
    double elevation = INFINITY;
};

std::pair<ElevationPoint, ElevationPoint> s_slope_elevations;

// ======== PRIVATE FUNCTIONS ========

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

// get the estimated friction coefficient based on the current weather.
double getMu()
{
    switch(s_weather)
    {
        case BN_DRY:
            return 0.8;
            break;
        case BN_WET:
            return 0.6;
            break;
        case BN_ICE:
            return 0.3;
            break;
        default:
            // this should never be hit.
            return -1;
    }
}

bool isWet(uint16_t wid)
{
    uint16_t group = wid / 100;
    uint16_t sub_group = wid / 10 % 10;

    return (group == 2 && (sub_group == 1 || sub_group == 3)) || group >= 3 && group <= 6;
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

// get the slope of the road, based on previously recorded elevation values
double slopeAngle()
{
    if(s_slope_elevations.second.elevation != INFINITY)
    {
        double d_height = s_slope_elevations.first.elevation - s_slope_elevations.second.elevation;
        double d_length = coordToDistance(s_slope_elevations.first.lat, s_slope_elevations.first.lon,
            s_slope_elevations.second.lat, s_slope_elevations.second.lon);

        return atan(d_height / d_length);
    }
    else
    {
        return 0;
    }
}

// callback function for curl
// stores the GET response into the userdata, which is assumed to be a std::string*.
size_t curlGetCallback(char* buffer, size_t size, size_t nitems, void* userdata)
{
    std::cout << nitems << '\n';
    std::string* response_str = (std::string*)userdata;

    response_str->append(buffer, nitems);

    return size;
}

// ======== EXPORTED FUNCTIONS ========


void setWeatherKey(const char* path)
{
    s_weather_key_file = path;
}

void setReactionTime(double reaction)
{
    s_reaction = reaction;
}

void getBrakingInfo(double velocity, BrakingInfo* info_out)
{
    double slope = slopeAngle();

    // start by calculating the braking distance
    info_out->distance = -sqrt(ipow(velocity, 2) / GRAVITY * (getMu() * cos(slope) + sin(slope))) + velocity * s_reaction;

    // use the distance calculated to calculate the time
    info_out->time = (sqrt(ipow(velocity, 2) * 2 * GRAVITY * (getMu() * cos(slope) + sin(slope))) * info_out->distance - velocity) / (GRAVITY * (getMu() * cos(slope) + sin(slope))) + s_reaction;
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
            
            if(!std::filesystem::exists(s_weather_key_file))
                return BN_INVALID_API_KEY;

            std::cout << "FOUND\n";

            std::ifstream key_file(s_weather_key_file);

            std::string key;
            
            std::getline(key_file, key);

            key_file.close();

            std::chrono::system_clock::time_point current_tpoint = std::chrono::system_clock::now();
            std::time_t current_time = std::chrono::system_clock::to_time_t(current_tpoint);
            std::tm* local_time = std::localtime(&current_time);
            // setup curl get request for the current weather

            std::stringstream header;
            header << "https://api.openweathermap.org/data/2.5/onecall/timemachine?units=metric&dt=" << std::chrono::duration_cast<std::chrono::seconds>(current_tpoint.time_since_epoch()).count() << "&lat=" << lat << "&lon=" << lon << "&appid=" << key;
            
            std::cout << header.str() << '\n';

            curl_easy_setopt(curlh, CURLOPT_URL, header.str().c_str());
            curl_easy_setopt(curlh, CURLOPT_HTTPGET, true);

            std::string response;

            curl_easy_setopt(curlh, CURLOPT_WRITEDATA, &response);
            curl_easy_setopt(curlh, CURLOPT_WRITEFUNCTION, curlGetCallback);

            curl_easy_perform(curlh);


            rjson::Document json_response;
            std::cout << "RESPONSE: " << response << '\n';

            json_response.Parse(response.c_str());

            // check response status code

            if(json_response.HasMember("cod"))
            {
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
            }

            // store the weather id


            double current_temp = json_response["current"]["temp"].GetDouble();

            bool is_wet = false;

            std::cout << "CW: " << json_response["current"]["weather"][0]["id"].GetInt() << '\n';

            is_wet = isWet(json_response["current"]["weather"][0]["id"].GetInt());

            std::cout << is_wet << '\n';

            if(!is_wet)
                for(uint16_t i = 0; i < local_time->tm_hour && i < 6; i++)
                {
                    std::cout << "CWH: " << json_response["hourly"][i]["weather"][0]["id"].GetInt() << '\n';
                    if(isWet(json_response["hourly"][i]["weather"][0]["id"].GetInt()))
                    {
                        
                        is_wet = true;
                        break;
                    }
                }

            
            // if the time is less than 6 o clock, the previous day must be retrieved
            if(!is_wet && local_time->tm_hour < 6)
            {
                size_t new_time = (std::chrono::duration_cast<std::chrono::seconds>(current_tpoint.time_since_epoch()).count() / 86400) * 86400 - 1 ;

                // prepare new request for the previous day.
                header.clear();
                header << "https://api.openweathermap.org/data/2.5/onecall/timemachine?units=metric&dt=" << new_time << "&lat=" << lat << "&lon=" << lon << "&appid=" << key;

                curl_easy_setopt(curlh, CURLOPT_URL, header.str().c_str());

                curl_easy_perform(curlh);

                json_response.Parse(response.c_str());

                if(isWet(json_response["current"]["weather"][0]["id"].GetInt()))
                {
                    is_wet = true;
                }
                else
                {
                    // loop over the missing hours
                    for(uint16_t i = 0; i < 6 - local_time->tm_hour; i++)
                    {
                        if(isWet(json_response["hourly"][i]["weather"][0]["id"].GetInt()))
                        {
                            is_wet = true;
                            break;
                        }
                    }
                }

            }


            curl_easy_cleanup(curlh);

            if(!is_wet)
                s_weather = BN_DRY;
            else if(is_wet)
                s_weather = current_temp < 0 ? BN_ICE : BN_WET;

            std::cout << "IW: " << is_wet << " CW: " << s_weather << "CT: " << current_temp << '\n';
        }
        else
        {
            return BN_UNKNOWN;
        }
    }

    return BN_OK;
}

BN_WEATHER getWeather()
{
    return s_weather;
}

BN_ERR BN_API sampleElevation(double lat, double lon)
{
    if(s_slope_elevations.first.elevation == INFINITY ||
    coordToDistance(s_slope_elevations.first.lat, s_slope_elevations.first.lon, lat, lon) < 30)
        return BN_OK;

    // use open topo data to get the elevation.

    CURL* curlh = curl_easy_init();

    if(curlh)
    {
         std::stringstream header;

            header << "https://api.opentopodata.org/v1/aster30m?locations=" << lat << ',' << lon;
            curl_easy_setopt(curlh, CURLOPT_URL, header.str().c_str());
            curl_easy_setopt(curlh, CURLOPT_HTTPGET, true);

            std::string response;

            curl_easy_setopt(curlh, CURLOPT_WRITEDATA, &response);
            curl_easy_setopt(curlh, CURLOPT_WRITEFUNCTION, curlGetCallback);
            
            curl_easy_perform(curlh);
            curl_easy_cleanup(curlh);

            rjson::Document json_response;
            json_response.Parse(response.c_str());

            // check response status code
            std::string res = json_response["status"].GetString();

            if(res == "INVALID_REQUEST")
                return BN_INVALID_REQUEST;
            else if(res == "SERVER_ERROR")
                return BN_UNKNOWN;

            // store the weather id

            double elevation_value = json_response["results"][0]["elevation"].GetDouble();

            s_slope_elevations.second = s_slope_elevations.first;
            s_slope_elevations.first = ElevationPoint{lat, lon, elevation_value / 1000.0 /*km*/ };
    }

    return BN_OK;
}
