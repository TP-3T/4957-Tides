@echo off

if not defined YAMLPath (
    :PathCheck
        set /p YAMLPath=Enter the path to UnityYamlMerge.exe (e.g. C:\Program Files\Unity\Hub\Editor\6000.0.33f1\Editor\Data\Tools\)  
        if not exist "%YAMLPath%\UnityYamlMerge.exe" (
            echo "Could not find %YAMLPath%\UnityYamlMerge.exe. Please try again."
            goto :PathCheck
        )
)

echo found UnityYamlMerge.exe at %YAMLPath%\UnityYamlMerge.exe
echo Setting up git to use UnityYamlMerge.exe as merge tool...

git config --local merge.unityyamlmerge.driver "'%YAMLPath%UnityYamlMerge.exe' merge --fallback none -h -p --force %O %B %A %A"
git config --local merge.unityyamlmerge.name "Unity SmartMerge (UnityYamlMerge)"
git config --local merge.unityyamlmerge.recursive binary

echo Completed setting UnityYamlMerge
pause