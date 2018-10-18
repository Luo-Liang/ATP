#!/bin/bash
#sh run-ping.sh ${myIP} $timeout $basename;
pgrep ping | xargs kill -9 > /dev/null 2>&1
myIP="$(ifconfig | sed -En 's/127.0.0.1//;s/.*inet (addr:)?(([0-9]*\.){3}[0-9]*).*/\2/p')"
echo "run-ping.sh currently running on ${myIP}"
reportFile="${PWD}/$3-${myIP}.txt"
#cat hosts | xargs -I{} ssh -o StrictHostKeyChecking=no {} "pgrep qperf | xargs kill -9 &> /dev/null; cd ~/qperf-0.4.9/src; ./qperf -t 10s -v ${myIP} udp_lat"
#rm $reportFile &> /dev/null
touch $reportFile
cat hosts | xargs -Iip sh -c "cd ${PWD}; bash ./ping-csv.sh ip $myIP ip $reportFile $2"
if [ $# -gt 0 ]
then
    #copy back.
    echo "Copying ${reportFile} to ubuntu@${1}:${reportFile}"
    scp $reportFile ubuntu@$1:$reportFile
    #rm $reportFile &> /dev/null
fi
echo "run-ping.sh finished running on ${myIP}"
exit
