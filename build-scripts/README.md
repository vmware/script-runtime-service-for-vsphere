
# Build helper scripts

## How to build the SRS images  and update the SRS deployment on the SRS hosting VM

### Prerequisites

* Build machine where you build the SRS OVA
* SRS VM with running SRS

### Build and Update SRS deployment

Use the `./build-scripts/buildanddeploy.sh <PowerShell Modules directory> <SRS VM IP> <SRS VM User> <SRS VM Password>` script on the build machine<br/>

#### Arguments
1. "PowerShell Modules Directory". This argument is mandatory. This is a directory with PowerShell modules that will be included in the runspace image. PowerCLI modules are required to be there but optionally other PowerShell modules can be added in this directory. All modules from the specified directory will be available in SRS runspaces.<br/>
2. "SRS VM IP". This argument is mandatory. The script copies the produced images on the SRS VM.<br/>
3. "SRS VM User". This argument is mandatory. The script copies the produced images on the SRS VM using ssh with user and password authentication. The username is need to copy the files to the target SRS VM<br/>
4. "SRS VM Password". This argument is mandatory. The password for the SRS VM User<br/>

Usage:<br/>
```bash
./build-scripts/buildanddeploy.sh <PowerShell Modules directory> <SRS VM IP> <SRS VM User> <SRS VM Password
```

Example:<br/>
```bash
 ./build-scripts/buildanddeploy.sh ~/PowerCLIModules/ 10.23.82.191 root 'Pa$$w0rd'
 ```
