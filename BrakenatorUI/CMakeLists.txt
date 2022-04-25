project("BrakenatorUI" VERSION "0.1.0" LANGUAGES CSharp)

include(CSharpUtilities)

set(DESIGNER_SRC
	AssemblyInfo.cs
)

# put anything with XAML in its name here
set(XAML_SRC
	App.xaml
	App.xaml.cs
	
	MainWindow.xaml
	MainWindow.xaml.cs
	Page1.xaml
	Page1.xaml.cs
	Page2.xaml
	Page2.xaml.cs
)

add_executable(${PROJECT_NAME}
	${DESIGNER_SRC}
	${XAML_SRC}
	Brakenator.cs
)

csharp_set_designer_cs_properties(${DESIGNER_SRC})
csharp_set_xaml_cs_properties(${XAML_SRC})
set_property(SOURCE App.xaml PROPERTY VS_XAML_TYPE "ApplicationDefinition")
set_property(TARGET ${PROJECT_NAME} PROPERTY WIN32_EXECUTABLE TRUE)

set_property(TARGET ${PROJECT_NAME} PROPERTY VS_DOTNET_REFERENCES "Microsoft.CSharp" "PresentationCore" "PresentationFramework" 
 "System" "System.Core" "System.Data" "System.Data.DataSetExtensions" "System.Net.Http" "System.Xaml" "System.Xml" "System.Xml.Linq" "WindowsBase"
)

# depends on the Brakenator dll
add_dependencies(${PROJECT_NAME} Brakenator)
target_link_libraries(${PROJECT_NAME} Brakenator)

# copy dll to binary dir
if(${BN_BUILD_SHARED})



add_custom_command(TARGET ${PROJECT_NAME} PRE_BUILD COMMAND ${CMAKE_COMMAND} -E copy_directory  "$<TARGET_FILE_DIR:Brakenator>" "$<TARGET_FILE_DIR:${PROJECT_NAME}>")

add_custom_command(TARGET ${PROJECT_NAME} PRE_BUILD COMMAND ${CMAKE_COMMAND} -E copy_directory "${CMAKE_CURRENT_SOURCE_DIR}/resources" "$<TARGET_FILE_DIR:${PROJECT_NAME}>")

endif()
