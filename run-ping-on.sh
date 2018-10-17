#!/bin/bash
#kind of like a call back
#$1 run on what
#$2 timeout
#$3 basename
myIP="$(ifconfig | sed -En 's/127.0.0.1//;s/.*inet (addr:)?(([0-9]*\.){3}[0-9]*).*/\2/p')"
ssh -tt $1 -o StrictHostKeyChecking=no "cd ${PWD}; sh run-ping.sh ${myIP} $2 $3; exit;"
echo "Runon finished on ${1}"
exit
