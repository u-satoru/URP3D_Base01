#!/bin/bash

echo "Fixing remaining compilation errors..."

# Fix Core.Services to Core.Services.Interfaces
echo "Fixing Services namespace references..."
for file in "Assets/_Project/Core/Camera/CameraService.cs" \
            "Assets/_Project/Core/Character/CharacterManager.cs" \
            "Assets/_Project/Core/Patterns/StateHandlerRegistry.cs"; do
    if [ -f "$file" ]; then
        sed -i 's/using asterivo\.Unity60\.Core\.Services;/using asterivo.Unity60.Core.Services.Interfaces;/g' "$file"
        echo "  Fixed: $file"
    fi
done

# Add Core.Data namespace to audio files
echo "Adding Core.Data to audio files..."
for file in "Assets/_Project/Core/Audio/StealthAudioCoordinator.cs" \
            "Assets/_Project/Core/Audio/Services/StealthAudioService.cs"; do
    if [ -f "$file" ]; then
        # Add Core.Data if not present
        if ! grep -q "using asterivo.Unity60.Core.Data;" "$file"; then
            sed -i '/namespace asterivo\.Unity60/i using asterivo.Unity60.Core.Data;\n' "$file"
            echo "  Fixed: $file"
        fi
    fi
done

echo "All fixes applied!"