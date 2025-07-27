# Advanced Reminders Makefile
# Build commands for the RimWorld mod

# Default target
.DEFAULT_GOAL := build

# Variables
PROJECT_NAME = AdvancedReminders
SOURCE_DIR = Source/$(PROJECT_NAME)
OUTPUT_DIR = Assemblies
SOLUTION_FILE = $(PROJECT_NAME).sln
RIMWORLD_MODS_DIR = /c/Program\ Files\ \(x86\)/Steam/steamapps/common/RimWorld/Mods

# Build and deploy the mod to RimWorld
build:
	@echo "Building and deploying $(PROJECT_NAME)..."
	dotnet build "$(SOURCE_DIR)/$(PROJECT_NAME).csproj" --configuration Release
	@echo "Build and deployment complete!"
	@echo "Mod available in RimWorld at: $(RIMWORLD_MODS_DIR)/$(PROJECT_NAME)"

# Clean build artifacts and deployed mod
clean:
	@echo "Cleaning build artifacts and deployed mod..."
	dotnet clean "$(SOURCE_DIR)/$(PROJECT_NAME).csproj"
	@echo "Clean complete!"

# Debug build and deploy
debug:
	@echo "Building $(PROJECT_NAME) in debug mode..."
	dotnet build "$(SOURCE_DIR)/$(PROJECT_NAME).csproj" --configuration Debug
	@echo "Debug build and deployment complete!"

# Build without deployment (local only)
build-local:
	@echo "Building $(PROJECT_NAME) locally only..."
	dotnet build "$(SOURCE_DIR)/$(PROJECT_NAME).csproj" --configuration Release --property:DeployToRimWorld=false
	@echo "Local build complete!"

# Restore NuGet packages
restore:
	@echo "Restoring packages..."
	dotnet restore "$(SOURCE_DIR)/$(PROJECT_NAME).csproj"
	@echo "Restore complete!"

# Build solution
solution:
	@echo "Building solution..."
	dotnet build "$(SOLUTION_FILE)" --configuration Release
	@echo "Solution build complete!"

# Check if RimWorld installation exists
check-rimworld:
	@if [ -d $(RIMWORLD_MODS_DIR) ]; then \
		echo "RimWorld installation found at: $(RIMWORLD_MODS_DIR)"; \
	else \
		echo "WARNING: RimWorld installation not found at expected path"; \
		echo "Expected: $(RIMWORLD_MODS_DIR)"; \
	fi

# Force redeploy (rebuild and copy everything)
redeploy: clean build

# Help
help:
	@echo "Available targets:"
	@echo "  build        - Build and deploy the mod to RimWorld (default)"
	@echo "  debug        - Build and deploy in debug mode"
	@echo "  build-local  - Build locally without deploying"
	@echo "  clean        - Clean build artifacts and deployed mod"
	@echo "  restore      - Restore NuGet packages"
	@echo "  solution     - Build entire solution"
	@echo "  redeploy     - Clean, build, and redeploy"
	@echo "  check-rimworld - Check RimWorld installation path"
	@echo "  help         - Show this help"
	@echo ""
	@echo "The mod will be automatically deployed to:"
	@echo "  $(RIMWORLD_MODS_DIR)/$(PROJECT_NAME)"

# Declare phony targets
.PHONY: build clean debug build-local restore solution check-rimworld redeploy help