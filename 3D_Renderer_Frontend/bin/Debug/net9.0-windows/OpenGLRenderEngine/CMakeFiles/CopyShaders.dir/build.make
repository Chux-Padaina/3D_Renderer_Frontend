﻿# CMAKE generated file: DO NOT EDIT!
# Generated by "NMake Makefiles" Generator, CMake Version 3.30

# Delete rule output on recipe failure.
.DELETE_ON_ERROR:

#=============================================================================
# Special targets provided by cmake.

# Disable implicit rules so canonical targets will work.
.SUFFIXES:

.SUFFIXES: .hpux_make_needs_suffix_list

# Command-line flag to silence nested $(MAKE).
$(VERBOSE)MAKESILENT = -s

#Suppress display of executed commands.
$(VERBOSE).SILENT:

# A target that is always out of date.
cmake_force:
.PHONY : cmake_force

#=============================================================================
# Set environment variables for the build.

!IF "$(OS)" == "Windows_NT"
NULL=
!ELSE
NULL=nul
!ENDIF
SHELL = cmd.exe

# The CMake executable.
CMAKE_COMMAND = "C:\Program Files\JetBrains\CLion 2024.3.4\bin\cmake\win\x64\bin\cmake.exe"

# The command to remove a file.
RM = "C:\Program Files\JetBrains\CLion 2024.3.4\bin\cmake\win\x64\bin\cmake.exe" -E rm -f

# Escaping for special characters.
EQUALS = =

# The top-level source directory on which CMake was run.
CMAKE_SOURCE_DIR = C:\Users\Vin\CLionProjects\OpenGLRenderingEngineLibrary

# The top-level build directory on which CMake was run.
CMAKE_BINARY_DIR = C:\Users\Vin\CLionProjects\OpenGLRenderingEngineLibrary\cmake-build-debug

# Utility rule file for CopyShaders.

# Include any custom commands dependencies for this target.
include CMakeFiles\CopyShaders.dir\compiler_depend.make

# Include the progress variables for this target.
include CMakeFiles\CopyShaders.dir\progress.make

CMakeFiles\CopyShaders:
	@$(CMAKE_COMMAND) -E cmake_echo_color "--switch=$(COLOR)" --blue --bold --progress-dir=C:\Users\Vin\CLionProjects\OpenGLRenderingEngineLibrary\cmake-build-debug\CMakeFiles --progress-num=$(CMAKE_PROGRESS_1) "Refreshing and copying shader folder..."
	echo >nul && "C:\Program Files\JetBrains\CLion 2024.3.4\bin\cmake\win\x64\bin\cmake.exe" -E remove_directory C:/Users/Vin/CLionProjects/OpenGLRenderingEngineLibrary/cmake-build-debug/shaders
	echo >nul && "C:\Program Files\JetBrains\CLion 2024.3.4\bin\cmake\win\x64\bin\cmake.exe" -E copy_directory C:/Users/Vin/CLionProjects/OpenGLRenderingEngineLibrary/shaders C:/Users/Vin/CLionProjects/OpenGLRenderingEngineLibrary/cmake-build-debug/shaders

CopyShaders: CMakeFiles\CopyShaders
CopyShaders: CMakeFiles\CopyShaders.dir\build.make
.PHONY : CopyShaders

# Rule to build all files generated by this target.
CMakeFiles\CopyShaders.dir\build: CopyShaders
.PHONY : CMakeFiles\CopyShaders.dir\build

CMakeFiles\CopyShaders.dir\clean:
	$(CMAKE_COMMAND) -P CMakeFiles\CopyShaders.dir\cmake_clean.cmake
.PHONY : CMakeFiles\CopyShaders.dir\clean

CMakeFiles\CopyShaders.dir\depend:
	$(CMAKE_COMMAND) -E cmake_depends "NMake Makefiles" C:\Users\Vin\CLionProjects\OpenGLRenderingEngineLibrary C:\Users\Vin\CLionProjects\OpenGLRenderingEngineLibrary C:\Users\Vin\CLionProjects\OpenGLRenderingEngineLibrary\cmake-build-debug C:\Users\Vin\CLionProjects\OpenGLRenderingEngineLibrary\cmake-build-debug C:\Users\Vin\CLionProjects\OpenGLRenderingEngineLibrary\cmake-build-debug\CMakeFiles\CopyShaders.dir\DependInfo.cmake "--color=$(COLOR)"
.PHONY : CMakeFiles\CopyShaders.dir\depend

