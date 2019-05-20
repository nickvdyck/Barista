.PHONY: purge clean test
.DEFAULT_GOAL := build

MACOSSLN	:= Barista.MacOS.sln
CORE		:= src/Barista.Core
MACOS		:= src/Barista.MacOS
BUILD		:= .build
TEST_CORE	:= test/Barista.Core.Tests

INFOPLIST	:= $(MACOS)/Info.plist
VERSION		= $(shell /usr/libexec/PlistBuddy -c "Print :CFBundleShortVersionString" $(INFOPLIST))

purge: clean
	rm -rf .vs
	rm -rf $(BUILD)

clean:
	rm -rf $(CORE)/obj
	rm -rf $(MACOS)/obj
	rm -rf $(TEST_CORE)/obj
	msbuild $(MACOSSLN) /t:Clean

restore:
	nuget restore $(MACOSSLN)

build: restore
	msbuild $(MACOSSLN) /restore:True /p:Configuration=Release

test:
	dotnet test $(TEST_CORE)

install:
	@open ./.build/bin/Barista.MacOS/Release/Barista-$(VERSION).pkg

uninstall:
	sudo rm -rf /Applications/Barista.app
	sudo pkgutil --forget codes.nvd.Barista
