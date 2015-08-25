function Push-Nuget($path, $csproj) {
    $fullPathToCsprog = Join-Path -Path $path -ChildPath $csproj -resolve;
    
    nuget pack $fullPathToCsprog -Prop Configuration=Release -IncludeReferencedProjects
    
    get-childitem -Filter *.nupkg -name | foreach ($_) {
        Write-Host "Pushing " $_ -backgroundcolor darkgreen -foregroundcolor white;
    
        nuget push $_
        Remove-Item $_
        
        Write-Host "Done " $_ -backgroundcolor darkgreen -foregroundcolor white;
    }
}

#Root of all projects
Push-Nuget "Myre/Myre" "Myre.csproj"

# Dependent upon Myre
Push-Nuget "Myre/Myre.Debugging" "Myre.Debugging.csproj"
Push-Nuget "Myre/Myre.UI" "Myre.UI.csproj"
Push-Nuget "Myre/Myre.Entities" "Myre.Entities.csproj"
Push-Nuget "Myre/Myre.StateManagement" "Myre.StateManagement.csproj"

# Dependent upon Myre.Debugging and Myre.UI
Push-Nuget "Myre/Myre.Debugging.UI" "Myre.Debugging.UI.csproj"

# Dependent upon Myre.Debugging and Myre.Entities
Push-Nuget "Myre/Myre.Graphics" "Myre.Graphics.csproj"

# Dependent upon Myre.Graphics
# Note no (x86) for pipeline. This is a pipeline project not a deploy project so target platform doesn't matter
Push-Nuget "Myre/Myre.Graphics.Pipeline" "Myre.Graphics.Pipeline.csproj"