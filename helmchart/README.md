# Helm Chart for "Script Runtime Service for vSphere"

## Required parameters
`vc.address` The VC address to which Script Runtime Service will be registered<br/>
`vc.user` The VC user with administrator privileges that will be used to register the service with the VC<br/>
`vc.password` The password of the `vc.user` to authorize service registration operations<br/>
`srs.service.hostname` Script Runtime Service hostname which will be used for VC registration, TLS certificate, and Ingress TLS termination rule<br/>
If VC you are registering Script RuntimeService to is with self-signed TLS certificate you need to specify its thumbprint to allow registration step to verify and trust target VC server<br/>
`vc.tls_thumbprint` It is a string value of the SHA-1 TLS certificate thumbprint.<br/>

## Script Runtime Service Images rpository and tags
Images repositry and charts and parameters of the helm chart. There are default values available in `values.yaml` which point to harbor staging vmware repository. You have to modify those to pick up images from other repositories.<br/>

## Install with helm from local disk
Download srs helm chart into local directory<br/>

### Try dry run helm chart install command to see what k8s files will be generated
``` 
helm install --dry-run --debug ./srs/ --generate-name --set vc.address='<target vc address>' --set vc.user='<vc username>' --set vc.password='<vc password>' [--set vc.tls_thumbprint='vc tls thumbprint'] --set srs.service.hostname='<script runtime service FQDN>'
```

### Run helm install command to install the service
```bash
helm install ./srs/ --generate-name --set vc.address='<target vc address>' --set vc.user='<vc username>' --set vc.password='<vc password>' [--set vc.tls_thumbprint='vc tls thumbprint'] --set srs.service.hostname='<script runtime service FQDN>'
```

#### Example
```bash
helm install ./srs/ --generate-name --set vc.address='10.23.80.118' --set vc.user='administrator@vsphere.local' --set vc.password='Admin!23' --set srs.service.hostname='srs.testdomain.com' vc.tls_thumbprint='71b11ca6e4861c86f74f33f802aa43f6c9e62f56'
```

## Uninstall
To unsintall use `helm delete` command. SRS helm chart runs service unregister on uninstall to leave the VC clean.<br/>
Take a look at template/ses-unregister-on-delete.yaml which runs the job to unregister from VC.<br/>
Helm dlete command syntax is:<br/>
```bash
helm delete <helm release name>
```

### Example
```bash
helm delete srs-1602231
```

## Use helm upgrade to update service settings
You can update service settings editined the helm values.yaml file and callin `helm upgrade` commaned. This should be used whe you want to update the tls cerficitae as well.
Helm upgrade is going to update the VC registration iwth update TLS cerficate.

Helm upgrade syntax is:
```bash
helm upgrade <helm release name> ./srs/ --set vc.address='<target vc address>' --set vc.user='<vc username>' --set vc.password='<vc password>' [--set vc.tls_thumbprint='vc tls thumbprint'] --set srs.service.hostname='<script runtime service FQDN>'
```

### Example
```bash
helm upgrade script-runtime5 ./srs/ --set vc.address='10.23.80.118' --set vc.user='administrator@vsphere.local' --set vc.password='Admin!23' --set srs.service.hostname='srs.testdomain.com' --set vc.tls_thumbprint='71b11ca6e4861c86f74f33f802aa43f6c9e62f56'
```

To test tls update on vc registration you can generate new self-signed certificate and run helm upgrade<br/>

### Example
```bash
openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout srs-tls.key -out srs-tls.crt -subj "/CN=srs.wcp.lab/O=srs.wcp.lab"
kubectl delete secret tls srs-tls -n script-runtime-service
kubectl create secret tls srs-tls --key srs-tls.key --cert srs-tls.crt -n script-runtime-service
helm upgrade script-runtime6 ./srs/ --set vc.address='10.23.80.118' --set vc.user='administrator@vsphere.local' --set vc.password='Admin!23' --set srs.service.hostname='srs.testdomain.com' --set vc.tls_thumbprint='71b11ca6e4861c86f74f33f802aa43f6c9e62f56'
```