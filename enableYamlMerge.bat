@echo off

cd %~dp0

set util = UnityYAMLMerge.exe

if not defined YAMLPath (
    :PathCheck
        set /p YAMLPath="Enter the path to UnityYamlMerge.exe (e.g. C:\Program Files\Unity\Hub\Editor\6000.0.60f1\Editor\Data\Tools):"
        if not exist "%YAMLPath%\%util%" (
            echo "Could not find %YAMLPath%\%util%. Please try again."
            goto :PathCheck
        )
)

echo found UnityYamlMerge.exe at %YAMLPath%\%util%
echo Setting up git to use %util% as merge tool...

git config --local merge.unityyamlmerge.driver "'%YAMLPath%UnityYamlMerge.exe' merge --fallback none -h -p --force %O %B %A %A"
git config --local merge.unityyamlmerge.name "Unity SmartMerge (UnityYamlMerge)"
git config --local merge.unityyamlmerge.recursive binary

echo Completed setting UnityYamlMerge
pause