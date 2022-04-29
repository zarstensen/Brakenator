#include "Brakenator.h"

#define _USE_MATH_DEINFES
#include <cmath>
#include <string>
#include <curl/curl.h>
#include <rapidjson/document.h>
#undef min
#undef max
#include <fstream>
#include <sstream>
#include <chrono>
#include <filesystem>
#include <array>

using namespace std::chrono;
using namespace std::chrono_literals;

namespace rjson = rapidjson;

using BNduration = high_resolution_clock::duration;
using Path = std::filesystem::path;

// is set to true, if the weather has been set by the user.
bool s_user_weather = false;
// the weather id used to approximate the friction coefficient.
BN_WEATHER s_weather = BN_ICY; // assume worst condition
// stores whether the current weather was set manually.
bool s_manual_weather;

// how often the weather should be updated, if the location is unchanged
BNduration s_weather_freq = 10min;

BNduration s_weather_last_call = 0min;

double s_min_weather_distance = 2.5; // km
double s_min_elevation_distance = 8; // m

double s_prev_lat = INFINITY;
double s_prev_lon = INFINITY;

double s_temperature = 0.0;

double s_reaction = 1.25;

Path s_weather_key_file;

constexpr double GRAVITY = 9.82;

CURL* s_curl = nullptr;

struct ElevationPoint
{
    double lat = INFINITY;
    double lon = INFINITY;
    double elevation = INFINITY;
};


std::pair<ElevationPoint, ElevationPoint> s_slope_elevations;

///@brief structure containing a friction coefficient table, to be used by the Brakenator library in order to determine the cars braking info.
struct FrictionCoeffs
{
    // FrictionCoeffs(std::pair<double, double>* dry, std::pair<double, double>* wet);

    ///@brief an array of maps mapping the velocity of the car depending on the road condition.
    /// the index of the array is the corresponding BN_WEATHER condition enum value.
    std::array<std::map<double, double>, 4> coeffs;

    double getCoeff(BN_WEATHER weather, double velocity)
    {
        auto m_iter = coeffs[weather].begin();

        // use the first two table values as the default values

        double b_vel = m_iter->first;
        double b_coeff = m_iter->second;

        m_iter++;

        // if only one coefficient is stored, just return this value no matter the velocity.
        if(m_iter == coeffs[weather].end())
            return b_coeff;

        double a_vel = m_iter->first;
        double a_coeff = m_iter->second;

        // find the two table values right before or after the passed velocity.
        // if the velocity is outside the table velocities, the final values will either be the first two table values or the last two table values.
        for(;++m_iter != coeffs[weather].end();)
        {
            if(velocity <= m_iter->first)
                break;

            b_vel = a_vel;
            b_coeff = a_coeff;

            a_vel = m_iter->first;
            a_coeff = m_iter->second;
        }

        // interpolate linearly between the velocities

        double a = (a_coeff - b_coeff) / (a_vel - b_vel);
        double b = a_coeff - a * a_vel;

        double res_coeff = a * velocity + b;

        return std::max(res_coeff, 0.0);
    }
};

FrictionCoeffs s_friction_coeffs;

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
    static constexpr double EARTH_RAD = 6371.290681854754;
    
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
size_t curlGetCallback(char* buffer, size_t, size_t nitems, void* userdata)
{
    std::string* response_str = (std::string*)userdata;

    response_str->append(buffer, nitems);

    return nitems;
}

// ======== EXPORTED FUNCTIONS ========

BN_API void BNinit()
{
    s_curl = curl_easy_init();
}

BN_API void BNcleanup()
{
    curl_easy_cleanup(s_curl);
}

void addCoeff(BN_WEATHER weather, double velocity, double coeff)
{
    s_friction_coeffs.coeffs[weather][velocity / 3.6] = coeff;
}

void removeCoeff(BN_WEATHER weather, double velocity)
{
    s_friction_coeffs.coeffs[weather].erase(velocity);
}

void setWeather(BN_WEATHER weather, bool user)
{
    if(user)
        s_user_weather = true;

    s_weather = weather;
}

void clearUserWeather()
{
    if (s_user_weather)
    {
        s_prev_lat = INFINITY;
        s_prev_lon = INFINITY;

        s_weather_last_call = seconds(0);
    }

    s_user_weather = false;
}

bool isUserWeather()
{
    return s_user_weather;
}

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
    if(velocity > 1e-3)
    {
        velocity /= 3.6;
        double slope = slopeAngle();

        double dacc = GRAVITY * (s_friction_coeffs.getCoeff(s_weather, velocity) * cos(slope) + sin(slope));

        // start by calculating the braking distance
        info_out->distance = ipow(velocity, 2) / (2 * dacc);

        // use the distance calculated to calculate the time
        info_out->time = velocity / dacc + s_reaction;

        info_out->distance += velocity * s_reaction;
    }
    else
    {
        info_out->distance = 0;
        info_out->time = 0;
    }
}

BN_ERR autoWeather(double lat, double lon)
{
    // if the user has set the weather, do nothing.
    if(s_user_weather)
        return BN_OK;

    if(s_prev_lat + s_prev_lon == INFINITY || coordToDistance(s_prev_lat, s_prev_lon, lat, lon) > s_min_weather_distance ||
    high_resolution_clock::now().time_since_epoch() - s_weather_last_call > s_weather_freq)
    {
        // read the api key
            
        if(!std::filesystem::exists(s_weather_key_file))
            return BN_INVALID_API_KEY;

        std::ifstream key_file(s_weather_key_file);

        std::string key;
            
        std::getline(key_file, key);

        key_file.close();

        std::chrono::system_clock::time_point current_tpoint = std::chrono::system_clock::now();
        std::time_t current_time = std::chrono::system_clock::to_time_t(current_tpoint);
        std::tm* local_time = std::localtime(&current_time);
        // setup curl get request for the current weather

        std::stringstream header;
        header << "http://api.openweathermap.org/data/2.5/onecall/timemachine?units=metric&dt=" << std::chrono::duration_cast<std::chrono::seconds>(current_tpoint.time_since_epoch()).count() - 60 << "&lat=" << lat << "&lon=" << lon << "&appid=" << key;

        curl_easy_setopt(s_curl, CURLOPT_URL, header.str().c_str());
        curl_easy_setopt(s_curl, CURLOPT_HTTPGET, true);

        std::string response;

        curl_easy_setopt(s_curl, CURLOPT_WRITEDATA, &response);
        curl_easy_setopt(s_curl, CURLOPT_WRITEFUNCTION, curlGetCallback);

        int res = curl_easy_perform(s_curl);
        // check if get request succeded

        if(res == CURLE_COULDNT_RESOLVE_HOST)
        {
            return BN_HOST_ERROR;
        }
        else if(res != CURLE_OK)
        {
            return BN_UNKNOWN;
        }

        rjson::Document json_response;

        json_response.Parse(response.c_str());

        // check response status code

        if(json_response.HasMember("cod"))
        {
            std::string res_str = json_response["cod"].GetString();
            int res_code = std::stoi(res_str);

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
                return BN_HOST_ERROR;
        }

        // store the weather id


        uint16_t current_weather_id = (uint16_t)json_response["current"]["weather"][0]["id"].GetInt();
        double current_temp = json_response["current"]["temp"].GetDouble();


        // if it is currently raining alot, assume the road is layered with water.
        if(current_weather_id >= 201 && current_weather_id <= 202 || // thunder and rain
            current_weather_id >= 313 && current_weather_id <= 314 || // drizzle and rain
            current_weather_id >= 501 && current_weather_id <= 504 ||
            current_weather_id >= 521 && current_weather_id <= 531    // rain
        )
        {
            s_weather = BN_WLAYER;
        }
        else
        {
            bool is_wet = false;
            is_wet = isWet(current_weather_id);

            if (!is_wet)
            {
                for (uint16_t i = 0; i < local_time->tm_hour && i < 6; i++)
                {
                    if (isWet((uint16_t)json_response["hourly"][i]["weather"][0]["id"].GetInt()))
                    {
                        is_wet = true;
                        break;
                    }
                }
            }

            // if the time is less than 6 o clock, the previous day must be retrieved
            if (!is_wet && local_time->tm_hour < 6)
            {
                size_t new_time = (std::chrono::duration_cast<std::chrono::seconds>(current_tpoint.time_since_epoch()).count() / 86400) * 86400 - 1;

                // prepare new request for the previous day.
                header.clear();
                header << "http://api.openweathermap.org/data/2.5/onecall/timemachine?units=metric&dt=" << new_time << "&lat=" << lat << "&lon=" << lon << "&appid=" << key;

                curl_easy_setopt(s_curl, CURLOPT_URL, header.str().c_str());

                res = curl_easy_perform(s_curl);

                if (res == CURLE_COULDNT_RESOLVE_HOST)
                {
                    return BN_HOST_ERROR;
                }
                else if (res != CURLE_OK)
                {
                    return BN_UNKNOWN;
                }

                json_response.Parse(response.c_str());

                if (json_response.HasMember("cod"))
                {
                    std::string res_str = json_response["cod"].GetString();
                    int res_code = std::stoi(res_str);

                    switch (res_code)
                    {
                    case 401:
                        return BN_INVALID_API_KEY;
                    case 429:
                        return BN_EXCEEDED_API_LIMIT;
                    case 408:
                        return BN_TIMED_OUT;
                    }

                    if (res_code >= 400)
                        return BN_HOST_ERROR;
                }

                if (isWet((uint16_t)json_response["current"]["weather"][0]["id"].GetInt()))
                {
                    is_wet = true;
                }
                else
                {
                    // loop over the missing hours
                    for (uint16_t i = 0; i < 6 - local_time->tm_hour; i++)
                    {
                        if (isWet((uint16_t)json_response["hourly"][i]["weather"][0]["id"].GetInt()))
                        {
                            is_wet = true;
                            break;
                        }
                    }
                }
            }

            if (!is_wet)
                s_weather = BN_DRY;
            else if (is_wet)
                s_weather = current_temp < 0 ? BN_ICY : BN_WET;
        }

        s_prev_lat = lat;
        s_prev_lon = lon;

        s_weather_last_call = high_resolution_clock::now().time_since_epoch();
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
    coordToDistance(s_slope_elevations.first.lat, s_slope_elevations.first.lon, lat, lon) < s_min_elevation_distance)
        return BN_OK;

    // use open topo data to get the elevation.
    std::stringstream header;

    header << "https://api.opentopodata.org/v1/aster30m?locations=" << lat << ',' << lon;
    curl_easy_setopt(s_curl, CURLOPT_URL, header.str().c_str());
    curl_easy_setopt(s_curl, CURLOPT_HTTPGET, true);

    std::string response;

    curl_easy_setopt(s_curl, CURLOPT_WRITEDATA, &response);
    curl_easy_setopt(s_curl, CURLOPT_WRITEFUNCTION, curlGetCallback);
            
    int res = curl_easy_perform(s_curl);
            
    if(res == CURLE_COULDNT_RESOLVE_HOST)
    {
        return BN_HOST_ERROR;
    }
    else if(res != CURLE_OK)
    {
        return BN_UNKNOWN;
    }

    rjson::Document json_response;
    json_response.Parse(response.c_str());

    // check response status code
    std::string res_str = json_response["status"].GetString();

    if(res_str == "INVALID_REQUEST")
        return BN_INVALID_REQUEST;
    else if(res_str == "SERVER_ERROR")
        return BN_UNKNOWN;

    // store the weather id

    double elevation_value = json_response["results"][0]["elevation"].GetDouble();

    s_slope_elevations.second = s_slope_elevations.first;
    s_slope_elevations.first = ElevationPoint{lat, lon, elevation_value / 1000.0 /*km*/ };

    return BN_OK;
}
