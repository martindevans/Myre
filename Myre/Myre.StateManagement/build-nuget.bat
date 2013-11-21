nuget pack Myre.StateManagement.nuspec -verbosity detailed

REM This looks for all nupkg files in the directory, regardless of version
REM There should only be one
for %%A in (*.nupkg) do (
    REM Push package
    nuget push %%A
    
    REM Delete package now we've used it
    DEL %%A
)