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
Push-Nuget "Myre/Myre" "(x86) Myre.csproj"

# Dependent upon Myre
Push-Nuget "Myre/Myre.Debugging" "(x86) Myre.Debugging.csproj"
Push-Nuget "Myre/Myre.UI" "(x86) Myre.UI.csproj"
Push-Nuget "Myre/Myre.Entities" "(x86) Myre.Entities.csproj"
Push-Nuget "Myre/Myre.StateManagement" "(x86) Myre.StateManagement.csproj"

# Dependent upon Myre.Debugging and Myre.UI
Push-Nuget "Myre/Myre.Debugging.UI" "(x86) Myre.Debugging.UI.csproj"

# Dependent upon Myre.Debugging and Myre.Entities
Push-Nuget "Myre/Myre.Graphics" "(x86) Myre.Graphics.csproj"

# Dependent upon Myre.Graphics
# Note no (x86) for pipeline. This is a pipeline project not a deploy project so target platform doesn't matter
Push-Nuget "Myre/Myre.Graphics.Pipeline" "Myre.Graphics.Pipeline.csproj"