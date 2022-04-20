cmake_minimum_required (VERSION 3.8) # C# support

set(CMAKE_CXX_STANDARD 17)
set(VCPKG_ROOT_)

project("Brakenator" VERSION "0.1.0" LANGUAGES CXX)

include("${CMAKE_SOURCE_DIR}/vcpkg/scripts/buildsystems/vcpkg.cmake")

# place all predefined target projects into a filter
set_property(GLOBAL PROPERTY USE_FOLDERS ON)
set(PREDEFINED_TARGETS_FOLDER "Targets")

option(BN_BUILD_UI "Generate a C# project that functions as a user interface for the Brakenator" ON)
option(BN_BUILD_TEST "Generate a C++ project that tests the functionality of the Brakenator" OFF)
option(BN_BUILD_SHARED "Builds a shared library (dll) instead of a static library (lib)" ON)

set (BN_SRC
	src/Brakenator.cpp
)

set (BN_INCLUDE
	include/Brakenator.h
)


# Add source to this project's executable.
if(${BN_BUILD_SHARED})
add_library (${PROJECT_NAME} SHARED
	${BN_INCLUDE} ${BN_SRC}
)
else()
add_library (${PROJECT_NAME}
	${BN_INCLUDE} ${BN_SRC}
)
endif()

if(MSVC)
  target_compile_options(${PROJECT_NAME}  PUBLIC "/ZI")
  target_link_options(${PROJECT_NAME}  PUBLIC "/INCREMENTAL")
endif()

target_include_directories(${PROJECT_NAME} PUBLIC ${CMAKE_SOURCE_DIR} "include/" "ipp_src/")
target_compile_definitions(${PROJECT_NAME} PRIVATE BN_EXPORTS $<$<BOOL:${BN_BUILD_SHARED}>:BN_DLL>)

# place files into the correct filters
source_group("src" FILES ${SRC})
source_group("include" FILES ${INCLUDE})

# libcurl

find_package(CURL CONFIG REQUIRED)
target_link_libraries(${PROJECT_NAME} PRIVATE CURL::libcurl)
target_include_directories(${PROJECT_NAME} PRIVATE ${CURL_INCLUDE_DIRECTORIES})

# rapidjson

find_package(RapidJSON CONFIG REQUIRED)
target_link_libraries(${PROJECT_NAME} PRIVATE rapidjson)
target_include_directories(${PROJECT_NAME} PRIVATE ${RapidJSON_INCLUDE_DIRECTORIES})

# UI

if(${BN_BUILD_UI})
add_subdirectory("BrakenatorUI")
endif()

# Test project

if(${BN_BUILD_TEST})
add_subdirectory("BrakenatorTEST" BNTEST)
endif()
