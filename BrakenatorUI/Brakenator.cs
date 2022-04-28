using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace Brakenator
{

    /// @brief Wrapper class for the Brakenator.dll
    /// also contains some datatypes needed when recieving data from the dll.
    public class BN
    {
		private const string DLL_NAME = @"Brakenator.dll";
		// ============ DATATYPES ============
		// [StructLayout(LayoutKind.Sequential)]  
		public struct WeatherID
		{
			public ushort group;
			public ushort sub_group;
			public ushort severity;
		}
		public struct BrakingInfo
		{
			public double distance;
			public double time;
		};
		public enum ERR: short
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
		}

		public enum WEATHER: short
		{
			BN_DRY,
			BN_WET,
			BN_WLAYER,
			BN_ICY
		}

		// ============ METHODS ============

		[DllImport(DLL_NAME)]
		/// @brief initializes curl. Should be called before any other functions.
		public static extern void BNinit();
		[DllImport(DLL_NAME)]
		/// @brief cleansup curl. Should be called before any other functions.
		public static extern void BNcleanup();

		[DllImport(DLL_NAME)]
		public static extern void addCoeff(WEATHER weather_id, double velocity, double coeff);


		[DllImport(DLL_NAME)]
		public static extern void removeCoeff(WEATHER weather_id, double velocity);

		[DllImport(DLL_NAME)]
		public static extern void setWeatherKey(string path);

		[DllImport(DLL_NAME)]
		public static extern void getBrakingInfo(double velocity, ref BrakingInfo out_info);

		/// @brief overrides the weather status.
		/// if user is set to true, autoWeather will not be able to override the passed weather type.
		/// autoWeather can be reenabled using the clearUserWeather() method.
		[DllImport(DLL_NAME)]
		public static extern void setWeather(WEATHER weather_id, bool user = true);
		
		///@brief clears the user_weather flag, allowing the autoWeather function to modify the current weather.
		[DllImport(DLL_NAME)]
		public static extern void clearUserWeather();
		///@brief returns wether the current weather has been set by the user.
		[DllImport(DLL_NAME)]

		public static extern bool isUserWeather();
		///@brief automaticly determines the weather based on the location passed.
		///@return returns BN_OK on success, and the failure code on a failure.
		[DllImport(DLL_NAME)]
		public static extern short autoWeather(double lat, double lon);

		///@brief gets the current weather id used to approximate the friction coefficient.
		[DllImport(DLL_NAME)]
		public static extern WEATHER getWeather();

		///@brief gets the elevation for the passed location.
		[DllImport(DLL_NAME)]
		public static extern ERR getElevation(double lat, double lon, ref double out_elevation);
	}
}
