#!/bin/bash

set -e

cd ..

APP_BUNDLE=Distribution/FRESHMusicPlayer.app
RESOURCES_BUNDLE=$APP_BUNDLE/Contents/Resources

rm -rf $APP_BUNDLE || true

pushd FRESHMusicPlayer/FRESHMusicPlayer-Avalonia
dotnet publish -r osx-$BUILD_ARCH --self-contained
popd

mkdir -p $APP_BUNDLE/Contents/MacOS
cp -r "FRESHMusicPlayer/FRESHMusicPlayer-Avalonia/bin/Debug/net"*"/osx-$BUILD_ARCH/"* $APP_BUNDLE/Contents/MacOS
mkdir -p $RESOURCES_BUNDLE
cp Distribution/icon.icns $RESOURCES_BUNDLE
cp Distribution/Info.plist $APP_BUNDLE/Contents/

codesign --deep --force -s - $APP_BUNDLE || true
