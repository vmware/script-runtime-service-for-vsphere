
# Script Runtime Service for vSphere (SRS)

## Overview

Script Runtime Service for vSphere (SRS) enables vSphere users and services (clients) to manage PowerCLI instances and run PowerCLI scripts. SRS clients authenticate once with vSphere credentials or access token. SRS clients create PowerCLI instances and run scripts within. PowerCLI runs server-side and automatically connects to the target vCenter Servers. SRS tracks history of script outputs.

### Highlights
* Central place with REST API endpoint for VI Admins in an organization to run [VMware PowerCLI](https://code.vmware.com/web/tool/12.1.0/vmware-powercli)
* Run PowerCLI without having it installed on local machine
* Manage multiple PowerCLI instances and run commands and scripts within against vSphere infrastructure without calling Connect-VIServer
* Runs on Kubernetes

![SRS Overview](https://github.com/vmware/script-runtime-service-for-vsphere/blob/master/doc/assets/img/SRSOverview.jpg?raw=true)

## Build & Run
Script Runtime Service is a kubernetes application but since it is vSphere add-on service you can build and package it in a Photon OS OVF virtual machine from source code. Build uses Photon OS appliance templates from [William Lam](https://github.com/lamw) github repository [photon os appliance](https://github.com/lamw/photonos-appliance)  modified with custom properties that take care to register SRS to desired vCenter Server on first boot. Thus, you can have SRS deployed and configured with simple ovf deploy.

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

## SRS API
API definition is available on `https://<SRS Address>/swagger`. Swagger UI hosted here is the easiest way to try the API.<br/>

![Swagger UI](https://github.com/vmware/script-runtime-service-for-vsphere/blob/master/doc/assets/img/SwaggerUI.JPG?raw=true)

### 1 Authetication
SRS uses VC SSO as Identity and Authentication Server. Two types of authentication are supported. SIGN and Basic. SIGN authentication is purposed for service to service access to SRS resources. For the end-users SRS supports basic authentication passing username and password which are used only to acquire HoK Saml token for SRS solution. When basic is used SRS uses usename and password to present them to SSO server and exchange them for SAML token. SRS uses the SAML token to Connect PowerCLI to VC services in further operations. <br/>
On successful authentication SRS issues its own Authorization token which MUST be used to authorize further SRS API calls.<br />

`POST https://<SRS IP>/api/auth/login`

In Swagger UI use `Authorize` button to provide username and password for basic autentication, and then use the `Try it out` button of `/api/auth/login` to `Execute` login operation.

Example basic authentication request:<br/>
```bash
curl -X POST "https://10.23.81.245/api/auth/login" -H "accept: \*/\*" -H "Authorization: Basic YWRt..."
```

`Response:`
```bash
HTTP/1.1 200 OK
content-length: 0
date: Wed, 03 Jun 2020 11:49:50 GMT
server: nginx/1.17.10
status: 200
strict-transport-security: max-age=15724800; includeSubDomains
x-srs-api-key: b3133982-93ea-4f92-ba03-2e122c1e0fd8
```

`x-srs-api-key` header contains the issued authorization token it should be used to authorize further SRS API calls<br/>

In Swagger UI having the API KEY go `Authorize` to remove `basicAuth` and to provide `apiKeyAuth` with the value returned by `login` operation. After that you can try to create runspace and request script execution following guidelines below.<br/>

### 2 Create Runspace (PowerShell instance running in SRS)

`POST https://<SRS IP>/api/runspaces`

`Example:`
```bash
curl -X POST "https://10.23.81.245/api/runspaces" -H "accept: application/json" -H "X-SRS-API-KEY: b3133982-93ea-4f92-ba03-2e122c1e0fd8" -H "Content-Type: application/json" -d "{\"name\":\"MyRunspace\",\"run_vc_connection_script\":true}"
```

`Response:`
```bash
content-length: 225
content-type: application/json; charset=utf-8
date: Wed, 03 Jun 2020 11:54:29 GMT
server: nginx/1.17.10
status: 202
strict-transport-security: max-age=15724800; includeSubDomains
```
```json
{
  "name": "MyRunspace",
  "id": "pcli-3e21e354-e59a-4fcb-a355-0bab10908cd1",
  "state": "creating",
  "error_details": null,
  "run_vc_connection_script": true,
  "vc_connection_script_id": null,
  "creation_time": "2020-06-03T11:54:29.6271143+00:00"
}
```

### 2.1 Get Runspace and check it's state is 'Ready'

`GET https://<SRS IP>/api/runspaces/{id}`

`Example:`
```bash
curl -X GET "https://10.23.81.245/api/runspaces/pcli-3e21e354-e59a-4fcb-a355-0bab10908cd1" -H "accept: application/json" -H "X-SRS-API-KEY: b3133982-93ea-4f92-ba03-2e122c1e0fd8"
```

`Response:`
```bash
 content-encoding: gzip
 content-type: application/json; charset=utf-8
 date: Wed, 03 Jun 2020 11:58:41 GMT
 server: nginx/1.17.10
 status: 200
 strict-transport-security: max-age=15724800; includeSubDomains
 vary: Accept-Encoding
 ```
```json
{
  "name": "MyRunspace",
  "id": "pcli-3e21e354-e59a-4fcb-a355-0bab10908cd1",
  "state": "ready",
  "error_details": null,
  "run_vc_connection_script": true,
  "vc_connection_script_id": "51b3ee0e-8c66-49c3-a1e5-4b448024a581",
  "creation_time": "2020-06-03T11:58:07.4530423+00:00"
}
```

### 3 Run PowerShell script

`POST https://<SRS IP>/api/script-executions`

```json
{
    "runspase_id":"&lt;runspace id&gt;",
    "script":"Get-VIAccount"
}
```

`Example:`
```bash
curl -X POST "https://10.23.81.245/api/script-executions" -H "accept: application/json" -H "X-SRS-API-KEY: b3133982-93ea-4f92-ba03-2e122c1e0fd8" -H "Content-Type: application/json" -d "{\"runspace_id\":\"pcli-3e21e354-e59a-4fcb-a355-0bab10908cd1\",\"name\":\"get vi accounts\",\"script\":\"Get-VIAccount\"}"
```

`Response:`
```bash
 content-length: 256 <br/>
 content-type: application/json; charset=utf-8 <br/>
 date: Wed, 03 Jun 2020 12:01:28 GMT <br/>
 server: nginx/1.17.10 <br/>
 status: 202
 strict-transport-security: max-age=15724800; includeSubDomains
```
```json
{
  "id": "c6927736-84db-4e53-8e72-e8d0c95dca2b",
  "name": "get vi accounts",   
  "output_objects_format": "text",
  "state": "running",
  "start_time": "2020-06-03T12:01:28.9273601+00:00"
}
```

### 4 Get script's status

`GET https://<SRS IP>/api/script-executions/{script-id}`

`Example:`

```bash
curl -X GET "https://10.23.81.245/api/script-executions/c6927736-84db-4e53-8e72-e8d0c95dca2b" -H "accept: application/json" -H "X-SRS-API-KEY: b3133982-93ea-4f92-ba03-2e122c1e0fd8"
```

`Response:`
```bash
 content-encoding: gzip
 content-type: application/json; charset=utf-8
 date: Wed, 03 Jun 2020 12:30:40 GMT
 server: nginx/1.17.10
 status: 200
 strict-transport-security: max-age=15724800; includeSubDomains
 vary: Accept-Encoding 
 ```
 ```json
{
  "id": "c6927736-84db-4e53-8e72-e8d0c95dca2b",
  "runspace_id": null,
  "name": "get vi accounts",
  "script": null,
  "script_parameters": null,
  "output_objects_format": "text",
  "state": "success",
  "reason": null,
  "start_time": "2020-06-03T12:01:28.9273601+00:00",
  "end_time": "2020-06-03T12:01:29.9480608+00:00"
}
```

### 5 Get script's output

`GET https://<SRS IP>/api/script-executions/{script-id}/output`

`Example:`

```bash
curl -X GET "https://10.23.81.245/api/script-executions/c6927736-84db-4e53-8e72-e8d0c95dca2b/output" -H "accept: application/json" -H "X-SRS-API-KEY: b3133982-93ea-4f92-ba03-2e122c1e0fd8"
```

`Response:`
```bash
 content-encoding: gzip
 content-type: application/json; charset=utf-8
 date: Wed, 03 Jun 2020 12:32:25 GMT
 server: nginx/1.17.10
 status: 200
 strict-transport-security: max-age=15724800; includeSubDomains
 vary: Accept-Encoding
```
```json
[
  "Name                 Domain               Description        Server<br/>
  ----                 ------               -----------         ------<br/>
  sshd                                      sshd PrivSep       10.23.80.118<br/>
  eam                                       eam                10.23.80.118<br/>"
]
```

## SRS API Client-side SDKs
[Swagger Codegen](https://github.com/swagger-api/swagger-codegen) can be used to generate client side SDKs for different lnaguages.<br/>
Java and C# example client appliactions based on automatic generate client-side SDKs are available in [OpenAPI Clients](https://github.com/vmware/script-runtime-service-for-vsphere/tree/master/test/openapi-clients).<br/>

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
