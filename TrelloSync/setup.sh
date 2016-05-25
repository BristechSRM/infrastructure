if [ ! -d ".nuget" ]; then  
    echo "Creating .nuget directory"
    mkdir .nuget
else
    echo "Nuget directory found"
fi

if [ ! -f ".nuget/nuget.exe" ]; then
    echo "Fetching latest nuget executable"
    cd .nuget
    wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
    cd ..
else
    echo "Nuget executable found"
fi

if [ ! -f "packages/FAKE/tools/FAKE.exe" ]; then
    echo "Installing FAKE package"
    mono .nuget/nuget.exe install "FAKE" -OutputDirectory packages/ -ExcludeVersion
else
    echo "Fake Package found"
fi

