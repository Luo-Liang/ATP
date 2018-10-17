#!/bin/bash
#test duration, report base, writeback addr
#     $1            $2           $3
pgrep iperf | xargs kill -9
rm *.txt *.raw 2> /dev/null
totalClients=`cat hosts | sed '/^\s*$/d' | wc -l`
totalClients=$((totalClients - 1))
myIP="$(ifconfig | sed -En 's/127.0.0.1//;s/.*inet (addr:)?(([0-9]*\.){3}[0-9]*).*/\2/p')"
echo "run-tcp-seq-one-all.sh currently running on ${myIP}"
#run multiple servers
sh iperf-parallel-servers.sh $totalClients $2 $1
#echo "STARTING ${myIP}" > $myIP
idx=0
while IFS='' read -r line || [ -n "$line" ]; do
    #echo "Text read from file: $line"
    if [ "$line" = "$myIP" ]
    then
	continue
    fi
    #grab one remote and fire a connection    
    idx=$((idx + 1))
    #create a client on a remote machine please
    #my port:
    currPort=$((5000+$idx))
    #recover data from the server.
    echo "launching on " $line " with command iperf -c ${myIP} -p ${currPort} -t $1 &  sleep $1; pkill iperf  "
    ssh $line -o StrictHostKeyChecking=no "iperf -c ${myIP} -p ${currPort} -t $1 &  sleep $1; pkill -9 iperf " < /dev/null
    echo $line " finsihed executing"
done < "hosts"
echo "${myIP} is waiting for a few seconds to shutdown server"
sleep 1
echo "${myIP} is finalizing data collection..."
#echo "ENDING ${myIP}" >> $myIP

report_file=$2-${myIP}.txt
count="$(find . -maxdepth 1 -name "$2-*.raw" | wc -l)"
if [ $count -ne $totalClients ]
then
    echo "${myIP} received only ${count} out of ${totalClients} reports"
    exit 1
fi
touch $report_file
cat $2-*.raw > $report_file
#now write back
echo "Copying ${report_file} to ubuntu@$3:$PWD/${report_file}"
scp $report_file ubuntu@$3:${PWD}/${report_file}
echo "${myIP} is terminating all iperf instances..."
pgrep iperf | xargs kill -9
echo "run-tcp-seq-one-all.sh finished running on ${myIP}"
