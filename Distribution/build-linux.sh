#!/bin/bash
cd ..

APPDIR=$"FRESHMusicPlayer.AppDir"

rm -rf $APPDIR || true


pushd FRESHMusicPlayer/FRESHMusicPlayer.Linux
dotnet publish FRESHMusicPlayer.Linux.csproj -c Release -r linux-x64
popd

mkdir -p $APPDIR
cp -r "FRESHMusicPlayer/FRESHMusicPlayer.Linux/bin/Release/net10.0/linux-x64/publish/"* $APPDIR
cp Distribution/io.github.royce551.FRESHMusicPlayer.desktop $APPDIR
cp Distribution/logo.svg $APPDIR/io.github.royce551.FRESHMusicPlayer.svg

ln -s FRESHMusicPlayer $APPDIR/AppRun
