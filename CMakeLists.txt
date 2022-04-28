cmake_minimum_required (VERSION 3.8) # C# support

set(CMAKE_CXX_STANDARD 17)
project("Brakenator" VERSION "0.1.0" LANGUAGES CXX)

# place all predefined target projects into a filter
set_property(GLOBAL PROPERTY USE_FOLDERS ON)
set(PREDEFINED_TARGETS_FOLDER "Targets")

option(BN_BUILD_UI "Generate a C# project that functions as a user interface for the Brakenator" ON)
option(BN_BUILD_TEST "Generate a C++ project that tests the functionality of the Brakenator" ON)
option(BN_BUILD_SHARED "Builds a shared library (dll) instead of a static library (lib)" ON)

# install packages
include("${CMAKE_BINARY_DIR}/conan/conan_paths.cmake" OPTIONAL)

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
  target_compile_options(${PROJECT_NAME} PRIVATE /W4 /external:anglebrackets /external:W0)
  target_compile_options(${PROJECT_NAME}  PUBLIC "/ZI")
  target_link_options(${PROJECT_NAME}  PUBLIC "/INCREMENTAL")
endif()

target_include_directories(${PROJECT_NAME} PUBLIC ${CMAKE_SOURCE_DIR} "include/" "ipp_src/")
target_compile_definitions(${PROJECT_NAME} PRIVATE BN_EXPORTS $<$<BOOL:${BN_BUILD_SHARED}>:BN_DLL> "_USE_MATH_DEFINES")

# place files into the correct filters
source_group("src" FILES ${SRC})
source_group("include" FILES ${INCLUDE})

# libcurl

find_package(CURL REQUIRED)
target_link_libraries(${PROJECT_NAME} PRIVATE CURL::libcurl)
target_include_directories(${PROJECT_NAME} PRIVATE ${CURL_INCLUDE_DIRS})

# rapidjson

find_package(RapidJSON REQUIRED)
target_include_directories(${PROJECT_NAME} PRIVATE ${RapidJSON_INCLUDE_DIRS})

# UI

if(${BN_BUILD_UI})
add_subdirectory("BrakenatorUI")
endif()

# Test project

if(${BN_BUILD_TEST})
add_subdirectory("BrakenatorTEST" BNTEST)
endif()
