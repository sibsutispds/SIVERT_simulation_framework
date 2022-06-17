#!/usr/bin/env zsh
#kill $(lsof -i tcp:8001 | awk 'NR==2' | awk '{print $2}') # kill socket binding if exists
cd $1
./$2
#./waf --run $2
