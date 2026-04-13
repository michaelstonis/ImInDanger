.PHONY: build publish install uninstall link help test clean

PREFIX ?= /usr/local
RID ?= $(shell dotnet --info 2>/dev/null | grep 'RID:' | awk '{print $$2}' || echo "osx-arm64")

help: ## Show this help
	@echo "Ralph Wiggum Loop — Makefile"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-12s\033[0m %s\n", $$1, $$2}'
	@echo ""

build: ## Build the .NET CLI (debug)
	@cd src/Rwl && dotnet build -q

publish: ## Publish single-file binary for current platform
	@echo "Publishing for $(RID)..."
	@cd src/Rwl && dotnet publish -c Release -r $(RID) -q
	@echo "✅ Binary: src/Rwl/bin/Release/net10.0/$(RID)/publish/rwl"

install: publish ## Install rwl binary to PREFIX/bin
	@echo "Installing to $(PREFIX)/bin/rwl..."
	@cp "src/Rwl/bin/Release/net10.0/$(RID)/publish/rwl" "$(PREFIX)/bin/rwl" 2>/dev/null || \
		sudo cp "src/Rwl/bin/Release/net10.0/$(RID)/publish/rwl" "$(PREFIX)/bin/rwl"
	@echo "✅ Installed. Run: rwl --help"

link: build ## Symlink debug build to PREFIX/bin (for development)
	@echo "Linking rwl to $(PREFIX)/bin/rwl..."
	@ln -sf "$(CURDIR)/src/Rwl/bin/Debug/net10.0/$(RID)/rwl" "$(PREFIX)/bin/rwl" 2>/dev/null || \
		sudo ln -sf "$(CURDIR)/src/Rwl/bin/Debug/net10.0/$(RID)/rwl" "$(PREFIX)/bin/rwl"
	@echo "✅ Symlinked for development."
	@echo ""
	@echo "Set RWL_HOME for component resolution:"
	@echo "  export RWL_HOME=$(CURDIR)"

uninstall: ## Remove rwl from PREFIX/bin
	@echo "Removing $(PREFIX)/bin/rwl..."
	@rm -f "$(PREFIX)/bin/rwl" 2>/dev/null || sudo rm -f "$(PREFIX)/bin/rwl"
	@echo "✅ Removed."

test: build ## Run basic CLI smoke tests
	@echo "Running smoke tests..."
	@cd src/Rwl && dotnet run -- --help > /dev/null
	@cd src/Rwl && dotnet run -- --version
	@echo "✅ All smoke tests passed."

clean: ## Clean build artifacts
	@cd src/Rwl && dotnet clean -q
	@echo "✅ Cleaned."
