# ATP
Active Topology Probing

### Prerequisites
You need:
Python, pip `sudo apt-get install python, python-pip`
Dotnet `https://dot.net/v1/dotnet-install.sh`
matplotlib `sudo pip install matplotlib`
numpy `sudo pip install numpy`

### Setting up 
First, create a file named `hosts` that contains all the IPs of the machines you want to probe.
`touch hosts`

Remember to add one additional line to the `hosts` file, so some of the linux distros can count lines correctly.
One example of `hosts` file is:

````
192.168.1.2
192.168.1.3
192.168.1.4
<--- empty line  
````

Then, you need to clone the ATP repo on all machines. To facilitate this, once you have `hosts` file ready, you can use `MultiIssue.sh` to do so.
In the ATP folder, type `bash MultiIssue.sh "git clone https://github.com/luo-liang/ATP.git".

### Running tests
There are three different tests to run.

#### Pairwise sequential ping
Just run `bash runall-ping.sh <output_name>`. This does a sequential, pairwise ping of all machines. To change how long each probe runs, 
`emacs runall-ping.sh`, then change the line `cat hosts | xargs -Iip sh run-ping-on.sh ip 1 $baseName`, where `1` says run each pairwise probe for 1 sec. The frequency of ping can be changed in `ping-csv.sh`.

#### Pairwise iperf bandwidth
Just run `bash runall-tcp-seq.sh <output_name>`. This does a sequential, pairwise bandwidth probe of all machines. Similarly, change `ip 60` in 
`runall-tcp-seq.sh` to change to run longer test per probe.

#### Concurrent iperf bandwidth
Just run `bash runall-tcp.sh <output_name>`. This does a concurrent, pairwise bandwidth probe of all machines, so each machine will establish N
connections to other machines. Similarly, change `ip 60` in `runall-tcp.sh` to change to run longer test per probe.

#### Concurrent ping
Someone please do it. Should just be a minor tweak to sequential ping


### Analyzing the result
Once the <output> file is produced by each of the machines, use the `raw2table` folder for analysis. 
`cd raw2table; dotnet run <path_to_output_file> <path_to_hosts_file>`
`raw2table` will create a folder named <output_FLDR> that contains summary. It also produces an average/min/max summary matrix, depending on the metadata of 
each test, in the ATP folder, named <output>.proced.csv. To change how raw2table produces data, for exmaple, go to each `runall-*.sh` command
and change the metadata banner. An example is given for `runall-ping.sh`

`BENCHMARK:PING;SELF_TEST_OPTION:TRUE;DIMENSION:${totalClients};VALUE:MIN;PREPROCESS:0`
BENCHMARK:<name of benchmark>
SELF_TEST_OPTION: TRUE so that a self to self test is performed. FALSE to avoid self to self test.
DIMENSION: number of hosts involved
VALUE: MIN/AVG/MAX, the sumamry matrix should use the minimum/average/max reading of all collected data
PREPROCESS: percentage of data to drop from beginning and end of each test, due to warmup problems and tcp slow start.

You do not need to change these metadata directives.

### Generating plot
Use `plot` folder. 
Run `python plot.py ..`, and it will create time/frequency domain representation of each test you just probed. To switch between time series
and frequency series, go to `plot.py` and change line 9 `graphType` between `time` or `histogram`.
