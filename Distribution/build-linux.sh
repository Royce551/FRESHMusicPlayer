#!/bin/bash
cd ..

APPDIR=$"FRESHMusicPlayer.AppDir"

rm -rf $APPDIR || true


pushd FRESHMusicPlayer/FRESHMusicPlayer-Avalonia
dotnet publish FRESHMusicPlayer-Avalonia.csproj -c Release -r linux-x64
popd

mkdir -p $APPDIR
cp -r "FRESHMusicPlayer/FRESHMusicPlayer-Avalonia/bin/Release/net5.0/linux-x64/publish/"* $APPDIR
cp Distribution/io.github.royce551.FRESHMusicPlayer.desktop $APPDIR
cp Distribution/logo.svg $APPDIR/io.github.royce551.FRESHMusicPlayer.svg

ln -s FRESHMusicPlayer $APPDIR/AppRun
