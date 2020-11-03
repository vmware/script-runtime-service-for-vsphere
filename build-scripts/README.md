
# Build helper scripts

## How to build the Runspace image, and updated it SRS VM deployment

### Prerequisites

* Build machine where you build the SRS OVA
* SRS VM with running SRS

### Build the Runspace image

Use `build-scripts/build-pclirunspace-image.sh` script on the build machine<br/>

It accepts three arguments
1. "PowerCLI Modules Directory". This argument is mandatory. This is a directory with PowerShell modules that will be included in the runspace image. PowerCLI modules are required to be there but optionally other PowerShell modules can be added in this directory. All modules from the specified directory will be available in SRS runspaces.<br/>
2. "Image Export Dir". This argument is mandatory. Image is exported as a .tar file on local machine. Specify destination folder where image file will be saved.<br/>
3. "SRS VM IP address". This argument is optional. When specified the script runs `scp` command to copy the output runspace image on the SRS VM. Target directory on the SRS VM is `/root/`. `scp` command will prompt for `root` password of the SRS VM.<br/>

Usage:<br/>
```bash
./build-scripts/build-pclirunspace-image.sh <powercli modules dir> <target image export dir> [<SRS VM IP>]
```

Example:<br/>
```bash
 ./build-scripts/build-pclirunspace-image.sh ~/PowerCLIModules/ ~/ 10.23.82.191
```

### Update Runspace on SRS VM
SRS OVA build have prepared a shell script file to update the Runspace image. 
1. On the SRS VM check update runspace script is available<br/>
```
/root/update-pclirunspace-image.sh
```

If it is not available on your SRS VM you have to copy it from the repository file `appliance/files/update-pclirunspace-image.sh`

2. The new runspace image .tar file should be available on the SRS VM in
```bash
/root/pclirunspace-docker-image.tar
```
This should be the result of build when "SRS VM IP address" is provided.<br/> 
If it is not available copy the image to this directory.

3. Run `/root/update-pclirunspace-image.sh`