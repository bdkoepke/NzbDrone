#!/bin/sh
set -e
nzbdrone_deploy() {
        /usr/local/etc/rc.d/sonarr stop || true
	rm -rf /usr/local/sonarr
        cp -a _output /usr/local/sonarr
        mv /usr/local/sonarr/NzbDrone.Console.exe /usr/local/sonarr/Sonarr.exe
        /usr/local/etc/rc.d/sonarr start
}
nzbdrone_deploy
