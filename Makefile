.PHONY: purge clean
.DEFAULT_GOAL := default

MACOSSLN	:= Barista.MacOS.sln
CORE		:= src/Barista.Core
MACOS		:= src/Barista.MacOS
BUILD		:= .build

purge: clean
	rm -rf .vs
	rm -rf $(BUILD)
	rm -rf $(CORE)/obj
	rm -rf $(MACOS)/obj

clean:
	msbuild $(MACOSSLN) /t:Clean

build:
	msbuild $(MACOSSLN) /restore:True

default:
	msbuild $(MACOSSLN) /restore:True /p:Configuration=Release
