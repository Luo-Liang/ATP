#!/bin/bash
#
# Do a ping and output results as CSV.
#
# dsimmons@squiz.co.uk
# 2011-12-23
#
# adapted: https://gist.githubusercontent.com/dansimau/1513880/raw/94765d6f46157ae6b95c6e994981aef0bfa4fe25/ping-csv.sh

if [ $# -lt 4 ]; then
	echo "Usage: $0 [--add-timestamp] <ping host> fromip toip location"
	exit 99
fi

trap echo 0

rm $4 &> /dev/null

sudo ping $1 -w 1 -i 0.001 | while read line; do

	# Skip header
	[[ "$line" =~ ^PING ]] && continue

	# Skip non-positive responses
	[[ ! "$line" =~ "bytes from" ]] && continue

	# Extract address field
	addr=$2,$3

	# Extract seq
	#seq=${line##*icmp_seq=}
	#seq=${seq%% *}

	# Extract time
	time=${line##*time=}
	time=${time%% *}
	time=$(echo $time*1000 | bc)

	echo "$addr,time=$time" >> $4
done
