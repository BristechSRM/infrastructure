FROM ubuntu:14.04

RUN apt-key adv --keyserver keyserver.ubuntu.com --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
    echo "deb http://download.mono-project.com/repo/debian wheezy main" | sudo tee /etc/apt/sources.list.d/mono-xamarin.list && \
    apt-get update && \
    apt-get -y install mono-devel fsharp wget

RUN mkdir -p code/.nuget && cd code/.nuget && wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe && cd .. && \
    mono .nuget/nuget.exe install "FAKE" -OutputDirectory packages/ -ExcludeVersion
