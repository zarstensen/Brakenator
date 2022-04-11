PROJECT("BNTEST" LANGUAGES CXX)

add_executable(${PROJECT_NAME} "${CMAKE_CURRENT_SRC_DIR}src/main.cpp")

target_link_libraries(${PROJECT_NAME} Brakenator)
target_include_directories(${PROJECT_NAME} PRIVATE "${CMAKE_SRC_DIR}/include")

# copy dll to binary dir
if(${BN_BUILD_SHARED})
add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD COMMAND ${CMAKE_COMMAND} -E copy "$<TARGET_FILE:Brakenator>" "$<TARGET_FILE_DIR:${PROJECT_NAME}>")
endif()
