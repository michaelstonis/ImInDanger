.PHONY: build publish publish-all pack install install-tool uninstall link sync-resources help test clean

PREFIX ?= /usr/local
RID ?= $(shell dotnet --info 2>/dev/null | grep 'RID:' | awk '{print $$2}' || echo "osx-arm64")

# All supported target platforms for cross-platform release builds
RELEASE_RIDS = linux-x64 linux-arm64 osx-arm64 osx-x64 win-x64

help: ## Show this help
	@echo "Ralph Wiggum Loop — Makefile"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-16s\033[0m %s\n", $$1, $$2}'
	@echo ""

build: ## Build the .NET CLI (debug)
	@cd src/Rwl && dotnet build -q

sync-resources: ## Sync .github assets into embedded resources (done automatically by dotnet build)
	@echo "Syncing .github assets into src/Rwl/Resources/Templates/..."
	@mkdir -p src/Rwl/Resources/Templates/agents
	@mkdir -p src/Rwl/Resources/Templates/skills
	@mkdir -p src/Rwl/Resources/Templates/instructions
	@cp .github/agents/*.agent.md src/Rwl/Resources/Templates/agents/ 2>/dev/null || true
	@for skill in loop-runner task-state-manager convergence-detector loop-guardrails; do \
	  mkdir -p src/Rwl/Resources/Templates/skills/$$skill; \
	  cp -r .github/skills/$$skill/. src/Rwl/Resources/Templates/skills/$$skill/; \
	done
	@cp .github/instructions/*.instructions.md src/Rwl/Resources/Templates/instructions/ 2>/dev/null || true
	@cp .github/copilot-instructions.md src/Rwl/Resources/Templates/ 2>/dev/null || true
	@cp AGENTS.md src/Rwl/Resources/Templates/ 2>/dev/null || true
	@echo "✅ Resources synced."

publish: ## Publish self-contained single-file binary for current platform
	@echo "Publishing for $(RID)..."
	@cd src/Rwl && dotnet publish -c Release -r $(RID) --self-contained -q
	@echo "✅ Binary: src/Rwl/bin/Release/net10.0/$(RID)/publish/rwl"

publish-all: ## Build self-contained native binaries for all platforms (requires cross-compilation support)
	@echo "Building for all platforms: $(RELEASE_RIDS)"
	@for rid in $(RELEASE_RIDS); do \
	  echo "  Publishing $$rid..."; \
	  (cd src/Rwl && dotnet publish -c Release -r $$rid --self-contained -q); \
	  echo "  ✅ $$rid done"; \
	done
	@echo ""
	@echo "✅ All platform binaries built in src/Rwl/bin/Release/net10.0/*/publish/"

pack: ## Create NuGet tool package (.nupkg)
	@cd src/Rwl && dotnet pack -c Release -q
	@echo "✅ Package: $$(ls src/Rwl/bin/Release/*.nupkg 2>/dev/null || echo 'see src/Rwl/bin/Release/')"
	@echo ""
	@echo "Install locally:  dotnet tool install -g --add-source src/Rwl/bin/Release rwl"
	@echo "Push to NuGet:    dotnet nuget push src/Rwl/bin/Release/*.nupkg -s https://api.nuget.org/v3/index.json -k YOUR_API_KEY"

install: publish ## Install self-contained binary to PREFIX/bin
	@echo "Installing to $(PREFIX)/bin/rwl..."
	@cp "src/Rwl/bin/Release/net10.0/$(RID)/publish/rwl" "$(PREFIX)/bin/rwl" 2>/dev/null || \
		sudo cp "src/Rwl/bin/Release/net10.0/$(RID)/publish/rwl" "$(PREFIX)/bin/rwl"
	@echo "✅ Installed. Run: rwl --help"

install-tool: pack ## Install as global dotnet tool
	@dotnet tool install -g --add-source src/Rwl/bin/Release rwl || \
		dotnet tool update -g --add-source src/Rwl/bin/Release rwl
	@echo "✅ Installed as dotnet tool. Run: rwl --help"

link: build ## Symlink debug build to PREFIX/bin (for development)
	@echo "Linking rwl to $(PREFIX)/bin/rwl..."
	@ln -sf "$(CURDIR)/src/Rwl/bin/Debug/net10.0/$(RID)/rwl" "$(PREFIX)/bin/rwl" 2>/dev/null || \
		sudo ln -sf "$(CURDIR)/src/Rwl/bin/Debug/net10.0/$(RID)/rwl" "$(PREFIX)/bin/rwl"
	@echo "✅ Symlinked for development."
	@echo ""
	@echo "Set RWL_HOME for component resolution:"
	@echo "  export RWL_HOME=$(CURDIR)"

uninstall: ## Remove rwl (binary + dotnet tool)
	@echo "Removing $(PREFIX)/bin/rwl..."
	@rm -f "$(PREFIX)/bin/rwl" 2>/dev/null || sudo rm -f "$(PREFIX)/bin/rwl"
	@dotnet tool uninstall -g rwl 2>/dev/null || true
	@echo "✅ Removed."

test: build ## Run basic CLI smoke tests
	@echo "Running smoke tests..."
	@cd src/Rwl && dotnet run -- --help > /dev/null
	@cd src/Rwl && dotnet run -- --version
	@echo "✅ All smoke tests passed."

clean: ## Clean build artifacts
	@cd src/Rwl && dotnet clean -q
	@rm -f src/Rwl/bin/Release/*.nupkg
	@rm -rf src/Rwl/Resources/Templates/agents \
	         src/Rwl/Resources/Templates/skills \
	         src/Rwl/Resources/Templates/instructions \
	         src/Rwl/Resources/Templates/copilot-instructions.md \
	         src/Rwl/Resources/Templates/AGENTS.md
	@echo "✅ Cleaned."
