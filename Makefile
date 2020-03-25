.PHONY: purge clean test
.DEFAULT_GOAL := default

MACOSSLN		:= Barista.sln
MACOS			:= src/Barista
SCHEDULER		:= src/Barista.Scheduler
BUILD			:= .build
TEST_CORE		:= test/Barista.Core.Tests
CONFIGURATION	:= Debug

INFOPLIST		:= $(MACOS)/Info.plist
VERSION			= $(shell /usr/libexec/PlistBuddy -c "Print :CFBundleShortVersionString" $(INFOPLIST))

purge: clean
	rm -rf .vs
	rm -rf $(BUILD)

clean:
	rm -rf $(SCHEDULER)/obj
	rm -rf $(MACOS)/obj
	msbuild $(MACOSSLN) /t:Clean

restore:
	nuget restore $(MACOSSLN)

default:
	$(MAKE) build CONFIGURATION=Release

build: restore
	msbuild $(MACOSSLN) /restore:True /p:Configuration=$(CONFIGURATION)

test:
	dotnet test $(TEST_CORE)

install:
	@open ./.build/bin/Barista/Release/Barista-$(VERSION).pkg

uninstall:
	sudo rm -rf /Applications/Barista.app
	sudo pkgutil --forget codes.nvd.Barista
