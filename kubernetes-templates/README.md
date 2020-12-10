# Kuberenetes templates helpers to manage "Script Runtime Service for vSphere" deployment

SRS is a Kubernetes deployment that manages its registration to VC Server with Kubernetes jobs. The purpose of these Kubernetes templates helpers is to manage Kubernetes deployment without helm chart. A good example is if you delete the SRS Kubernetes resources without using `helm delete` which runs a job to remove the SRS VC registration. In this case SRS deployment would be removed but the VC registration will remain in the target VC server. You can use a template to create Kubernetes job that performs SRS VC registration cleanup.

## srs-vc-registration-cleanup.yaml
A template with Kubernetes job that performs VC registration cleanup. It runs a job from the srs-setup container which performs SRS registration cleanup actions. The job is created in the `default` runspaces. In order to run that you need to replace the template values for
1. image name and image tag
2. VC address
3. VC user
4. VC password

### Example target yaml file
apiVersion: batch/v1
kind: Job
metadata:
  name: "srs-vc-registration-cleanup"
spec:
  template:
    spec:
      containers:
      - image: srs-setup:1.0
        name: srs-setup
        imagePullPolicy: IfNotPresent
        command: ["/app/service/setup"]
        env:
          - name: RUN
            value: CleanupVCRegistration
          - name: TARGET_VC_SERVER
            value: "10.23.80.118"
          - name: TARGET_VC_USER
            value: "administrator@vsphere.local"
          - name: TARGET_VC_PASSWORD
            value: "Pa$$w0rd"
          - name: TARGET_VC_INSECURE
            value: "True"
      restartPolicy: Never


### To run the job use
`kubect apply -f srs-vc-registration-cleanup.yaml`

### Once the job is completed you can delete the Kubernetes resources created by the job with
`kubect apply -f srs-vc-registration-cleanup.yaml`

## srs-https-hostname-ingress.yaml
When SRS is packaged in an OVF with the build procedure, the VM deployed from the OVF edits the `appliance/files/srs-app-template.yaml` Kubernetes file on first boot. This template defines the SRS ingress resource in a way the ingress controller doesn't rout by VM hostname rather by IP. The reason for that is to allow accessing the SRS service by IP. 

If you set up a FQDN for the SRS IP address, the Ingress resource needs modification to configure traffic routing from the Ingress controller to the SRS Server API Gateway pod using the FQDN. This modification can be applied manually on the deployed resource, or it can be applied using the `srs-https-hostname-ingress.yaml` template.<br/>

1. Edit the deployed Ingress resource on the VM.
 - Log in the SRS VM
 - Run `kubectl edit ingress -n script-runtime-service`
 - VIM editor is opened with the Ingress resource definition. Replace the following content
 ```yaml
 rules:
  - host: < Your FQDN >
  - http:
      paths:
      - path: /
        backend:
          serviceName: srs-apigateway
          servicePort: 5050
 ```
 with 
```yaml
 rules:
  - host: < Your FQDN >
    http:
      paths:
      - path: /
        backend:
          serviceName: srs-apigateway
          servicePort: 5050
 ```
 2. Use the  `srs-https-hostname-ingress.yaml` template
  - Edit the template file replacing `${SRSA_HOSTNAME}` with the desired FQDN 
  - Run `kubectl apply -f srs-https-hostname-ingress.yaml`