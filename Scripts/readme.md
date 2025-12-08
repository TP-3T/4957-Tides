# Scripts

Not Unity ones btw.

## Use the random mesh generator thing

This is how you add the script 

```powershell
# Assuming that you are in the root directory
. ./Scripts/genMap.ps1
. .\Scripts\genMap.ps1
```

This is the syntax (powershell)

```powershell
> GenerateMaps `
> @( `
> @{ MapName = "Epic Example"; Width = 100; Height = 100; Variance = 100 }, `
> @{ MapName = "Anohter"; Width = 50; Height = 50; Variance = 20 } `
> );
```
