#!/bin/bash
#$1 full file name
rm *.raw
rm $1 &> /dev/null
touch $1
totalClients=`cat hosts | sed '/^\s*$/d' | wc -l`
echo "BENCHMARK:TCP-ALL-ALL;SELF_TEST_OPTION:FALSE;DIMENSION:${totalClients};VALUE:AVG;PREPROCESS:10" > $1
baseName="tcp-all-seq"
cat hosts | xargs -Iip sh run-tcp-seq-on.sh ip 10 $baseName

expectedFileName="${baseName}-*.txt"
cat $expectedFileName >> $1
#i want to append it to a global raw file.
#rm $expectedFileName
#rm *.raw
