#!/bin/sh
set -e

nzbdrone_xbuild() {
	xbuild src/NzbDrone.sln /p:Configuration=Mono /p:Platform=x86 /t:Build 
	for dll in System.Data.SQLite.dll MediaInfoDotNet.dll.config Microsoft.AspNet.SignalR.Core.dll Microsoft.AspNet.SignalR..Owin.dll NzbDrone.Core.dll
	do
		cp src/External/${dll} _output/
	done
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
nzbdrone_xbuild && nzbdrone_gulp && nzbdrone_chmod
