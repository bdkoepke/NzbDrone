#!/bin/sh
yes | certmgr -ssl -m https://go.microsoft.com
yes | certmgr -ssl -m https://nugetgallery.blob.core.windows.net
yes | certmgr -ssl -m https://myget.org
yes | certmgr -ssl -m https://nuget.org
mozroots --import --sync --quiet
