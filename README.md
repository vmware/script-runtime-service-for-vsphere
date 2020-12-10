
# Script Runtime Service for vSphere (SRS)

## Overview

Script Runtime Service for vSphere (SRS) enables vSphere users and services (clients) to manage PowerCLI instances and run PowerCLI scripts. SRS clients authenticate once with vSphere credentials or access token. SRS clients create PowerCLI instances and run scripts within. PowerCLI runs server-side and automatically connects to the target vCenter Servers. SRS tracks history of script outputs.

### Highlights
* Central place with REST API endpoint for VI Admins to run [VMware PowerCLI](https://code.vmware.com/web/tool/12.1.0/vmware-powercli)
* Manage multiple PowerCLI instances to run commands and scripts against vCenter Servers without calling Connect-VIServer
* Runs on Kubernetes

![SRS Overview](https://github.com/vmware/script-runtime-service-for-vsphere/blob/master/doc/assets/img/SRSOverview.jpg?raw=true)

## Latest Release
[Script Runtime Service for vSphere 1.0.0](https://github.com/vmware/script-runtime-service-for-vsphere/releases/tag/v1.0.0)

## Istall SRS
[Install on a VM](https://github.com/vmware/script-runtime-service-for-vsphere/wiki/Getting-Started-with-SRSHostVM)<br/>
[Install on a Kubernetes Cluster](https://github.com/vmware/script-runtime-service-for-vsphere/wiki/Install-SRS)

## User Guide
[User Guide](https://github.com/vmware/script-runtime-service-for-vsphere/wiki/Home)

## SRS API
[Getting Started with SRS API](https://github.com/vmware/script-runtime-service-for-vsphere/wiki/Getting-Started-with-SRS-API)

## SRS API Client-side SDKs
[Swagger Codegen](https://github.com/swagger-api/swagger-codegen) can be used to generate client side SDKs for different lnaguages.<br/>
Java and C# example client appliactions based on automatic generate client-side SDKs are available in [OpenAPI Clients](https://github.com/vmware/script-runtime-service-for-vsphere/tree/master/test/openapi-clients).<br/>

## Build & Run
[Build & Run](https://github.com/vmware/script-runtime-service-for-vsphere/blob/master/BUILD_AND_RUN.md)

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
