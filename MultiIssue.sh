#!/bin/bash
procs=`cat hosts | wc -l`
#procs=4
cat hosts | xargs -P $procs -I____ip____ ssh -o StrictHostKeyChecking=no ____ip____ $1
