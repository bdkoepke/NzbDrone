#!/bin/sh
set -e

nzbdrone_xbuild() {
	xbuild src/NzbDrone.sln /p:Configuration=Mono /p:Platform=x86 /t:Build 
	cp src/External/System.Data.SQLite.dll _output/
	cp src/External/MediaInfoDotNet.dll.config _output/
}
nzbdrone_gulp() {
	gulp build
	cp src/External/logs.css _output/UI/Content
}
cleanup_mdb() {
	rm _output/*.mdb
	rm _output/NzbDrone.Update/*.mdb
}
nzbdrone_chmod() {
	chmod -x _output/*.dll
	chmod -x _output/NzbDrone.Update/*.dll
}
nzbdrone_xbuild && nzbdrone_gulp && cleanup_mdb && nzbdrone_chmod
