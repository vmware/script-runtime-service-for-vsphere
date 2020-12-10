# Build & Run
Script Runtime Service is a Kubernetes application but since it is vSphere add-on service you can build and package it in a Photon OS OVF virtual machine from source code. Build uses Photon OS appliance templates from [William Lam](https://github.com/lamw) github repository [photon os appliance](https://github.com/lamw/photonos-appliance)  modified with custom properties that take care to register SRS to desired vCenter Server on first boot. Thus, you can have SRS deployed and configured with simple ovf deploy.

Result appliance packages [Photon OS](https://vmware.github.io/photon/), [docker](https://docs.docker.com/engine/install/), [kind](https://kind.sigs.k8s.io/) kubernetes with  [NGINX Ingress Controller](https://kubernetes.github.io/ingress-nginx/), and the Script Runtime Service for vSphere K8s appliaction deployed in `script-runtime-service` namespace.<br/>

If you login on the deployed result VM `kubectl` is availble to browse the kubernetes cluster.<br/>

![SRS Appliance](https://github.com/vmware/script-runtime-service-for-vsphere/blob/master/doc/assets/img/appliance.jpg?raw=true)

### Prerequisites

* MacOS or Linux Desktop
* vCenter Server or Standalone ESXi host 6.x or greater
* [dotnet sdk](https://docs.microsoft.com/en-us/dotnet/core/install/)
* [docker](https://docs.docker.com/engine/install/)
* [PowerShell 7.0](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7)
* [VMware PowerCLI](https://code.vmware.com/web/tool/12.1.0/vmware-powercli)
* [VMware OVFTool](https://www.vmware.com/support/developer/ovf/)
* [Packer](https://www.packer.io/docs/install)


### How to build and run SRS

> `packer` builds the OVF on a remote ESXi host via the [`vmware-iso`](https://www.packer.io/docs/builders/vmware-iso.html) builder. This builder requires the SSH service running on the ESXi host, as well as `GuestIPHack` enabled via the command below.
```bash
esxcli system settings advanced set -o /Net/GuestIPHack -i 1
```

1. Edit the `photon-builder.json` file to configure the vSphere endpoint for building the SRS appliance

```json
{
  "builder_host": "192.168.30.10",
  "builder_host_username": "root",
  "builder_host_password": "VMware1!",
  "builder_host_datastore": "vsanDatastore",
  "builder_host_portgroup": "VM Network"
}
```

**Note:** If you need to change the initial root password on the SRS appliance, take a look at `photon-version.json` and `http/photon-kickstart.json`. When the OVF is produced, there is no default password, so this does not really matter other than for debugging purposes.

2. Start the build by running the [build.sh script](https://github.com/vmware/script-runtime-service-for-vsphere/blob/master/build.sh)  which builds Script Runtime Service containers and then calls `packer` and the respective build files

```
./build.sh <PowerCLI Modules path>
````

3. Deploy SRS from OVF on vSphere with [deploy.ps1](https://github.com/vmware/script-runtime-service-for-vsphere/blob/master/appliance/deploy.ps1) PowerShell script which basically edits the OvfConfig settings by given parameters and deploys a VM from the OVA.

4. Test SRS API is available on `https://<SRS VM IP>/swagger`.


## Contributing

The script-runtime-service-for-vsphere project team welcomes contributions from the community. If you wish to contribute code and you have not signed our contributor license agreement (CLA), our bot will update the issue when you open a Pull Request. For any questions about the CLA process, please refer to our [FAQ](https://cla.vmware.com/faq). For more detailed information, refer to [CONTRIBUTING.md](CONTRIBUTING.md).

## Join us on Slack

The repo is in very early stage for contributors. A lot of documentation is pending to be created. Until it is done you can use the script-runtime-service-assist channel on VMware Code slack

1. Join [VMware Code](https://code.vmware.com/web/code/join)
2. Join the following channel:
    ```
    script-runtime-service-assist
    ```

## License
Script Runtime Service for vSphere is distributed under the [Apache 2.0](https://github.com/vmware/script-runtime-service-for-vsphere/blob/master/LICENSE.txt).

For more details, please refer to the [Apache 2.0 License File](https://github.com/vmware/script-runtime-service-for-vsphere/blob/master/LICENSE.txt).
