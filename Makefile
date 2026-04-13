.PHONY: install uninstall link help test

PREFIX ?= /usr/local

help: ## Show this help
	@echo "Ralph Wiggum Loop — Makefile"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-12s\033[0m %s\n", $$1, $$2}'
	@echo ""

install: ## Install rwl to PREFIX/bin (default: /usr/local/bin)
	@bash install.sh --prefix $(PREFIX)

link: ## Symlink rwl to PREFIX/bin (for development)
	@echo "Linking rwl to $(PREFIX)/bin/rwl..."
	@ln -sf "$(CURDIR)/bin/rwl" "$(PREFIX)/bin/rwl" 2>/dev/null || \
		sudo ln -sf "$(CURDIR)/bin/rwl" "$(PREFIX)/bin/rwl"
	@echo "✅ Symlinked. Changes to bin/rwl are immediately available."
	@echo ""
	@echo "Set RWL_HOME for component resolution:"
	@echo "  export RWL_HOME=$(CURDIR)"

uninstall: ## Remove rwl from PREFIX/bin
	@echo "Removing $(PREFIX)/bin/rwl..."
	@rm -f "$(PREFIX)/bin/rwl" 2>/dev/null || sudo rm -f "$(PREFIX)/bin/rwl"
	@echo "✅ Removed."

test: ## Run basic CLI smoke tests
	@echo "Running smoke tests..."
	@bash bin/rwl version
	@bash bin/rwl help > /dev/null
	@echo "✅ All smoke tests passed."
