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
		public enum ERR: short
        {
			BN_OK = 0,
			BN_INVALID_API_KEY,
			BN_INVALID_REQUEST,
			BN_TIMED_OUT,
			BN_EXCEEDED_API_LIMIT,
			BN_INVLAID_FILE,
			BN_UNKNOWN
		}

		public enum WEATHER: short
		{
			BN_DRY,
			BN_WET,
			BN_ICE
		}

		// ============ METHODS ============

		[DllImport(DLL_NAME)]
		public static extern void setWeatherKey(string path);
		[DllImport(DLL_NAME)]
		public static extern void setElevationKey(string path);

		[DllImport(DLL_NAME)]
		public static extern short getBrakingDistance(double lat, double lon, ref double out_distance, ref double out_time);

		[DllImport(DLL_NAME)]
		public static extern void setWeather(WeatherID weather_id);
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
