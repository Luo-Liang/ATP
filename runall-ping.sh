#!/bin/bash
#$1 full file name
rm $1 &> /dev/null
touch $1
totalClients=`cat hosts | sed '/^\s*$/d' | wc -l`
echo "BENCHMARK:PING;SELF_TEST_OPTION:TRUE;DIMENSION:${totalClients};VALUE:MIN;PREPROCESS:0" > $1
baseName="ping"
cat hosts | xargs -Iip sh run-ping-on.sh ip 1 $baseName

expectedFileName="${baseName}-*.txt"
cat $expectedFileName >> $1
#i want to append it to a global raw file.
#rm $expectedFileName
#rm *.txt
