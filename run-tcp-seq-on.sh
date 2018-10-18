#!/bin/bash
#kind of like a call back
#$2: test duration, $3: report base,  writeback addr=myIp
#$1: target machine, $4: full file name
myIP="$(ifconfig | sed -En 's/127.0.0.1//;s/.*inet (addr:)?(([0-9]*\.){3}[0-9]*).*/\2/p')"
ssh -tt $1 -o StrictHostKeyChecking=no "cd ${PWD}; echo $PWD; sh run-tcp-seq-one-all.sh $2 $3 ${myIP}; exit;"
#i now have a $1 file in my directory.
#expected file name
echo "Runon finished on ${1}"
exit
